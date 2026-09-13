using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Messaging
{
    /// <summary>One user's reaction to one message — at most one active row per (message, user); picking a new emoji replaces it, picking the same one again removes it.</summary>
    [Table("message_reactions")]
    public class MessageReactions : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("message_id", Order = 1)]
        public Guid MessageId { get; set; }

        [Column("user_id", Order = 2)]
        public Guid UserId { get; set; }

        [Column("emoji", Order = 3)]
        public string Emoji { get; set; } = string.Empty;

        #region Related Tables

        [ForeignKey(nameof(MessageId))]
        public Messages? Messages { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
