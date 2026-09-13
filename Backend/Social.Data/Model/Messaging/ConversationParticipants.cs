using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Messaging
{
    [Table("conversation_participants")]
    public class ConversationParticipants : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("conversation_id", Order = 1)]
        public Guid ConversationId { get; set; }

        [Column("user_id", Order = 2)]
        public Guid UserId { get; set; }

        /// <summary>Everything in the conversation newer than this is unread for this participant.</summary>
        [Column("last_read_at")]
        public DateTime? LastReadAt { get; set; }

        #region Related Tables

        [ForeignKey(nameof(ConversationId))]
        public Conversations? Conversations { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
