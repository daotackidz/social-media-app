using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("user_relations")]
    public class UserRelations : BaseRecordModel
    {
        public enum UserRelationType
        {
            Follow = 1,
            Friend = 2,
            Block = 3
        }

        public enum UserRelationStatus
        {
            Pending = 1,
            Accepted = 2,
            Rejected = 3
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("follower_user_id", Order = 1)]
        public Guid FollowerUserId { get; set; }

        [Column("following_user_id", Order = 2)]
        public Guid FollowingUserId { get; set; }

        [Column("relation_type", Order = 3)]
        public UserRelationType RelationType { get; set; }

        [Column("user_relation_status", Order = 4)]
        public UserRelationStatus Status { get; set; }

        #region Related Tables

        [ForeignKey(nameof(FollowerUserId))]
        public Users? FollowerUsers { get; set; }

        [ForeignKey(nameof(FollowingUserId))]
        public Users? FollowingUsers { get; set; }

        #endregion
    }
}
