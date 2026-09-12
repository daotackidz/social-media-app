using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.Post.Repository;
using Social.Repository.Social.Relation.Interface;
using Social.Repository.Social.Relation.Repository;
using Social.Repository.Social.Search.Interface;
using Social.Repository.Social.Search.Repository;
using Social.Repository.Social.Story.Interface;
using Social.Repository.Social.Story.Repository;
using Social.Repository.Social.User.Interface;
using Social.Repository.Social.User.Repository;

namespace Social.WebApi.Installers
{
    public class RepositoryInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserPendingRepository, UserPendingRepository>();
            services.AddScoped<IUserRelationRepository, UserRelationRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
            services.AddScoped<IStoryRepository, StoryRepository>();
        }
    }
}
