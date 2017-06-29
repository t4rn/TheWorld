using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _config;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();

            _config = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<WorldUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.Password.RequiredLength = 8;
                config.Cookies.ApplicationCookie.LoginPath = "/Auth/Login";
                config.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = async ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") &&
                        ctx.Response.StatusCode == 200)
                        {
                            // API call
                            ctx.Response.StatusCode = 401;
                        }
                        else
                        {
                            // normal call
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }

                        await Task.Yield();
                    }
                };
            })
            .AddEntityFrameworkStores<WorldContext>();

            services.AddSingleton(_config);

            if (_env.IsDevelopment())
            {
                services.AddScoped<IMailService, DebugMailService>();
            }
            else
            {
                // Implement a real Mail Service
            }

            services.AddDbContext<WorldContext>();

            services.AddScoped<IWorldRepository, WorldRepository>();

            services.AddScoped<IGeoCoordsService, GeoCoordsGoogleService>();

            services.AddTransient<WorldContextSeedData>();

            services.AddLogging();

            services.AddMvc(config =>
            {
                if (_env.IsProduction())
                {
                    config.Filters.Add(new RequireHttpsAttribute());
                }
            });

            // camel case is working by default, without the code belowe
                //.AddJsonOptions(config =>
                //{
                //    config.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            WorldContextSeedData seeder)
        {
            Mapper.Initialize(config =>
            {
                config.CreateMap<TripViewModel, Trip>().ReverseMap();
                config.CreateMap<StopViewModel, Stop>().ReverseMap();
            });

            //loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug(LogLevel.Information);
            }
            else
            {
                loggerFactory.AddDebug(LogLevel.Error);
            }

            //app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc(MapRoutes);

            seeder.InsertSeedData().Wait();
        }

        private void MapRoutes(IRouteBuilder config)
        {
            config.MapRoute(
                name: "Default",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = "App", action = "Index" });
        }
    }
}
