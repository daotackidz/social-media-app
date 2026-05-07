using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Social.Data.Model.Base.RecordStatus;

namespace Social.Data.Model.Base
{
    [Index(nameof(RecordStatusId), IsUnique = false)]
    public class BaseRecordModel : BaseModel
    {
        [Column("reccord_status_id")]
        [Display(Name = "Trạng thái bản ghi")]
        [DefaultValue(Status.Active)]
        public Status RecordStatusId { get; set; } = Status.Active;

        #region Related Tables

        [ForeignKey(nameof(RecordStatusId))]
        public RecordStatus? RecordStatus { get; set; }

        #endregion
    }
}
