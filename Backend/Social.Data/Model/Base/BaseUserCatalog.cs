using Microsoft.EntityFrameworkCore;
using Social.Common.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Base
{
    [Index(nameof(CreatedByUserId), nameof(StatusId), IsUnique = false)]
    public class BaseCatalog : object
    {
        [Column("status_id")]
        [Display(Name = "Trạng thái bản ghi")]
        [DefaultValue(SocialStatusConstant.Active)]
        [ForeignKey(nameof(StatusId))]
        public int StatusId { get; set; } = SocialStatusConstant.Active;

        [Column("created_by_user_id")]
        [Display(Name = "Người tạo")]
        [ScaffoldColumn(false)]
        [DefaultValue(SocialConstantValue.DefaultSystemId)]
        public Guid CreatedByUserId { get; set; } = Guid.Empty;

        [Column("create_datetime")]
        [Display(Name = "Ngày tạo")]
        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("create_datetime_unix")]
        [Display(Name = "Ngày tạo")]
        [ScaffoldColumn(false)]
        public long CreatedDateUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
