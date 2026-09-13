using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Messaging
{
    /// <summary>
    /// A message thread. Only direct (1-1) conversations are created by the app today — IsGroup/Title
    /// exist so the schema doesn't need to change if group chats are added later.
    /// </summary>
    [Table("conversations")]
    public class Conversations : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("is_group")]
        public bool IsGroup { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        /// <summary>Denormalized so the conversation list can sort/page without joining into messages.</summary>
        [Column("last_message_at")]
        public DateTime? LastMessageAt { get; set; }

        #region Related Tables

        public ICollection<ConversationParticipants>? Participants { get; set; }
        public ICollection<Messages>? Messages { get; set; }

        #endregion
    }
}
