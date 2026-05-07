using Microsoft.EntityFrameworkCore;
using Social.Data.Model.Base;
using Social.Data.Model.File;
using Social.Data.Model.Notification;
using Social.Data.Model.Post;
using Social.Data.Model.User;

namespace Social.Data.Repository
{
    public class SocialDbContext : BaseDbContext
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(GetDefaultSchemaName());
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserProfiles> UserProfiles { get; set; }
        public DbSet<UserRelations> UserRelations { get; set; }
        public DbSet<UserFiles> UserFiles { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<PostLikes> PostLikes { get; set; }
        public DbSet<PostComments> PostComments { get; set; }
        public DbSet<PostFiles> PostFiles { get; set; }
        public DbSet<PostHashTags> PostHashTags { get; set; }
        public DbSet<HashTags> HashTags { get; set; }
        public DbSet<PostSaves> PostSaves { get; set; }
        public DbSet<PostTags> PostTags { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<RecordStatus> RecordStatuses { get; set; }
    }
}
