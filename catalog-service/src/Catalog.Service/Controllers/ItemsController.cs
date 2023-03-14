using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Contracts;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;
using Catalog.Service.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Services.Common;
using Services.Common.Settings;

namespace Catalog.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly Counter<int> createItemCounter, updateItemCounter, deleteItemCounter;

        public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndpoint, IConfiguration configuration)
        {
            this.repository = repository;
            this.publishEndpoint = publishEndpoint;

            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var meter = new Meter(serviceSettings.ServiceName);
            createItemCounter = meter.CreateCounter<int>("CatalogItemCreated");
            updateItemCounter = meter.CreateCounter<int>("CatalogItemUpdated");
            deleteItemCounter = meter.CreateCounter<int>("CatalogItemDeleted");
        }

        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemResponseDto>> GetItemsAsync()
        {
            var items = (await repository.GetAllAsync())
                .Select(item => item.AsDto());

            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemResponseDto>> GetItemAsync(Guid id)
        {
            var item = await repository.GetAsync(id);

            if(item is null)
            {
                return NotFound();
            }
            
            return item.AsDto();
        }

        // POST /items
        [HttpPost]
        public async Task<ActionResult<ItemResponseDto>> CreateItemAsync(CreateItemDto createItemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAsync(item);

            createItemCounter.Add(1, new KeyValuePair<string, object?>(nameof(item.Id), item.Id));

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto());
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto updatedItemDto)
        {
            var existingItem = await repository.GetAsync(id);

            if(existingItem is null)
            {
                return NotFound();
            }

            // UpdatedItem is a copy of existingItem with name and Price updated
            Item updatedItem = existingItem with
            {
                Name = updatedItemDto.Name,
                Description = updatedItemDto.Description,
                Price = updatedItemDto.Price
            };

            await repository.UpdateAsync(updatedItem);

            updateItemCounter.Add(1, new KeyValuePair<string, object?>(nameof(existingItem.Id), existingItem.Id));

            await publishEndpoint.Publish(new CatalogItemUpdated(updatedItem.Id, updatedItem.Name, updatedItem.Description));

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItemAsync(Guid id)
        {
            var existingItem = await repository.GetAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            await repository.RemoveAsync(id);

            deleteItemCounter.Add(1, new KeyValuePair<string, object?>(nameof(existingItem.Id), existingItem.Id));

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}