using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.Post;
using Social.Data.Model.Response.Profile;
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

            var isFollowing = !isCurrentUser && CurrentUserId.HasValue
                && await _relationRepository.IsFollowingAsync(CurrentUserId.Value, user.Id);

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

            await _relationRepository.FollowAsync(CurrentUserId.Value, user.Id);
            var followersCount = await _relationRepository.CountFollowersAsync(user.Id);

            return ApiOk(new { isFollowing = true, followersCount }, "Đã theo dõi.");
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

            return ApiOk(new { isFollowing = false, followersCount }, "Đã bỏ theo dõi.");
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
                CommentCount = post.CommentCount
            };
        }

        #endregion
    }
}
