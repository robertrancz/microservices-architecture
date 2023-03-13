using System.ComponentModel.DataAnnotations;

namespace Catalog.Service.Dtos
{
    public class CreateItemDto
    {
        [Required]
        public string Name { get; init; }

        [Required]
        [Range(1, 1000)]
        public decimal Price { get; init; }
    }
}