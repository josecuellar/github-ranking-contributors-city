using GitHub.API.Repository;
using GitHub.API.Repository.Impl.InMemory;
using GitHub.API.Service;
using GitHub.API.Service.Impl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHub.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMemoryCache();
            services.AddTransient<ILoadDataService, LoadDataService>();
            services.AddTransient<IGitHubApiProvider, OctokitGitHubApiProvider>();
            services.AddTransient<IStatusRepository, InMemoryStatusRepository>();
            services.AddTransient<IRepository, InMemoryRepository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
        }
    }
}
