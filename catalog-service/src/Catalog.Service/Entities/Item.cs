using System;
using Services.Common;

namespace Catalog.Service.Entities
{
    public record Item : IEntity
    {
        // Init means that the property can only be set during the initialization of the object.
        public Guid Id { get; set; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}