using Microsoft.AspNetCore.Http;

namespace Social.Data.Model.Request.Messaging
{
    public class SendMessageRequest
    {
        /// <summary>Text body. May be empty when File carries the whole message (a photo/video/voice note with no caption).</summary>
        public string? Content { get; set; }

        /// <summary>Optional attachment — image, video, or a recorded voice note (audio/*). Its content type decides the message's Type.</summary>
        public IFormFile? File { get; set; }

        /// <summary>Set to quote another message in this conversation (the "Trả lời" feature).</summary>
        public Guid? ReplyToMessageId { get; set; }
    }
}
