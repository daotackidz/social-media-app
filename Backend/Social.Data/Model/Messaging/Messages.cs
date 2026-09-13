using Social.Data.Model.Base;
using Social.Data.Model.File;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Messaging
{
    [Table("messages")]
    public class Messages : BaseRecordModel
    {
        public enum MessageType
        {
            Text = 1,
            Image = 2,
            Video = 3,
            Audio = 4
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("conversation_id", Order = 1)]
        public Guid ConversationId { get; set; }

        [Column("sender_user_id", Order = 2)]
        public Guid SenderUserId { get; set; }

        /// <summary>Text body — null for a pure attachment message (image/video/voice note with no caption).</summary>
        [Column("content")]
        public string? Content { get; set; }

        [Column("message_type")]
        public MessageType Type { get; set; }

        [Column("file_id")]
        public Guid? FileId { get; set; }

        /// <summary>The message this one quotes (the "Trả lời" feature) — null for an ordinary, non-reply message.</summary>
        [Column("reply_to_message_id")]
        public Guid? ReplyToMessageId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(ConversationId))]
        public Conversations? Conversations { get; set; }

        [ForeignKey(nameof(SenderUserId))]
        public Users? Sender { get; set; }

        [ForeignKey(nameof(FileId))]
        public Files? Files { get; set; }

        [ForeignKey(nameof(ReplyToMessageId))]
        public Messages? ReplyToMessage { get; set; }

        #endregion
    }
}
