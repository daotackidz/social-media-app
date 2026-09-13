using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.File;
using Social.Data.Model.Request.Messaging;
using Social.Data.Model.Response.Base;
using Social.Data.Model.Response.Messaging;
using Social.Repository.Social.Messaging.Interface;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;
using MessageEntity = Social.Data.Model.Messaging.Messages;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Direct messages: the conversation list, starting (or resuming) a 1-1 thread, and sending/
    /// reading messages within one — text, or an image/video/voice-note attachment. There's no
    /// realtime transport here; the thread view polls GetMessages while it's open.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public MessagesController(
            IConversationRepository conversationRepository,
            IUserRepository userRepository,
            IFileService fileService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _fileService = fileService;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations([FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 50);

            var summaries = await _conversationRepository.GetConversationsForUserAsync(CurrentUserId.Value, skip, take + 1);
            var hasMore = summaries.Count > take;
            if (hasMore) summaries = summaries.Take(take).ToList();

            var otherIds = summaries.Select(s => s.OtherUserId).Distinct().ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(otherIds);
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(otherIds);

            var items = summaries.Select(s => ToResponse(s, usersById, avatars)).ToList();

            return ApiOk(new PagedResponse<ConversationResponse> { Items = items, HasMore = hasMore });
        }

        /// <summary>Gets or creates the 1-1 thread with the given username — the "Tin nhắn mới" picker's "Chat" action.</summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();

            var otherUser = await _userRepository.GetByUserNameAsync(request.Username);
            if (otherUser is null) return ApiNotFound("Không tìm thấy người dùng.", ErrorCode.USERNAME_NOT_FOUND);
            if (otherUser.Id == CurrentUserId.Value) return ApiBadRequest("Không thể nhắn tin cho chính mình.", ErrorCode.VALIDATION_ERROR);

            var conversationId = await _conversationRepository.GetOrCreateDirectConversationAsync(CurrentUserId.Value, otherUser.Id);
            var avatar = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { otherUser.Id });

            return ApiOk(new ConversationResponse
            {
                Id = conversationId,
                OtherUserId = otherUser.Id,
                Username = otherUser.UserName,
                AvatarUrl = avatar.TryGetValue(otherUser.Id, out var url) ? url : null,
                HasUnread = false
            });
        }

        [HttpGet("conversations/{id:guid}")]
        public async Task<IActionResult> GetConversation(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            if (!await _conversationRepository.IsParticipantAsync(id, CurrentUserId.Value))
            {
                return ApiNotFound("Không tìm thấy cuộc trò chuyện.", ErrorCode.CONVERSATION_NOT_FOUND);
            }

            var otherUserId = await _conversationRepository.GetOtherParticipantIdAsync(id, CurrentUserId.Value);
            if (!otherUserId.HasValue) return ApiNotFound("Không tìm thấy cuộc trò chuyện.", ErrorCode.CONVERSATION_NOT_FOUND);

            var otherLastReadAt = await _conversationRepository.GetLastReadAtAsync(id, otherUserId.Value);
            var usersById = await _userRepository.GetUsersByIdsAsync(new[] { otherUserId.Value });
            var avatars = await _userRepository.GetPrimaryAvatarUrlsByUserIdsAsync(new[] { otherUserId.Value });

            var summary = new ConversationSummary { ConversationId = id, OtherUserId = otherUserId.Value, OtherLastReadAt = otherLastReadAt };
            return ApiOk(ToResponse(summary, usersById, avatars));
        }

        [HttpGet("conversations/{id:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int skip = 0, [FromQuery] int take = 30)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            if (!await _conversationRepository.IsParticipantAsync(id, CurrentUserId.Value))
            {
                return ApiNotFound("Không tìm thấy cuộc trò chuyện.", ErrorCode.CONVERSATION_NOT_FOUND);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var messages = await _conversationRepository.GetMessagesAsync(id, skip, take + 1);
            var hasMore = messages.Count > take;
            if (hasMore) messages = messages.Take(take).ToList();

            var replyToIds = messages.Where(m => m.ReplyToMessageId.HasValue).Select(m => m.ReplyToMessageId!.Value).Distinct().ToList();
            var replyToById = await _conversationRepository.GetMessagesByIdsAsync(replyToIds);

            var senderIds = messages.Select(m => m.SenderUserId)
                .Concat(replyToById.Values.Select(m => m.SenderUserId))
                .Distinct()
                .ToList();
            var usersById = await _userRepository.GetUsersByIdsAsync(senderIds);
            var reactionsByMessage = await _conversationRepository.GetReactionsAsync(messages.Select(m => m.Id));

            var items = messages.Select(m => ToMessageResponse(m, usersById, reactionsByMessage, replyToById)).ToList();

            return ApiOk(new PagedResponse<MessageResponse> { Items = items, HasMore = hasMore });
        }

        [HttpPost("conversations/{id:guid}/messages")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(200 * 1024 * 1024)]
        public async Task<IActionResult> SendMessage(Guid id, [FromForm] SendMessageRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            if (!await _conversationRepository.IsParticipantAsync(id, CurrentUserId.Value))
            {
                return ApiNotFound("Không tìm thấy cuộc trò chuyện.", ErrorCode.CONVERSATION_NOT_FOUND);
            }

            var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();
            if (content is null && request.File is null)
            {
                return ApiBadRequest("Tin nhắn không được để trống.", ErrorCode.VALIDATION_ERROR);
            }

            MessageEntity? replyTo = null;
            if (request.ReplyToMessageId.HasValue)
            {
                replyTo = await _conversationRepository.GetMessageByIdAsync(request.ReplyToMessageId.Value);
                if (replyTo is null || replyTo.ConversationId != id)
                {
                    return ApiBadRequest("Không tìm thấy tin nhắn để trả lời.", ErrorCode.VALIDATION_ERROR);
                }
            }

            var userId = CurrentUserId.Value;
            Guid? fileId = null;
            var type = MessageEntity.MessageType.Text;

            if (request.File is not null)
            {
                var contentType = request.File.ContentType ?? string.Empty;
                if (contentType.StartsWith("video/")) type = MessageEntity.MessageType.Video;
                else if (contentType.StartsWith("audio/")) type = MessageEntity.MessageType.Audio;
                else if (contentType.StartsWith("image/")) type = MessageEntity.MessageType.Image;
                else return ApiBadRequest("Chỉ hỗ trợ ảnh, video hoặc ghi âm.", ErrorCode.VALIDATION_ERROR);

                var url = await _fileService.UploadFileAsync(request.File, "messagefiles");
                var fileMeta = new Files
                {
                    Id = Guid.NewGuid(),
                    FileName = Path.GetFileName(url),
                    FileNameOrigin = request.File.FileName,
                    FileExtension = Path.GetExtension(request.File.FileName),
                    FileType = type == MessageEntity.MessageType.Video ? (ushort)2 : type == MessageEntity.MessageType.Audio ? (ushort)3 : (ushort)1,
                    FileSize = request.File.Length,
                    StoragePath = url,
                    StorageType = 1,
                    IsPublic = true,
                    FileVersion = false,
                    CreatedByUserId = userId
                };
                await _userRepository.AddFileAsync(fileMeta);
                fileId = fileMeta.Id;
            }

            var message = await _conversationRepository.AddMessageAsync(id, userId, content, type, fileId, request.ReplyToMessageId);

            var senderIds = replyTo is not null ? new[] { userId, replyTo.SenderUserId } : new[] { userId };
            var usersById = await _userRepository.GetUsersByIdsAsync(senderIds);
            var replyToById = replyTo is not null
                ? new Dictionary<Guid, MessageEntity> { [replyTo.Id] = replyTo }
                : new Dictionary<Guid, MessageEntity>();

            return ApiCreated(ToMessageResponse(message, usersById, new Dictionary<Guid, List<MessageReactionInfo>>(), replyToById), "Đã gửi.");
        }

        [HttpPost("conversations/{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            await _conversationRepository.MarkReadAsync(id, CurrentUserId.Value);
            return ApiOk<object?>(null);
        }

        /// <summary>
        /// Sets the caller's reaction on messageId: a new emoji, swapping for a different one, or
        /// clearing it entirely if they pick the same one again. Returns that message's full reaction list.
        /// </summary>
        [HttpPost("conversations/{id:guid}/messages/{messageId:guid}/react")]
        public async Task<IActionResult> ReactToMessage(Guid id, Guid messageId, [FromBody] ReactToMessageRequest request)
        {
            if (!CurrentUserId.HasValue) return ApiUnauthorized();
            if (!await _conversationRepository.IsParticipantAsync(id, CurrentUserId.Value))
            {
                return ApiNotFound("Không tìm thấy cuộc trò chuyện.", ErrorCode.CONVERSATION_NOT_FOUND);
            }

            await _conversationRepository.ToggleReactionAsync(messageId, CurrentUserId.Value, request.Emoji);

            var reactions = await _conversationRepository.GetReactionsAsync(new[] { messageId });
            var items = reactions.TryGetValue(messageId, out var list)
                ? list.Select(r => new MessageReactionResponse { UserId = r.UserId, Emoji = r.Emoji }).ToList()
                : new List<MessageReactionResponse>();

            return ApiOk(items);
        }

        private static ConversationResponse ToResponse(ConversationSummary summary, Dictionary<Guid, Social.Data.Model.User.Users> usersById, Dictionary<Guid, string> avatars)
        {
            usersById.TryGetValue(summary.OtherUserId, out var user);
            return new ConversationResponse
            {
                Id = summary.ConversationId,
                OtherUserId = summary.OtherUserId,
                Username = user?.UserName ?? string.Empty,
                AvatarUrl = avatars.TryGetValue(summary.OtherUserId, out var avatarUrl) ? avatarUrl : null,
                LastMessagePreview = summary.LastMessagePreview,
                LastMessageType = summary.LastMessageType,
                LastMessageAt = summary.LastMessageAt,
                HasUnread = summary.HasUnread,
                OtherLastReadAt = summary.OtherLastReadAt
            };
        }

        private static MessageResponse ToMessageResponse(
            MessageEntity message,
            Dictionary<Guid, Social.Data.Model.User.Users> usersById,
            Dictionary<Guid, List<MessageReactionInfo>> reactionsByMessage,
            Dictionary<Guid, MessageEntity>? replyToById = null)
        {
            usersById.TryGetValue(message.SenderUserId, out var sender);
            var reactions = reactionsByMessage.TryGetValue(message.Id, out var list)
                ? list.Select(r => new MessageReactionResponse { UserId = r.UserId, Emoji = r.Emoji }).ToList()
                : new List<MessageReactionResponse>();

            MessageReplyPreviewResponse? replyTo = null;
            if (message.ReplyToMessageId.HasValue && (replyToById?.TryGetValue(message.ReplyToMessageId.Value, out var quoted) ?? false))
            {
                usersById.TryGetValue(quoted.SenderUserId, out var quotedSender);
                replyTo = new MessageReplyPreviewResponse
                {
                    Id = quoted.Id,
                    SenderUserId = quoted.SenderUserId,
                    SenderUsername = quotedSender?.UserName ?? string.Empty,
                    Content = quoted.Content,
                    Type = quoted.Type,
                    FileUrl = quoted.Files?.StoragePath
                };
            }

            return new MessageResponse
            {
                Id = message.Id,
                SenderUserId = message.SenderUserId,
                SenderUsername = sender?.UserName ?? string.Empty,
                Content = message.Content,
                Type = message.Type,
                FileUrl = message.Files?.StoragePath,
                CreatedDate = message.CreatedDate,
                Reactions = reactions,
                ReplyTo = replyTo
            };
        }
    }
}
