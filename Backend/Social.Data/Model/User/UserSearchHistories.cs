using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    /// <summary>
    /// One "recent search" entry — a user searched for / opened another user's
    /// profile from the search popup. RecordStatusId drives visibility: rows are
    /// soft-deleted (set to Deleted) by the "x" / "Clear all" actions in the popup,
    /// never removed from the table.
    /// </summary>
    [Table("user_search_histories")]
    public class UserSearchHistories : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        /// <summary>Owner of this search history entry — who searched.</summary>
        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        /// <summary>The user that was searched for / clicked on.</summary>
        [Column("target_user_id", Order = 2)]
        public Guid TargetUserId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        [ForeignKey(nameof(TargetUserId))]
        public Users? TargetUsers { get; set; }

        #endregion
    }
}
