using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Post;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Notification
{
    [Table("notifications")]
    [Index(nameof(UserId), nameof(IsRead))]
    public class Notifications
    {
        public enum NotificationType
        {
            Follow = 1,
            LikePost = 2,
            CommentPost = 3,
            ReplyComment = 4,
            LikeComment = 5,
            SharePost = 6,
            Mention = 7
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

        #endregion

    }
}
