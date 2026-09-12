using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.Post;
using Social.Data.Model.Response.Profile;
using Social.Data.Model.User;
using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.User.Interface;
using Social.WebApi.Infrastructure.Services;
using static Social.Data.Model.Post.PostFiles;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Read side of a profile page (header stats, highlights, post grid) plus
    /// follow/unfollow — as opposed to <see cref="ProfileController"/>, which
    /// is the signed-in user editing their own profile.
    /// </summary>
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileViewController : BaseApiController
    {
        private const int PageSize = 30;

        private readonly IUserRepository _userRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly IPostRepository _postRepository;

        public ProfileViewController(
            IUserRepository userRepository,
            IUserRelationRepository relationRepository,
            IPostRepository postRepository,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _userRepository = userRepository;
            _relationRepository = relationRepository;
            _postRepository = postRepository;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _userRepository.GetByUserNameAsync(username);
            if (user is null)
            {
                return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            }

            var profile = await _userRepository.GetProfileByUserIdAsync(user.Id);
            var avatar = await _userRepository.GetPrimaryAvatarAsync(user.Id);
            var isCurrentUser = CurrentUserId == user.Id;

            var postsCount = await _postRepository.CountByUserIdAsync(user.Id);
            var followersCount = await _relationRepository.CountFollowersAsync(user.Id);
            var followingCount = await _relationRepository.CountFollowingAsync(user.Id);

            var relationStatus = !isCurrentUser && CurrentUserId.HasValue
                ? await _relationRepository.GetRelationStatusAsync(CurrentUserId.Value, user.Id)
                : null;
            var isFollowing = relationStatus == UserRelations.UserRelationStatus.Accepted;
            var isRequested = relationStatus == UserRelations.UserRelationStatus.Pending;

            var followedByUsername = await _relationRepository.GetFollowedByUsernameAsync(CurrentUserId, user.Id);

            return ApiOk(new ProfileResponse
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = profile?.FullName ?? user.UserName,
                Bio = profile?.Bio,
                WebsiteUrl = profile?.WebsiteUrl,
                AvatarUrl = avatar?.Files?.StoragePath,
                Verified = user.IsVerified ?? false,
                IsCurrentUser = isCurrentUser,
                IsFollowing = isFollowing,
                IsRequested = isRequested,
                IsPrivate = profile?.IsPrivate ?? false,
                FollowedByUsername = followedByUsername,
                PostsCount = postsCount,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            });
        }

        [HttpGet("{username}/highlights")]
        public async Task<IActionResult> GetHighlights(string username)
        {
            var user = await _userRepository.GetByUserNameAsync(username);
            if (user is null)
            {
                return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            }

            var highlights = await _userRepository.GetHighlightsByUserIdAsync(user.Id);

            return ApiOk(highlights.Select(h => new ProfileHighlightResponse
            {
                Id = h.Id,
                Title = h.Title,
                CoverUrl = h.CoverFile?.StoragePath
            }).ToList());
        }

        [HttpGet("{username}/posts")]
        public async Task<IActionResult> GetPosts(string username, [FromQuery] string tab = "posts")
        {
            var user = await _userRepository.GetByUserNameAsync(username);
            if (user is null)
            {
                return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            }

            var isCurrentUser = CurrentUserId == user.Id;
            if (!isCurrentUser)
            {
                var profile = await _userRepository.GetProfileByUserIdAsync(user.Id);
                if (profile?.IsPrivate == true)
                {
                    var isFollowing = CurrentUserId.HasValue
                        && await _relationRepository.IsFollowingAsync(CurrentUserId.Value, user.Id);
                    if (!isFollowing)
                    {
                        // Riêng tư và người xem chưa được duyệt follow — không lộ bài viết.
                        return ApiOk(new List<ProfilePostResponse>());
                    }
                }
            }

            var posts = tab.Equals("tagged", StringComparison.OrdinalIgnoreCase)
                ? await _postRepository.GetTaggedByUserIdAsync(user.Id, 0, PageSize)
                : await _postRepository.GetByUserIdAsync(user.Id, 0, PageSize);

            var items = posts.Select(ToPostResponse).ToList();

            // "Reels" is the same authored-posts list filtered down to single-video posts.
            if (tab.Equals("reels", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(p => p.Type == "video").ToList();
            }

            return ApiOk(items);
        }

        [HttpPost("{username}/follow")]
        public async Task<IActionResult> Follow(string username)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var user = await _userRepository.GetByUserNameAsync(username);
            if (user is null)
            {
                return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            }

            if (user.Id == CurrentUserId.Value)
            {
                return ApiBadRequest("Không thể tự theo dõi chính mình.", ErrorCode.VALIDATION_ERROR);
            }

            var profile = await _userRepository.GetProfileByUserIdAsync(user.Id);
            var requiresApproval = profile?.IsPrivate ?? false;

            var status = await _relationRepository.FollowAsync(CurrentUserId.Value, user.Id, requiresApproval);
            var isFollowing = status == UserRelations.UserRelationStatus.Accepted;
            var isRequested = status == UserRelations.UserRelationStatus.Pending;
            var followersCount = await _relationRepository.CountFollowersAsync(user.Id);

            var message = isRequested ? "Đã gửi yêu cầu theo dõi." : "Đã theo dõi.";
            return ApiOk(new { isFollowing, isRequested, followersCount }, message);
        }

        [HttpPost("{username}/unfollow")]
        public async Task<IActionResult> Unfollow(string username)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var user = await _userRepository.GetByUserNameAsync(username);
            if (user is null)
            {
                return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            }

            await _relationRepository.UnfollowAsync(CurrentUserId.Value, user.Id);
            var followersCount = await _relationRepository.CountFollowersAsync(user.Id);

            return ApiOk(new { isFollowing = false, isRequested = false, followersCount }, "Đã bỏ theo dõi.");
        }

        /// <summary>Follow requests waiting on the signed-in user's approval (their account is private).</summary>
        [HttpGet("follow-requests")]
        public async Task<IActionResult> GetFollowRequests()
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var requests = await _relationRepository.GetPendingFollowRequestsAsync(CurrentUserId.Value);
            var followerIds = requests.Select(r => r.FollowerUserId).ToList();

            var usersById = await _userRepository.GetUsersByIdsAsync(followerIds);
            var profilesById = await _userRepository.GetProfilesByUserIdsAsync(followerIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(followerIds);

            var items = requests.Select(r =>
            {
                usersById.TryGetValue(r.FollowerUserId, out var user);
                profilesById.TryGetValue(r.FollowerUserId, out var profile);

                return new FollowRequestResponse
                {
                    RelationId = r.RelationId,
                    UserId = r.FollowerUserId,
                    Username = user?.UserName ?? string.Empty,
                    FullName = profile?.FullName,
                    AvatarUrl = avatars.TryGetValue(r.FollowerUserId, out var avatarUrl) ? avatarUrl : null
                };
            }).ToList();

            return ApiOk(items);
        }

        [HttpPost("follow-requests/{relationId:guid}/approve")]
        public async Task<IActionResult> ApproveFollowRequest(Guid relationId)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var followerId = await _relationRepository.ApproveFollowRequestAsync(relationId, CurrentUserId.Value);
            if (followerId is null)
            {
                return ApiNotFound("Không tìm thấy yêu cầu theo dõi.", ErrorCode.VALIDATION_ERROR);
            }

            var followersCount = await _relationRepository.CountFollowersAsync(CurrentUserId.Value);
            return ApiOk(new { followersCount }, "Đã chấp nhận yêu cầu theo dõi.");
        }

        [HttpPost("follow-requests/{relationId:guid}/reject")]
        public async Task<IActionResult> RejectFollowRequest(Guid relationId)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var success = await _relationRepository.RejectFollowRequestAsync(relationId, CurrentUserId.Value);
            if (!success)
            {
                return ApiNotFound("Không tìm thấy yêu cầu theo dõi.", ErrorCode.VALIDATION_ERROR);
            }

            return ApiOk<object>(null!, "Đã từ chối yêu cầu theo dõi.");
        }

        #region Private helpers

        private static ProfilePostResponse ToPostResponse(Posts post)
        {
            var files = (post.PostFiles ?? Enumerable.Empty<PostFiles>())
                .OrderByDescending(f => f.IsPrimary)
                .ThenBy(f => f.DisplayOrder)
                .ThenBy(f => f.CreatedDate)
                .ToList();

            var cover = files.FirstOrDefault();

            var type = files.Count > 1
                ? "carousel"
                : cover?.FileType == PostFileType.Video ? "video" : "image";

            return new ProfilePostResponse
            {
                Id = post.Id,
                Type = type,
                CoverUrl = cover?.Files?.StoragePath,
                LikeCount = post.LikeCount,
                CommentCount = post.CommentCount,
                IsAiGenerated = post.IsAiGenerated
            };
        }

        #endregion
    }
}
