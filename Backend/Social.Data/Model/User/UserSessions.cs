using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    /// <summary>
    /// The one currently-valid session per (UserId, ClientType) — enforces
    /// "single active session per client type" (e.g. only one signed-in web
    /// browser at a time), while leaving room for a future Mobile session to
    /// stay active alongside the Web one, since each ClientType gets its own row.
    ///
    /// Every login overwrites <see cref="SessionToken"/> for that pair rather than
    /// inserting a new row. Its JWT embeds the fresh token as the "sid" claim;
    /// Program.cs's OnTokenValidated handler rejects any older token whose "sid"
    /// no longer matches this row, which is what actually signs the other
    /// session out (a stateless JWT can't otherwise be revoked before it expires).
    /// </summary>
    [Table("user_sessions")]
    [Index(nameof(UserId), nameof(ClientType), IsUnique = true)]
    public class UserSessions : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }

        [Column("client_type", Order = 2)]
        public ClientType ClientType { get; set; }

        /// <summary>Random value minted on every login for this (UserId, ClientType); mirrored into the JWT's "sid" claim.</summary>
        [Column("session_token")]
        [MaxLength(SocialConstantsLengths.Length100)]
        public string SessionToken { get; set; } = string.Empty;

        [Column("last_login_date")]
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
