using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Voltis.IDP.Entities;
using Voltis.IDP.Services;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Facebook;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;

namespace Voltis.IDP
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{ env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionStrings:DefaultConnection"];
            services.AddDbContext<VoltisUserContext>(o => o.UseSqlServer(connectionString));

            services.AddScoped<IVoltisUserRepository, VoltisUserRepository>();

            var identityServerDataDBConnectionString = Configuration["connectionStrings:identityServerDataDbConnectionString"];

            var migrationAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;

            services.Configure<IISOptions>(options =>
            {
                options.AuthenticationDisplayName = "Windows";
                options.AutomaticAuthentication = true;
            });

            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddVoltisUserStore()
                .AddConfigurationStore(opt => opt.ConfigureDbContext = builder =>
                builder.UseSqlServer(identityServerDataDBConnectionString,
                options => options.MigrationsAssembly(migrationAssembly)));

            services.AddAuthentication()
                .AddFacebook("Facebook", options =>
                   {
                       options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                       options.ClientId = "1611375052299796";
                       options.ClientSecret = "c8f7c91f8add736867044816eb7e46d7";
                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, VoltisUserContext voltisUserContext,
            ConfigurationDbContext configurationDbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            configurationDbContext.Database.Migrate();
            configurationDbContext.EnsureSeedDAtaForContext();

            voltisUserContext.Database.Migrate();
            voltisUserContext.EnsureSeedDataForContext();

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
