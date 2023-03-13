using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;

namespace Catalog.Service.Extensions
{
    public static class ItemMapper
    {
        public static ItemResponseDto AsDto(this Item item)
        {
            return new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                CreatedDate = item.CreatedDate
            };
        }
    }
}