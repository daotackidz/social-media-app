using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.Post;
using Social.Data.Model.Story;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Notification
{
    [Table("notifications")]
    [Index(nameof(UserId), nameof(IsRead))]
    public class Notifications : BaseRecordModel
    {
        public enum NotificationType
        {
            Follow = 1,
            LikePost = 2,
            CommentPost = 3,
            ReplyComment = 4,
            LikeComment = 5,
            SharePost = 6,
            Mention = 7,

            /// <summary>A private account received a new follow request (not yet accepted/rejected).</summary>
            FollowRequest = 8,

            /// <summary>The recipient's own follow request to ActorUserId was accepted.</summary>
            FollowAccepted = 9,
            LikeStory = 10,

            /// <summary>A reply was sent to the recipient's story.</summary>
            CommentStory = 11
        }

        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id")]
        [Display(Name = "Người nhận")]
        public Guid UserId { get; set; }       // 👈 người nhận

        [Column("actor_user_id")]
        [Display(Name = "Người gây ra")]
        public Guid ActorUserId { get; set; }  // 👈 người gây ra (like, follow...)

        [Column("notification_type")]
        [Display(Name = "Loại thông báo")]
        public NotificationType Type { get; set; }

        [Column("post_id")]
        public Guid? PostId { get; set; }

        [Column("post_comment_id")]
        public Guid? PostCommentId { get; set; }

        [Column("story_id")]
        public Guid? StoryId { get; set; }

        [Column("data")]
        public string? Data { get; set; } // JSON mở rộng (optional)

        [Column("is_read")]
        public bool IsRead { get; set; }

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? User { get; set; }

        [ForeignKey(nameof(ActorUserId))]
        public Users? ActorUser { get; set; }

        [ForeignKey(nameof(PostId))]
        public Posts? Posts { get; set; }

        [ForeignKey(nameof(PostCommentId))]
        public PostComments? PostComments { get; set; }

        [ForeignKey(nameof(StoryId))]
        public Stories? Stories { get; set; }

        #endregion

    }
}
