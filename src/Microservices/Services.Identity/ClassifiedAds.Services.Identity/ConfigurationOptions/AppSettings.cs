﻿using ClassifiedAds.Infrastructure.Caching;
using ClassifiedAds.Infrastructure.Interceptors;
using ClassifiedAds.Infrastructure.Logging;
using ClassifiedAds.Infrastructure.Monitoring;
using ClassifiedAds.Infrastructure.Notification;

namespace ClassifiedAds.Services.Identity.ConfigurationOptions
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public LoggingOptions Logging { get; set; }

        public CachingOptions Caching { get; set; }

        public MonitoringOptions Monitoring { get; set; }

        public IdentityServerAuthentication IdentityServerAuthentication { get; set; }

        public NotificationOptions Notification { get; set; }

        public InterceptorsOptions Interceptors { get; set; }
    }

    public class ConnectionStrings
    {
        public string ClassifiedAds { get; set; }

        public string MigrationsAssembly { get; set; }
    }

    public class IdentityServerAuthentication
    {
        public string Authority { get; set; }

        public string ApiName { get; set; }

        public bool RequireHttpsMetadata { get; set; }
    }
}
