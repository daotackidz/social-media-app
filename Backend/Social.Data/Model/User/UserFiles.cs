using Social.Common.Constants;
using Social.Data.Model.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.User
{
    [Table("user_files")]
    public class UserFiles : BaseCatalog
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("user_id", Order = 1)]
        public Guid UserId { get; set; }


        #region Related Tables

        [ForeignKey(nameof(UserId))]
        public Users? Users { get; set; }

        #endregion
    }
}
