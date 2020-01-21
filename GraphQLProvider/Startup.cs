using GraphQLProvider.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GraphQLProvider.Repositories;
using GraphQLProvider.GraphQL;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using GraphQL;

namespace GraphQLProvider
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

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddDbContext<ProviderDbContext>(options =>  options.UseSqlServer(Configuration["ConnectionStrings:ProviderCon"]));
            
            services.AddScoped<ProductRepository>();         
            services.AddScoped<ProductModelRepository>();         


            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<ProviderSchema>();


            services.AddGraphQL(o => { o.ExposeExceptions = false; })
                    .AddGraphTypes(ServiceLifetime.Scoped).AddUserContextBuilder(httpContext => httpContext)
                    .AddUserContextBuilder(httpContext => httpContext.User)
                    .AddDataLoader();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseGraphQL<ProviderSchema>();
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
        }
    }
}
