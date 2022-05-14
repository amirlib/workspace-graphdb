using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;
using WorkspaceGraphs.Configurations;
using WorkspaceGraphs.Repositories;

namespace WorkspaceGraphs
{
    class Program
    {
        public static IConfigurationRoot configuration;

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var service = host.Services.GetRequiredService<Ui>();

            await service.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, reloadOnChange: true)
                .Build();

            // Read Neo4j values from appsettings file
            var connection = configuration
                .GetSection("Neo4j")
                .Get<Neo4jOptions>();

            return Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Dependency Injections
                    services.AddSingleton(configuration);
                    services.AddSingleton(GraphDatabase.Driver(
                        connection.Uri,
                        AuthTokens.Basic(connection.UserName, connection.Password))); // GraphDb login
                    services.AddScoped<WorkspacesRepository, WorkspacesRepository>();
                    services.AddScoped<Strater, Strater>();
                    services.AddTransient<Ui>();
                });
        }
    }
}
