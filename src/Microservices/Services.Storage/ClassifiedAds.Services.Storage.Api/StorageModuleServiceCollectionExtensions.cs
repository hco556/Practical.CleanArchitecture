﻿using ClassifiedAds.Domain.Events;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Infrastructure.Identity;
using ClassifiedAds.Services.Storage.Authorization;
using ClassifiedAds.Services.Storage.ConfigurationOptions;
using ClassifiedAds.Services.Storage.DTOs;
using ClassifiedAds.Services.Storage.Entities;
using ClassifiedAds.Services.Storage.HostedServices;
using ClassifiedAds.Services.Storage.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddStorageModule(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddDbContext<StorageDbContext>(options => options.UseSqlServer(appSettings.ConnectionStrings.ClassifiedAds, sql =>
            {
                if (!string.IsNullOrEmpty(appSettings.ConnectionStrings.MigrationsAssembly))
                {
                    sql.MigrationsAssembly(appSettings.ConnectionStrings.MigrationsAssembly);
                }
            }))
                .AddScoped<IRepository<FileEntry, Guid>, Repository<FileEntry, Guid>>()
                .AddScoped<IRepository<AuditLogEntry, Guid>, Repository<AuditLogEntry, Guid>>()
                .AddScoped<IRepository<OutboxEvent, long>, Repository<OutboxEvent, long>>();

            DomainEvents.RegisterHandlers(Assembly.GetExecutingAssembly(), services);

            services.AddMessageHandlers(Assembly.GetExecutingAssembly());

            services.AddAuthorizationPolicies(Assembly.GetExecutingAssembly(), AuthorizationPolicyNames.GetPolicyNames());

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUser, CurrentWebUser>();

            services.AddStorageManager(appSettings.Storage);

            services.AddMessageBusSender<FileUploadedEvent>(appSettings.MessageBroker)
                    .AddMessageBusSender<FileDeletedEvent>(appSettings.MessageBroker)
                    .AddMessageBusSender<AuditLogCreatedEvent>(appSettings.MessageBroker)
                    .AddMessageBusReceiver<FileUploadedEvent>(appSettings.MessageBroker)
                    .AddMessageBusReceiver<FileDeletedEvent>(appSettings.MessageBroker);

            return services;
        }

        public static void MigrateStorageDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<StorageDbContext>().Database.Migrate();
            }
        }

        public static IServiceCollection AddHostedServicesStorageModule(this IServiceCollection services)
        {
            services.AddScoped<PublishEventService>();

            services.AddHostedService<MessageBusReceiver>();
            services.AddHostedService<PublishEventWorker>();

            return services;
        }
    }
}
