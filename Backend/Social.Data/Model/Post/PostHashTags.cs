using Social.Data.Model.Base;
using Social.Data.Model.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social.Data.Model.Post
{
    [Table("post_hash_tags")]
    public class PostHashTags : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("post_id", Order = 1)]
        public Guid PostId { get; set; }

        [Column("hash_tag_id", Order = 2)]
        public Guid HashTagId { get; set; }

        #region Related Tables

        [ForeignKey(nameof(PostId))]
        public Posts? Posts { get; set; }

        [ForeignKey(nameof(HashTagId))]
        public HashTags? HashTags { get; set; }

        #endregion
    }

    [Table("hash_tags")]
    public class HashTags : BaseRecordModel
    {
        [Key]
        [Column("id", Order = 0)]
        [ScaffoldColumn(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Column("hash_tag_name", Order = 1)]
        [Display(Name = "Tên hashtag")]
        public string HashTagName { get; set; }

        #region Related Tables

        #endregion
    }
}
