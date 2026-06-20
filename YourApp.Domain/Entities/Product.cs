using YourApp.Domain.Common;
using YourApp.Domain.Exceptions;

namespace YourApp.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public string Description { get; private set; }

        private Product() { } // EF Core constructor

        public Product(string name, decimal price, string description = null)
        {
            SetName(name);
            SetPrice(price);
            Description = description;
        }

        public void UpdateName(string newName)
        {
            SetName(newName);
        }

        public void UpdatePrice(decimal newPrice)
        {
            SetPrice(newPrice);
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Product name is required");
            Name = name;
        }

        private void SetPrice(decimal price)
        {
            if (price <= 0)
                throw new DomainException("Price must be greater than zero");
            Price = price;
        }
    }
}