using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Contracts;
using Inventory.Service.Entities;
using MassTransit;
using Services.Common;
using Services.Common.Settings;

namespace Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository;
        private readonly Counter<int> updateItemCounter;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository, IConfiguration configuration)
        {
            this.repository = repository;

            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var meter = new Meter(serviceSettings.ServiceName);
            updateItemCounter = meter.CreateCounter<int>("CatalogItemUpdatedMessageConsumed");
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;

            var item = await repository.GetAsync(message.ItemId);

            if(item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;

                await repository.UpdateAsync(item);
            }

            updateItemCounter.Add(1, new KeyValuePair<string, object?>(nameof(item.Id), item.Id));
        }
    }
}