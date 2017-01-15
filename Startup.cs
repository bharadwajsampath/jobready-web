using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Implementations;
using Web.Models;
using Stormpath.AspNetCore;
using Stormpath.Configuration.Abstractions;
using System.Collections.Generic;

namespace Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var form = new Dictionary<string, WebFieldConfiguration>();
            form.Add("Trainer", new WebFieldConfiguration
            {
                Required = true,
                Placeholder = "Trainer ID",
                Enabled = true,
                Type = "text",
                Label = "Trainer",
            });

            var customData = new Dictionary<string, bool>();
            customData.Add("customData", true);

            services.AddStormpath(new StormpathConfiguration
            {
                Web = new WebConfiguration
                {

                    Login = new WebLoginRouteConfiguration
                    {
                        View = "~/Views/Stormpath/Login.cshtml"
                    },
                    Logout = new WebLogoutRouteConfiguration
                    {
                        Uri = "/logout",
                        NextUri = "/"
                    },
                    Register = new WebRegisterRouteConfiguration
                    {
                        Form = new WebRegisterRouteFormConfiguration
                        {
                            Fields = form
                        },
                        View = "~/Views/Stormpath/Register.cshtml"
                    },
                    Me = new WebMeRouteConfiguration
                    {
                        Expand = customData
                    }

                }
            });

            services.Configure<JRSettings>(options => Configuration.GetSection("JRSettings").Bind(options));

            services.AddEntityFramework().AddDbContext<JRDbContext>();

            //services.AddIdentity<User, IdentityRole>()
            //    .AddEntityFrameworkStores<JRDbContext>()
            //    .AddDefaultTokenProviders();
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            // app.UseIdentity();
            app.UseStormpath();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Event}/{action=Index}/{id?}");
            });
        }
    }
}
