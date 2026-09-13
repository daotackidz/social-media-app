using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Data.Model.Notification;
using Social.Data.Model.Post;
using Social.Data.Model.Response.Base;
using Social.Data.Model.Response.Notification;
using Social.Repository.Social.Notification.Interface;
using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.Story.Interface;
using Social.Repository.Social.User.Interface;
using Social.WebApi.Infrastructure.Services;
using PostFileType = Social.Data.Model.Post.PostFiles.PostFileType;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// The bell-icon popup: new followers, accepted/incoming follow requests, and
    /// likes/comments on the caller's own posts and stories. Likes on the same
    /// post/story from several people are grouped into one row (see GroupRaw);
    /// everything else is one row per event.
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        /// <summary>How deep into the caller's history to look before grouping — bounds the query without needing true cross-page-consistent grouping.</summary>
        private const int RawWindowSize = 300;
        private const int MaxActorsShown = 3;

        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRelationRepository _relationRepository;
        private readonly IPostRepository _postRepository;
        private readonly IStoryRepository _storyRepository;

        public NotificationsController(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IUserRelationRepository relationRepository,
            IPostRepository postRepository,
            IStoryRepository storyRepository,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _relationRepository = relationRepository;
            _postRepository = postRepository;
            _storyRepository = storyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 50);

            var raw = await _notificationRepository.GetPageAsync(CurrentUserId.Value, 0, RawWindowSize);
            var groups = GroupRaw(raw);
            var hasMore = groups.Count > skip + take;
            var page = groups.Skip(skip).Take(take).ToList();

            var actorIds = page.SelectMany(g => g.Select(n => n.ActorUserId)).Distinct().ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(actorIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(actorIds);

            var postIds = page.Where(g => g[0].PostId.HasValue).Select(g => g[0].PostId!.Value).Distinct().ToList();
            var storyIds = page.Where(g => g[0].StoryId.HasValue).Select(g => g[0].StoryId!.Value).Distinct().ToList();

            var postCovers = new Dictionary<Guid, string?>();
            foreach (var postId in postIds)
            {
                var post = await _postRepository.GetByIdAsync(postId);
                postCovers[postId] = post is null ? null : GetPostCoverUrl(post);
            }

            var storyMedia = new Dictionary<Guid, string?>();
            foreach (var storyId in storyIds)
            {
                var story = await _storyRepository.GetByIdAsync(storyId);
                storyMedia[storyId] = story?.Files?.StoragePath;
            }

            var items = new List<NotificationResponse>();
            foreach (var group in page)
            {
                var latest = group[0];
                var distinctActorIds = group.Select(n => n.ActorUserId).Distinct().ToList();
                var actors = distinctActorIds.Take(MaxActorsShown).Select(actorId =>
                {
                    usersById.TryGetValue(actorId, out var user);
                    return new NotificationActorResponse
                    {
                        UserId = actorId,
                        Username = user?.UserName ?? string.Empty,
                        AvatarUrl = avatars.TryGetValue(actorId, out var avatarUrl) ? avatarUrl : null
                    };
                }).ToList();

                bool? isFollowingActor = null;
                if (latest.Type == Notifications.NotificationType.Follow)
                {
                    isFollowingActor = await _relationRepository.IsFollowingAsync(CurrentUserId.Value, latest.ActorUserId);
                }

                items.Add(new NotificationResponse
                {
                    Id = latest.Id,
                    Type = latest.Type,
                    Actors = actors,
                    TotalActorCount = distinctActorIds.Count,
                    CommentPreview = latest.Data,
                    PostId = latest.PostId,
                    StoryId = latest.StoryId,
                    ThumbnailUrl = latest.PostId.HasValue
                        ? postCovers.GetValueOrDefault(latest.PostId.Value)
                        : latest.StoryId.HasValue ? storyMedia.GetValueOrDefault(latest.StoryId.Value) : null,
                    IsRead = latest.IsRead,
                    CreatedDate = latest.CreatedDate,
                    IsFollowingActor = isFollowingActor
                });
            }

            return ApiOk(new PagedResponse<NotificationResponse> { Items = items, HasMore = hasMore });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            var count = await _notificationRepository.GetUnreadCountAsync(CurrentUserId.Value);
            return ApiOk(new { count });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            await _notificationRepository.MarkAllReadAsync(CurrentUserId.Value);
            return ApiOk<object?>(null);
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            await _notificationRepository.MarkReadAsync(id, CurrentUserId.Value);
            return ApiOk<object?>(null);
        }

        /// <summary>
        /// raw is already newest-first; only LikePost/LikeStory notifications merge (by target),
        /// and always into the slot of their most recent occurrence, so group order stays newest-first too.
        /// </summary>
        private static List<List<Notifications>> GroupRaw(List<Notifications> raw)
        {
            var groups = new List<List<Notifications>>();
            var likeGroupIndex = new Dictionary<(Notifications.NotificationType Type, Guid? PostId, Guid? StoryId), int>();

            foreach (var n in raw)
            {
                if (n.Type is Notifications.NotificationType.LikePost or Notifications.NotificationType.LikeStory)
                {
                    var key = (n.Type, n.PostId, n.StoryId);
                    if (likeGroupIndex.TryGetValue(key, out var index))
                    {
                        groups[index].Add(n);
                        continue;
                    }
                    likeGroupIndex[key] = groups.Count;
                }

                groups.Add(new List<Notifications> { n });
            }

            return groups;
        }

        private static string? GetPostCoverUrl(Posts post)
        {
            var files = post.PostFiles ?? Enumerable.Empty<PostFiles>();
            return files
                .Where(f => f.FileType == PostFileType.Image && f.Files is not null)
                .OrderByDescending(f => f.IsPrimary)
                .ThenBy(f => f.DisplayOrder)
                .Select(f => f.Files!.StoragePath)
                .FirstOrDefault();
        }
    }
}
