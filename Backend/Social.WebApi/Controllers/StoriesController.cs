using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.File;
using Social.Data.Model.Notification;
using Social.Data.Model.Request.Story;
using Social.Data.Model.Response.Base;
using Social.Data.Model.Response.Story;
using Social.Repository.Social.Notification.Interface;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.Story.Interface;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;
using StoryEntity = Social.Data.Model.Story.Stories;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Creating a story (single image/video + caption, up to 15s of video — enforced client-side
    /// where the file is actually recorded/picked) and the story viewer overlay: one owner's reel,
    /// like/reply, and the owner-only "who viewed / who liked" list.
    /// </summary>
    [Route("api/stories")]
    [ApiController]
    [Authorize]
    public class StoriesController : BaseApiController
    {
        private readonly IStoryRepository _storyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFileService _fileService;

        public StoriesController(
            IStoryRepository storyRepository,
            IUserRepository userRepository,
            IUserRelationRepository relationRepository,
            INotificationRepository notificationRepository,
            IFileService fileService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _storyRepository = storyRepository;
            _userRepository = userRepository;
            _relationRepository = relationRepository;
            _notificationRepository = notificationRepository;
            _fileService = fileService;
        }

        private const int NotificationPreviewLength = 100;

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(500 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] CreateStoryRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            if (request.File is null || request.File.Length == 0)
            {
                return ApiBadRequest("Cần chọn một ảnh hoặc video.", ErrorCode.FILE_REQUIRED);
            }

            var contentType = request.File.ContentType ?? string.Empty;
            var isVideo = contentType.StartsWith("video/");
            if (!contentType.StartsWith("image/") && !isVideo)
            {
                return ApiBadRequest("Chỉ hỗ trợ tệp ảnh hoặc video.", ErrorCode.VALIDATION_ERROR);
            }

            var userId = CurrentUserId.Value;
            var url = await _fileService.UploadFileAsync(request.File, "storyfiles");

            var fileMeta = new Files
            {
                Id = Guid.NewGuid(),
                FileName = Path.GetFileName(url),
                FileNameOrigin = request.File.FileName,
                FileExtension = Path.GetExtension(request.File.FileName),
                FileType = isVideo ? (ushort)2 : (ushort)1,
                FileSize = request.File.Length,
                StoragePath = url,
                StorageType = 1,
                IsPublic = true,
                FileVersion = false,
                CreatedByUserId = userId
            };
            await _userRepository.AddFileAsync(fileMeta);

            var story = new StoryEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileId = fileMeta.Id,
                Caption = request.Caption?.Trim() ?? string.Empty,
                IsAiGenerated = request.IsAiGenerated,
                CreatedByUserId = userId
            };
            await _storyRepository.CreateAsync(story);

            var owner = await _userRepository.GetUsersByIdsAsync(new[] { userId });
            owner.TryGetValue(userId, out var ownerUser);
            var ownerAvatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { userId });

            return ApiCreated(BuildResponse(story, fileMeta, ownerUser?.UserName, ownerAvatar.TryGetValue(userId, out var avatarUrl) ? avatarUrl : null, isLiked: false, isOwnStory: true), "Đăng tin thành công.");
        }

        /// <summary>
        /// One story by id — used to resolve a notification's StoryId to its owner before opening
        /// the full reel (GetByUser), since a notification only ever names the story, not its owner's id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var story = await _storyRepository.GetByIdAsync(id);
            if (story is null || story.Files is null || !await CanViewStoryAsync(story))
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            var owner = await _userRepository.GetUsersByIdsAsync(new[] { story.UserId });
            owner.TryGetValue(story.UserId, out var ownerUser);
            var ownerAvatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { story.UserId });
            var avatarUrl = ownerAvatar.TryGetValue(story.UserId, out var url) ? url : null;

            var isLiked = CurrentUserId.HasValue && (await _storyRepository.GetLikedStoryIdsAsync(new[] { id }, CurrentUserId.Value)).Contains(id);
            var isOwnStory = CurrentUserId == story.UserId;

            return ApiOk(BuildResponse(story, story.Files, ownerUser?.UserName, avatarUrl, isLiked, isOwnStory));
        }

        /// <summary>
        /// Every story the signed-in user has ever posted, regardless of expiry, newest first — "Kho
        /// lưu trữ" on their own profile. Always self: there's no owner-not-found or privacy case,
        /// since nobody else can ever see this list.
        /// </summary>
        [HttpGet("archive")]
        public async Task<IActionResult> GetArchive([FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var stories = await _storyRepository.GetArchivedByUserIdAsync(CurrentUserId.Value, skip, take + 1);
            var hasMore = stories.Count > take;
            if (hasMore) stories = stories.Take(take).ToList();

            var owner = await _userRepository.GetUsersByIdsAsync(new[] { CurrentUserId.Value });
            owner.TryGetValue(CurrentUserId.Value, out var ownerUser);
            var ownerAvatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { CurrentUserId.Value });
            var avatarUrl = ownerAvatar.TryGetValue(CurrentUserId.Value, out var url) ? url : null;

            var items = stories
                .Where(s => s.Files is not null)
                .Select(s => BuildResponse(s, s.Files!, ownerUser?.UserName, avatarUrl, isLiked: false, isOwnStory: true))
                .ToList();

            return ApiOk(new PagedResponse<StoryItemResponse> { Items = items, HasMore = hasMore });
        }

        /// <summary>One owner's full reel — the story viewer plays these in order, oldest first.</summary>
        [HttpGet("by-user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            if (!await CanViewAsync(userId))
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            var stories = await _storyRepository.GetActiveByUserIdAsync(userId);
            if (stories.Count == 0)
            {
                return ApiOk(new List<StoryItemResponse>());
            }

            var owner = await _userRepository.GetUsersByIdsAsync(new[] { userId });
            owner.TryGetValue(userId, out var ownerUser);
            var ownerAvatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { userId });
            var avatarUrl = ownerAvatar.TryGetValue(userId, out var url) ? url : null;

            var likedIds = CurrentUserId.HasValue
                ? await _storyRepository.GetLikedStoryIdsAsync(stories.Select(s => s.Id), CurrentUserId.Value)
                : new HashSet<Guid>();
            var isOwnStory = CurrentUserId == userId;

            var items = stories
                .Where(s => s.Files is not null)
                .Select(s => BuildResponse(s, s.Files!, ownerUser?.UserName, avatarUrl, likedIds.Contains(s.Id), isOwnStory))
                .ToList();

            return ApiOk(items);
        }

        [HttpPost("{id:guid}/view")]
        public async Task<IActionResult> MarkViewed(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var story = await _storyRepository.GetByIdAsync(id);
            if (story is null || !await CanViewStoryAsync(story))
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            var ok = await _storyRepository.MarkViewedAsync(id, CurrentUserId.Value);
            if (!ok) return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);

            return ApiOk<object?>(null);
        }

        [HttpPost("{id:guid}/like")]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var story = await _storyRepository.GetByIdAsync(id);
            if (story is null || !await CanViewStoryAsync(story))
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            var result = await _storyRepository.ToggleLikeAsync(id, CurrentUserId.Value);
            if (result is null) return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);

            if (result.Value.Liked && story.UserId != CurrentUserId.Value)
            {
                await _notificationRepository.UpsertLikeNotificationAsync(new Notifications
                {
                    Id = Guid.NewGuid(),
                    UserId = story.UserId,
                    ActorUserId = CurrentUserId.Value,
                    Type = Notifications.NotificationType.LikeStory,
                    StoryId = id,
                    CreatedByUserId = CurrentUserId.Value
                });
            }

            return ApiOk(new StoryLikeResponse { Liked = result.Value.Liked, LikeCount = result.Value.LikeCount });
        }

        [HttpPost("{id:guid}/reply")]
        public async Task<IActionResult> Reply(Guid id, [FromBody] AddStoryReplyRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var story = await _storyRepository.GetByIdAsync(id);
            if (story is null || !await CanViewStoryAsync(story))
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            var reply = await _storyRepository.AddReplyAsync(id, CurrentUserId.Value, request.Content.Trim());

            if (story.UserId != CurrentUserId.Value)
            {
                var preview = reply.Content.Length > NotificationPreviewLength ? reply.Content[..NotificationPreviewLength] : reply.Content;
                await _notificationRepository.CreateAsync(new Notifications
                {
                    Id = Guid.NewGuid(),
                    UserId = story.UserId,
                    ActorUserId = CurrentUserId.Value,
                    Type = Notifications.NotificationType.CommentStory,
                    StoryId = id,
                    Data = preview,
                    CreatedByUserId = CurrentUserId.Value
                });
            }

            return ApiCreated(new StoryReplyResponse { Id = reply.Id, Content = reply.Content, CreatedDate = reply.CreatedDate }, "Đã gửi trả lời.");
        }

        /// <summary>Who viewed (and who also liked) this story — visible only to the story's own owner.</summary>
        [HttpGet("{id:guid}/viewers")]
        public async Task<IActionResult> GetViewers(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var story = await _storyRepository.GetByIdAsync(id);
            if (story is null || story.UserId != CurrentUserId.Value)
            {
                return ApiNotFound("Không tìm thấy tin.", ErrorCode.STORY_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var viewers = await _storyRepository.GetViewersAsync(id, skip, take + 1);
            var hasMore = viewers.Count > take;
            if (hasMore) viewers = viewers.Take(take).ToList();

            var viewerIds = viewers.Select(v => v.UserId).ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(viewerIds);
            var profilesById = await _userRepository.GetProfilesByUserIdsAsync(viewerIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(viewerIds);

            var items = viewers.Select(v =>
            {
                usersById.TryGetValue(v.UserId, out var user);
                profilesById.TryGetValue(v.UserId, out var profile);
                return new StoryViewerResponse
                {
                    UserId = v.UserId,
                    Username = user?.UserName ?? string.Empty,
                    FullName = profile?.FullName,
                    AvatarUrl = avatars.TryGetValue(v.UserId, out var avatarUrl) ? avatarUrl : null,
                    ViewedDate = v.ViewedDate,
                    Liked = v.Liked
                };
            }).ToList();

            return ApiOk(new PagedResponse<StoryViewerResponse> { Items = items, HasMore = hasMore });
        }

        /// <summary>Same rule PostsController/ProfileViewController use: a private owner's content is visible only to themselves and accepted followers.</summary>
        private async Task<bool> CanViewAsync(Guid ownerUserId)
        {
            if (CurrentUserId == ownerUserId) return true;

            var profile = await _userRepository.GetProfileByUserIdAsync(ownerUserId);
            if (profile?.IsPrivate != true) return true;

            return CurrentUserId.HasValue && await _relationRepository.IsFollowingAsync(CurrentUserId.Value, ownerUserId);
        }

        /// <summary>
        /// Same as CanViewAsync, plus the 24h expiry rule: once a story has expired, only its own
        /// owner may ever look at it again (via the archive) — everyone else is turned away exactly
        /// like the story no longer exists, private-account rules or not.
        /// </summary>
        private async Task<bool> CanViewStoryAsync(StoryEntity story)
        {
            if (CurrentUserId == story.UserId) return true;
            if (story.ExpiresDate <= DateTime.UtcNow) return false;

            return await CanViewAsync(story.UserId);
        }

        private static StoryItemResponse BuildResponse(StoryEntity story, Files file, string? username, string? avatarUrl, bool isLiked, bool isOwnStory)
            => new()
            {
                Id = story.Id,
                UserId = story.UserId,
                Username = username ?? string.Empty,
                AvatarUrl = avatarUrl,
                MediaUrl = file.StoragePath,
                MediaType = file.FileType == 2 ? "video" : "image",
                Caption = story.Caption,
                IsAiGenerated = story.IsAiGenerated,
                CreatedDate = story.CreatedDate,
                LikeCount = story.LikeCount,
                ViewCount = story.ViewCount,
                IsLiked = isLiked,
                IsOwnStory = isOwnStory
            };
    }
}
