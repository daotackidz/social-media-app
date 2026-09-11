using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.Request.Search;
using Social.Data.Model.Response.Search;
using Social.Data.Model.User;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.Search.Interface;
using Social.Repository.Social.User.Interface;
using Social.WebApi.Infrastructure.Services;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Backs the search popup: live user search plus the signed-in user's own
    /// "Recent" list. History rows are soft-deleted (RecordStatusId = Deleted) —
    /// never removed from the table — by <see cref="DeleteHistory"/> / <see cref="ClearHistory"/>.
    /// </summary>
    [Route("api/search")]
    [ApiController]
    [Authorize]
    public class SearchController : BaseApiController
    {
        private const int MaxResults = 20;
        private const int MaxHistory = 20;

        private readonly IUserRepository _userRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly ISearchHistoryRepository _searchHistoryRepository;

        public SearchController(
            IUserRepository userRepository,
            IUserRelationRepository relationRepository,
            ISearchHistoryRepository searchHistoryRepository,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _userRepository = userRepository;
            _relationRepository = relationRepository;
            _searchHistoryRepository = searchHistoryRepository;
        }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            if (string.IsNullOrWhiteSpace(q)) return ApiOk(new List<SearchUserResponse>());

            var users = await _userRepository.SearchUsersAsync(q, CurrentUserId, MaxResults);
            var result = await ToSearchUserResponsesAsync(users);

            return ApiOk(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var history = await _searchHistoryRepository.GetRecentAsync(CurrentUserId.Value, MaxHistory);
            var targetUsers = history.Where(h => h.TargetUsers is not null).Select(h => h.TargetUsers!).ToList();
            var userResponses = (await ToSearchUserResponsesAsync(targetUsers)).ToDictionary(u => u.UserId);

            var result = history
                .Where(h => userResponses.ContainsKey(h.TargetUserId))
                .Select(h => new SearchHistoryResponse { Id = h.Id, User = userResponses[h.TargetUserId] })
                .ToList();

            return ApiOk(result);
        }

        [HttpPost("history")]
        public async Task<IActionResult> AddHistory([FromBody] AddSearchHistoryRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            await _searchHistoryRepository.AddOrTouchAsync(CurrentUserId.Value, request.TargetUserId);
            return ApiOk<object?>(null, "Đã lưu lịch sử tìm kiếm.");
        }

        [HttpDelete("history/{id:guid}")]
        public async Task<IActionResult> DeleteHistory(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var deleted = await _searchHistoryRepository.SoftDeleteAsync(CurrentUserId.Value, id);
            if (!deleted)
            {
                return ApiNotFound("Không tìm thấy lịch sử tìm kiếm.", ErrorCode.SEARCH_HISTORY_NOT_FOUND);
            }

            return ApiOk<object?>(null, "Đã xoá lịch sử tìm kiếm.");
        }

        [HttpDelete("history")]
        public async Task<IActionResult> ClearHistory()
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            await _searchHistoryRepository.SoftDeleteAllAsync(CurrentUserId.Value);
            return ApiOk<object?>(null, "Đã xoá tất cả lịch sử tìm kiếm.");
        }

        #region Private helpers

        private async Task<List<SearchUserResponse>> ToSearchUserResponsesAsync(List<Users> users)
        {
            if (users.Count == 0) return new List<SearchUserResponse>();

            var userIds = users.Select(u => u.Id).ToList();
            var profiles = await _userRepository.GetProfilesByUserIdsAsync(userIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(userIds);

            var result = new List<SearchUserResponse>();
            foreach (var user in users)
            {
                var isFollowing = CurrentUserId.HasValue
                    && await _relationRepository.IsFollowingAsync(CurrentUserId.Value, user.Id);

                result.Add(new SearchUserResponse
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    FullName = profiles.TryGetValue(user.Id, out var profile) ? profile.FullName : user.UserName,
                    AvatarUrl = avatars.TryGetValue(user.Id, out var avatarUrl) ? avatarUrl : null,
                    Verified = user.IsVerified ?? false,
                    IsFollowing = isFollowing
                });
            }

            return result;
        }

        #endregion
    }
}
