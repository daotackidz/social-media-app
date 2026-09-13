using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.File;
using Social.Data.Model.Notification;
using Social.Data.Model.Request.Post;
using Social.Data.Model.Response.Base;
using Social.Data.Model.Response.Post;
using Social.Data.Model.Response.Profile;
using Social.Repository.Social.Notification.Interface;
using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;
using PostEntity = Social.Data.Model.Post.Posts;
using PostFileEntity = Social.Data.Model.Post.PostFiles;
using PostFileType = Social.Data.Model.Post.PostFiles.PostFileType;

namespace Social.WebApi.Controllers
{
    /// <summary>Creating a post (upload media + caption), and the "open post" detail overlay (media + caption + comments) reused by both the profile grid and the home feed.</summary>
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class PostsController : BaseApiController
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFileService _fileService;

        public PostsController(
            IPostRepository postRepository,
            IUserRepository userRepository,
            IUserRelationRepository relationRepository,
            INotificationRepository notificationRepository,
            IFileService fileService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _relationRepository = relationRepository;
            _notificationRepository = notificationRepository;
            _fileService = fileService;
        }

        /// <summary>Truncated preview stored on a comment/reply notification, so the popup can show a snippet without a second lookup.</summary>
        private const int NotificationPreviewLength = 100;

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(500 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest request)
        {
            if (!IsCurrentUserAuthenticated || CurrentUserId is null)
            {
                return ApiUnauthorized();
            }

            if (request.Files is null || request.Files.Count == 0)
            {
                return ApiBadRequest("Cần chọn ít nhất một ảnh hoặc video.", ErrorCode.FILE_REQUIRED);
            }

            foreach (var file in request.Files)
            {
                var contentType = file.ContentType ?? string.Empty;
                if (!contentType.StartsWith("image/") && !contentType.StartsWith("video/"))
                {
                    return ApiBadRequest("Chỉ hỗ trợ tệp ảnh hoặc video.", ErrorCode.VALIDATION_ERROR);
                }
            }

            var userId = CurrentUserId.Value;

            var post = new PostEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Caption = request.Caption?.Trim() ?? string.Empty,
                Content = request.Caption?.Trim() ?? string.Empty,
                Privacy = PostEntity.PostPrivacy.Public,
                IsAiGenerated = request.IsAiGenerated,
                CreatedByUserId = userId
            };

            var postFiles = new List<PostFileEntity>();
            string? coverUrl = null;

            // The first video's client-captured frame (if the client sent one) stands in as
            // the cover instead of the video's own row — a raw video URL can't back an <img>,
            // which is why video posts had no working thumbnail in the grid before this existed.
            var firstVideoIndex = -1;
            for (var j = 0; j < request.Files.Count; j++)
            {
                if ((request.Files[j].ContentType ?? string.Empty).StartsWith("video/"))
                {
                    firstVideoIndex = j;
                    break;
                }
            }
            var hasThumbnailForFirstVideo = firstVideoIndex >= 0 && request.Thumbnail is not null;

            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var isVideo = (file.ContentType ?? string.Empty).StartsWith("video/");
                var isPrimary = i == 0 && !(isVideo && i == firstVideoIndex && hasThumbnailForFirstVideo);

                var url = await _fileService.UploadFileAsync(file, "postfiles");
                if (i == 0 && !hasThumbnailForFirstVideo) coverUrl = url;

                var fileMeta = new Files
                {
                    Id = Guid.NewGuid(),
                    FileName = Path.GetFileName(url),
                    FileNameOrigin = file.FileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileType = isVideo ? (ushort)2 : (ushort)1,
                    FileSize = file.Length,
                    StoragePath = url,
                    StorageType = 1,
                    IsPublic = true,
                    FileVersion = false,
                    CreatedByUserId = userId
                };
                await _userRepository.AddFileAsync(fileMeta);

                var altText = i < request.AltTexts.Count ? request.AltTexts[i]?.Trim() : null;

