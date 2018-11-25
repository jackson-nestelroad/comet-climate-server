using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using WebAPI.Models;

namespace Comet.Climate.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            Uri databaseUri;
            // Production environment - use Heroku string
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                databaseUri = new Uri(Environment.GetEnvironmentVariable("DATABASE_URL"));
            }
            // Development environment - use Configuration string
            else
            {
                databaseUri = new Uri(Configuration["DATABASE_URL"]);
            }
            // Build connection string
            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = databaseUri.UserInfo.Split(':')[0],
                Password = databaseUri.UserInfo.Split(':')[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                IntegratedSecurity = true,
                Pooling = true
            };

            // Add scoped connection to PostgreSQL 
            services.AddEntityFrameworkNpgsql().AddDbContext<WebAPIContext>(options => 
                options.UseNpgsql(connectionString.ToString()), ServiceLifetime.Scoped);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Development (local) environment
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Production (Heroku) environment
            else
            {
                // Use custom exception handler
                app.UseStatusCodePagesWithReExecute("/error");
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
