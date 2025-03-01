﻿using ClassifiedAds.Domain.Entities;
using System;

namespace ClassifiedAds.Services.Product.Entities
{
    public class OutboxEvent : Entity<long>, IAggregateRoot
    {
        public string EventType { get; set; }

        public Guid TriggeredById { get; set; }

        public string ObjectId { get; set; }

        public string Message { get; set; }

        public bool Published { get; set; }
    }
}