                postFiles.Add(new PostFileEntity
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    FileId = fileMeta.Id,
                    FileType = isVideo ? PostFileEntity.PostFileType.Video : PostFileEntity.PostFileType.Image,
                    IsPrimary = isPrimary,
                    DisplayOrder = i,
                    AltText = string.IsNullOrWhiteSpace(altText) ? null : altText,
                    CreatedByUserId = userId
                });

                if (i == firstVideoIndex && hasThumbnailForFirstVideo)
                {
                    var thumbnailUrl = await _fileService.UploadFileAsync(request.Thumbnail!, "postfiles");
                    coverUrl = thumbnailUrl;

                    var thumbnailFileMeta = new Files
                    {
                        Id = Guid.NewGuid(),
                        FileName = Path.GetFileName(thumbnailUrl),
                        FileNameOrigin = request.Thumbnail!.FileName,
                        FileExtension = Path.GetExtension(thumbnailUrl),
                        FileType = (ushort)1,
                        FileSize = request.Thumbnail.Length,
                        StoragePath = thumbnailUrl,
                        StorageType = 1,
                        IsPublic = true,
                        FileVersion = false,
                        CreatedByUserId = userId
                    };
                    await _userRepository.AddFileAsync(thumbnailFileMeta);

                    postFiles.Add(new PostFileEntity
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        FileId = thumbnailFileMeta.Id,
                        FileType = PostFileEntity.PostFileType.Image,
                        IsPrimary = true,
                        DisplayOrder = i,
                        CreatedByUserId = userId
                    });
                }
            }

            await _postRepository.CreateAsync(post, postFiles);

            // A generated thumbnail adds an extra Image row alongside its video, so "carousel"
            // is decided by the actual attachment count (request.Files.Count), not postFiles.Count.
            var type = firstVideoIndex >= 0
                ? "video"
                : request.Files.Count > 1 ? "carousel" : "image";

            return ApiCreated(new ProfilePostResponse
            {
                Id = post.Id,
                Type = type,
                CoverUrl = coverUrl,
                LikeCount = 0,
                CommentCount = 0,
                IsAiGenerated = post.IsAiGenerated
            }, "Đăng bài viết thành công.");
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post is null)
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            if (!await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            var owner = await _userRepository.GetUsersByIdsAsync(new[] { post.UserId });
            owner.TryGetValue(post.UserId, out var ownerUser);
            var ownerAvatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { post.UserId });

            // Only the first page of top-level comments loads with the post — the rest (and every
            // reply thread) is paged in on demand from the comments panel to avoid shipping a
            // potentially huge comment tree on every "open post" click.
            const int commentPageSize = 10;
            var comments = await _postRepository.GetTopLevelCommentsAsync(id, 0, commentPageSize + 1);
            var commentsHasMore = comments.Count > commentPageSize;
            if (commentsHasMore) comments = comments.Take(commentPageSize).ToList();

            var commentResponses = await MapCommentsAsync(comments);
            var isLiked = CurrentUserId.HasValue && await _postRepository.IsLikedByUserAsync(id, CurrentUserId.Value);

            return ApiOk(BuildDetailResponse(post, ownerUser, ownerAvatar, commentResponses, commentsHasMore, isLiked));
        }

        [HttpPost("{id:guid}/like")]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            var result = await _postRepository.ToggleLikeAsync(id, CurrentUserId.Value);
            if (result is null)
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            // Only the "like" side notifies (not unlike), and never yourself. Upserting instead of
            // always inserting means repeatedly unliking/reliking the same post doesn't pile up
            // duplicate rows for the same person.
            if (result.Value.Liked && post.UserId != CurrentUserId.Value)
            {
                await _notificationRepository.UpsertLikeNotificationAsync(new Notifications
                {
                    Id = Guid.NewGuid(),
                    UserId = post.UserId,
                    ActorUserId = CurrentUserId.Value,
                    Type = Notifications.NotificationType.LikePost,
                    PostId = id,
                    CreatedByUserId = CurrentUserId.Value
                });
            }

            return ApiOk(new PostLikeResponse { Liked = result.Value.Liked, LikeCount = result.Value.LikeCount });
        }

        [HttpGet("{id:guid}/likes")]
        public async Task<IActionResult> GetLikes(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var likerIds = await _postRepository.GetLikerUserIdsAsync(id, skip, take + 1);
            return ApiOk(await BuildLikeUserPageAsync(likerIds, take));
        }

        [HttpGet("{id:guid}/comments/{commentId:guid}/likes")]
        public async Task<IActionResult> GetCommentLikes(Guid id, Guid commentId, [FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            var comment = await _postRepository.GetCommentByIdAsync(commentId);
            if (comment is null || comment.PostId != id)
            {
                return ApiNotFound("Không tìm thấy bình luận.", ErrorCode.POST_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var likerIds = await _postRepository.GetCommentLikerUserIdsAsync(commentId, skip, take + 1);
            return ApiOk(await BuildLikeUserPageAsync(likerIds, take));
        }

        /// <summary>
        /// Shared by the post's and each comment's "Lượt thích" popup: likerIds is fetched take+1
        /// deep by the caller so HasMore can be read off the count here without a second query.
        /// </summary>
        private async Task<PagedResponse<PostLikeUserResponse>> BuildLikeUserPageAsync(List<Guid> likerIds, int take)
        {
            var hasMore = likerIds.Count > take;
            if (hasMore) likerIds = likerIds.Take(take).ToList();

            var usersById = await _userRepository.GetUsersByIdsAsync(likerIds);
            var profilesById = await _userRepository.GetProfilesByUserIdsAsync(likerIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(likerIds);
            var followingSet = CurrentUserId.HasValue
                ? await _relationRepository.GetFollowingSetAsync(CurrentUserId.Value, likerIds)
                : new HashSet<Guid>();

            var items = likerIds.Select(userId =>
            {
                usersById.TryGetValue(userId, out var user);
                profilesById.TryGetValue(userId, out var profile);
                return new PostLikeUserResponse
                {
                    UserId = userId,
                    Username = user?.UserName ?? string.Empty,
                    FullName = profile?.FullName,
                    AvatarUrl = avatars.TryGetValue(userId, out var avatarUrl) ? avatarUrl : null,
                    IsCurrentUser = CurrentUserId == userId,
                    IsFollowing = followingSet.Contains(userId)
                };
            }).ToList();

            return new PagedResponse<PostLikeUserResponse> { Items = items, HasMore = hasMore };
        }

        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 50);

            var comments = await _postRepository.GetTopLevelCommentsAsync(id, skip, take + 1);
            var hasMore = comments.Count > take;
            if (hasMore) comments = comments.Take(take).ToList();

            return ApiOk(new PagedResponse<PostCommentResponse> { Items = await MapCommentsAsync(comments), HasMore = hasMore });
        }

        [HttpGet("{id:guid}/comments/{commentId:guid}/replies")]
        public async Task<IActionResult> GetReplies(Guid id, Guid commentId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 50);

            var replies = await _postRepository.GetRepliesAsync(commentId, skip, take + 1);
            var hasMore = replies.Count > take;
            if (hasMore) replies = replies.Take(take).ToList();

            return ApiOk(new PagedResponse<PostCommentResponse> { Items = await MapCommentsAsync(replies), HasMore = hasMore });
        }

        [HttpPost("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            Guid? parentId = null;
            if (request.ParentId.HasValue)
            {
                var parent = await _postRepository.GetCommentByIdAsync(request.ParentId.Value);
                if (parent is null || parent.PostId != id)
                {
                    return ApiNotFound("Không tìm thấy bình luận.", ErrorCode.POST_NOT_FOUND);
                }

                // Keep replies flat (one level): replying to a reply re-parents to that reply's own top-level comment.
                parentId = parent.ParentID ?? parent.Id;
            }

            var comment = await _postRepository.AddCommentAsync(id, CurrentUserId.Value, request.Content.Trim(), parentId);
            var mapped = (await MapCommentsAsync(new[] { comment })).First();

            if (post.UserId != CurrentUserId.Value)
            {
                var preview = comment.Content.Length > NotificationPreviewLength ? comment.Content[..NotificationPreviewLength] : comment.Content;
                await _notificationRepository.CreateAsync(new Notifications
                {
                    Id = Guid.NewGuid(),
                    UserId = post.UserId,
                    ActorUserId = CurrentUserId.Value,
                    Type = Notifications.NotificationType.CommentPost,
                    PostId = id,
                    PostCommentId = comment.Id,
                    Data = preview,
                    CreatedByUserId = CurrentUserId.Value
                });
            }

            return ApiCreated(mapped, "Đã đăng bình luận.");
        }

        [HttpPost("{id:guid}/comments/{commentId:guid}/like")]
        public async Task<IActionResult> ToggleCommentLike(Guid id, Guid commentId)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var post = await _postRepository.GetByIdAsync(id);
            if (post is null || !await CanViewAsync(post.UserId))
            {
                return ApiNotFound("Không tìm thấy bài viết.", ErrorCode.POST_NOT_FOUND);
            }

            var comment = await _postRepository.GetCommentByIdAsync(commentId);
            if (comment is null || comment.PostId != id)
            {
                return ApiNotFound("Không tìm thấy bình luận.", ErrorCode.POST_NOT_FOUND);
            }

            var result = await _postRepository.ToggleCommentLikeAsync(commentId, CurrentUserId.Value);
            if (result is null)
            {
                return ApiNotFound("Không tìm thấy bình luận.", ErrorCode.POST_NOT_FOUND);
            }

            return ApiOk(new CommentLikeResponse { Liked = result.Value.Liked, LikeCount = result.Value.LikeCount });
        }

        /// <summary>Same rule ProfileViewController uses: a private owner's content is visible only to themselves and accepted followers.</summary>
        private async Task<bool> CanViewAsync(Guid ownerUserId)
        {
            if (CurrentUserId == ownerUserId) return true;

            var profile = await _userRepository.GetProfileByUserIdAsync(ownerUserId);
            if (profile?.IsPrivate != true) return true;

            return CurrentUserId.HasValue && await _relationRepository.IsFollowingAsync(CurrentUserId.Value, ownerUserId);
        }

        private async Task<List<PostCommentResponse>> MapCommentsAsync(IEnumerable<Social.Data.Model.Post.PostComments> comments)
        {
            var list = comments.ToList();
            var userIds = list.Select(c => c.UserId).Distinct().ToList();
            var commentIds = list.Select(c => c.Id).ToList();

            var usersById = await _userRepository.GetUsersByIdsAsync(userIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(userIds);
            var replyCounts = await _postRepository.GetReplyCountsAsync(commentIds);
            var likedCommentIds = CurrentUserId.HasValue
                ? await _postRepository.GetLikedCommentIdsAsync(commentIds, CurrentUserId.Value)
                : new HashSet<Guid>();

            return list.Select(c =>
            {
                usersById.TryGetValue(c.UserId, out var user);
                return new PostCommentResponse
                {
                    Id = c.Id,
                    Username = user?.UserName ?? string.Empty,
                    AvatarUrl = avatars.TryGetValue(c.UserId, out var avatarUrl) ? avatarUrl : null,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    LikeCount = c.LikeCount,
                    IsLiked = likedCommentIds.Contains(c.Id),
                    ReplyCount = replyCounts.TryGetValue(c.Id, out var replyCount) ? replyCount : 0
                };
            }).ToList();
        }

        private static PostDetailResponse BuildDetailResponse(
            PostEntity post,
            Social.Data.Model.User.Users? ownerUser,
            Dictionary<Guid, string> ownerAvatars,
            List<PostCommentResponse> comments,
            bool commentsHasMore,
            bool isLiked)
        {
            var postFiles = post.PostFiles ?? Enumerable.Empty<PostFileEntity>();

            var images = postFiles
                .Where(f => f.FileType == PostFileType.Image && f.Files is not null)
                .OrderByDescending(f => f.IsPrimary)
                .ThenBy(f => f.DisplayOrder)
                .ThenBy(f => f.CreatedDate)
                .Select(f => f.Files!.StoragePath)
                .ToList();

            var videoUrl = postFiles
                .Where(f => f.FileType == PostFileType.Video && f.Files is not null)
                .OrderByDescending(f => f.IsPrimary)
                .ThenBy(f => f.DisplayOrder)
                .Select(f => f.Files!.StoragePath)
                .FirstOrDefault();

            // Same rule as the feed: a video post's image row is its auto-captured cover
            // frame, so it belongs in PosterUrl rather than the carousel's ImageUrls.
            var isVideoPost = videoUrl is not null;

            return new PostDetailResponse
            {
                Id = post.Id,
                Username = ownerUser?.UserName ?? string.Empty,
                Verified = ownerUser?.IsVerified ?? false,
                AvatarUrl = ownerAvatars.TryGetValue(post.UserId, out var avatarUrl) ? avatarUrl : null,
                ImageUrls = isVideoPost ? new List<string>() : images,
                VideoUrl = videoUrl,
                PosterUrl = isVideoPost ? images.FirstOrDefault() : null,
                Caption = post.Caption,
                Edited = post.IsEdited,
                LikeCount = post.LikeCount,
                IsLiked = isLiked,
                CommentCount = post.CommentCount,
                CreatedDate = post.CreatedDate,
                Comments = comments,
                CommentsHasMore = commentsHasMore
            };
        }
    }
}
