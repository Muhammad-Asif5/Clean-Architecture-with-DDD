using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;

namespace YourApp.Application.Products.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification(string searchTerm, decimal? minPrice, decimal? maxPrice)
            : base(p =>
                (string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm)) &&
                (!minPrice.HasValue || p.Price >= minPrice) &&
                (!maxPrice.HasValue || p.Price <= maxPrice))
        {
            ApplyOrderBy(p => p.Name);
        }
    }
}   