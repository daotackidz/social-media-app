using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Common.Constants;
using Social.Data;
using Social.Data.Repository;

Console.WriteLine("Data Seeding Starting !");

// Create hosting object and DI layer
using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();
var dbInitializer = scope.ServiceProvider.GetService<IDbInitializer>();
dbInitializer?.SeedData();

Console.WriteLine("Data Seeding Completed !");

IHostBuilder CreateHostBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            string? connectionString = _.Configuration.GetConnectionString(SocialConstant.MainConnString);

            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            services.AddDbContext<SocialDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IDbInitializer, DbInitializer>();
        });
}