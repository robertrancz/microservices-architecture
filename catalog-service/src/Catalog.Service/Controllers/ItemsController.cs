using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Contracts;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;
using Catalog.Service.Extensions;
using Catalog.Service.Metrics;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Services.Common;

namespace Catalog.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly OtelMetrics metrics;

        public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndpoint, OtelMetrics metrics)
        {
            this.repository = repository;
            this.publishEndpoint = publishEndpoint;
            this.metrics = metrics;
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

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            metrics.AddCatalogItems();
            metrics.IncreaseTotalCatalogItems();

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

            await publishEndpoint.Publish(new CatalogItemUpdated(updatedItem.Id, updatedItem.Name, updatedItem.Description));

            metrics.UpdateCatalogItems();

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

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            metrics.DeleteCatalogItems();
            metrics.DecreaseTotalCatalogItems();

            return NoContent();
        }
    }
}