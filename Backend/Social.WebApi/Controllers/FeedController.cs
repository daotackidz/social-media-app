using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Post;
using Social.Data.Model.Response.Base;
using Social.Data.Model.Response.Feed;
using Social.Data.Model.User;
using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.Story.Interface;
using Social.Repository.Social.User.Interface;
using Social.WebApi.Infrastructure.Services;
using PostFileType = Social.Data.Model.Post.PostFiles.PostFileType;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Backs the home feed's infinite scroll: posts and stories from the caller
    /// plus everyone they follow, paged with skip/take so a first paint never
    /// has to wait on the whole history. "hasMore" on each page tells the
    /// client whether to keep scrolling or show the "you're all caught up" state.
    /// </summary>
    [Route("api/feed")]
    [ApiController]
    [Authorize]
    public class FeedController : BaseApiController
    {
        private const int DefaultPostsTake = 10;
        private const int MaxPostsTake = 30;
        private const int DefaultStoriesTake = 20;
        private const int MaxStoriesTake = 50;
        private const int DefaultSuggestionsTake = 8;
        private const int MaxSuggestionsTake = 20;

        private readonly IPostRepository _postRepository;
        private readonly IStoryRepository _storyRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly IUserRepository _userRepository;

        public FeedController(
            IPostRepository postRepository,
            IStoryRepository storyRepository,
            IUserRelationRepository relationRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _postRepository = postRepository;
            _storyRepository = storyRepository;
            _relationRepository = relationRepository;
            _userRepository = userRepository;
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts([FromQuery] int skip = 0, [FromQuery] int take = DefaultPostsTake)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, MaxPostsTake);

            var ownerIds = await GetFeedOwnerIdsAsync(CurrentUserId.Value);
            var page = await _postRepository.GetFeedPageAsync(ownerIds, skip, take + 1);
            var hasMore = page.Count > take;
            var posts = page.Take(take).ToList();

            var ownerUserIds = posts.Select(p => p.UserId).Distinct().ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(ownerUserIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(ownerUserIds);

            var items = posts.Select(p =>
            {
                var images = (p.PostFiles ?? Enumerable.Empty<PostFiles>())
                    .Where(f => f.FileType == PostFileType.Image && f.Files is not null)
                    .OrderByDescending(f => f.IsPrimary)
                    .ThenBy(f => f.DisplayOrder)
                    .ThenBy(f => f.CreatedDate)
                    .Select(f => f.Files!.StoragePath)
                    .ToList();

                usersById.TryGetValue(p.UserId, out var user);

                return new FeedPostResponse
                {
                    Id = p.Id,
                    Username = user?.UserName ?? string.Empty,
                    Verified = user?.IsVerified ?? false,
                    AvatarUrl = avatars.TryGetValue(p.UserId, out var avatarUrl) ? avatarUrl : null,
                    ImageUrls = images,
                    Caption = p.Caption,
                    LikeCount = p.LikeCount,
                    CommentCount = p.CommentCount,
                    CreatedDate = p.CreatedDate
                };
            }).ToList();

            return ApiOk(new PagedResponse<FeedPostResponse> { Items = items, HasMore = hasMore });
        }

        [HttpGet("stories")]
        public async Task<IActionResult> GetStories([FromQuery] int skip = 0, [FromQuery] int take = DefaultStoriesTake)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, MaxStoriesTake);

            var ownerIds = await GetFeedOwnerIdsAsync(CurrentUserId.Value);
            var page = await _storyRepository.GetFeedPageAsync(ownerIds, CurrentUserId.Value, skip, take + 1);
            var hasMore = page.Count > take;
            var groups = page.Take(take).ToList();

            var groupOwnerIds = groups.Select(g => g.UserId).ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(groupOwnerIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(groupOwnerIds);

            var items = groups.Select(g =>
            {
                usersById.TryGetValue(g.UserId, out var user);
                return new FeedStoryResponse
                {
                    UserId = g.UserId,
                    Username = user?.UserName ?? string.Empty,
                    AvatarUrl = avatars.TryGetValue(g.UserId, out var avatarUrl) ? avatarUrl : null,
                    Viewed = g.Viewed
                };
            }).ToList();

            return ApiOk(new PagedResponse<FeedStoryResponse> { Items = items, HasMore = hasMore });
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] int take = DefaultSuggestionsTake)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            take = Math.Clamp(take, 1, MaxSuggestionsTake);

            var candidates = await _relationRepository.GetSuggestionsAsync(CurrentUserId.Value, take);

            var candidateIds = candidates.Select(c => c.UserId).ToList();
            var reasonUserIds = candidates.Where(c => c.ReasonUserId.HasValue).Select(c => c.ReasonUserId!.Value).Distinct().ToList();

            var usersById = await _userRepository.GetUsersByIdsAsync(candidateIds);
            var profilesById = await _userRepository.GetProfilesByUserIdsAsync(candidateIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(candidateIds);
            var reasonUsersById = await _userRepository.GetUsersByIdsAsync(reasonUserIds);

            var items = candidates.Select(c =>
            {
                usersById.TryGetValue(c.UserId, out var user);
                profilesById.TryGetValue(c.UserId, out var profile);
                Users? reasonUser = null;
                if (c.ReasonUserId.HasValue) reasonUsersById.TryGetValue(c.ReasonUserId.Value, out reasonUser);

                return new FeedSuggestedUserResponse
                {
                    UserId = c.UserId,
                    Username = user?.UserName ?? string.Empty,
                    FullName = profile?.FullName,
                    AvatarUrl = avatars.TryGetValue(c.UserId, out var avatarUrl) ? avatarUrl : null,
                    Reason = c.Reason,
                    ReasonUsername = reasonUser?.UserName
                };
            }).ToList();

            return ApiOk(items);
        }

        /// <summary>Self + everyone the caller actively follows — the audience for their home feed.</summary>
        private async Task<List<Guid>> GetFeedOwnerIdsAsync(Guid userId)
        {
            var following = await _relationRepository.GetFollowingUserIdsAsync(userId);
            following.Add(userId);
            return following.Distinct().ToList();
        }
    }
}
