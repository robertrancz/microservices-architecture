using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;
using Catalog.Service.Extensions;
using Microsoft.AspNetCore.Mvc;
using Services.Common;

namespace Catalog.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repository;

        public ItemsController(IRepository<Item> repository)
        {
            this.repository = repository;
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
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAsync(item);

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
                Price = updatedItemDto.Price
            };

            await repository.UpdateAsync(updatedItem);

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

            return NoContent();
        }
    }
}