﻿using ClassifiedAds.Infrastructure.Monitoring;
using ClassifiedAds.Infrastructure.Web.Filters;
using ClassifiedAds.Services.Product.ConfigurationOptions;
using ClassifiedAds.Services.Product.RateLimiterPolicies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Reflection;

namespace ClassifiedAds.Services.Product
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            AppSettings = new AppSettings();
            Configuration.Bind(AppSettings);
        }

        public IConfiguration Configuration { get; }

        private AppSettings AppSettings { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AppSettings.ConnectionStrings.MigrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMonitoringServices(AppSettings.Monitoring);

            services.AddControllers(configure =>
            {
                configure.Filters.Add(typeof(GlobalExceptionFilter));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
            })
            .AddJsonOptions(options =>
            {
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddDateTimeProvider();
            services.AddApplicationServices();

            services.AddHtmlGenerator();
            services.AddDinkToPdfConverter();

            services.AddProductModule(AppSettings);
            services.AddHostedServicesProductModule();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = AppSettings.IdentityServerAuthentication.Authority;
                        options.Audience = AppSettings.IdentityServerAuthentication.ApiName;
                        options.RequireHttpsMetadata = AppSettings.IdentityServerAuthentication.RequireHttpsMetadata;
                    });

            services.AddRateLimiter(options =>
            {
                options.AddPolicy<string, DefaultRateLimiterPolicy>(RateLimiterPolicyNames.DefaultPolicy);
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDaprClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Policy.Handle<Exception>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(30),
            })
            .Execute(() =>
            {
                app.MigrateProductDb();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
