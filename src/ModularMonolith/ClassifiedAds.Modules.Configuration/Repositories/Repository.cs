﻿using ClassifiedAds.CrossCuttingConcerns.OS;
using ClassifiedAds.Domain.Entities;
using ClassifiedAds.Infrastructure.Persistence;

namespace ClassifiedAds.Modules.Configuration.Repositories
{
    public class Repository<T, TKey> : DbContextRepository<ConfigurationDbContext, T, TKey>
        where T : Entity<TKey>, IAggregateRoot
    {
        public Repository(ConfigurationDbContext dbContext, IDateTimeProvider dateTimeProvider)
            : base(dbContext, dateTimeProvider)
        {
        }
    }
}
