using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;

namespace YourApp.Application.Products.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public ProductSpecification(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool sortDescending = false)
            : base(p =>
                (string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm)) &&
                (!minPrice.HasValue || p.Price >= minPrice) &&
                (!maxPrice.HasValue || p.Price <= maxPrice))
        {
            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                ApplySorting(sortBy, sortDescending);
            }
            else
            {
                ApplyOrderBy(p => p.Name);
            }

            // Apply pagination
            if (pageNumber > 0 && pageSize > 0)
            {
                var skip = (pageNumber - 1) * pageSize;
                ApplyPaging(skip, pageSize);
            }
        }

        // ✅ Constructor for count query - uses protected setter
        public static ProductSpecification CreateCountSpecification(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice)
        {
            // Create a new specification with the criteria
            var spec = new ProductSpecification(searchTerm, minPrice, maxPrice, 0, 0);

            // Mark as count query
            spec.IsCountQuery = true;

            // Clear ordering and paging for count query
            spec.ApplyOrderBy(null);
            spec.ApplyOrderByDescending(null);
            spec.IsPagingEnabled = false;

            return spec;
        }

        private void ApplySorting(string sortBy, bool sortDescending)
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortDescending)
                        ApplyOrderByDescending(p => p.Name);
                    else
                        ApplyOrderBy(p => p.Name);
                    break;
                case "price":
                    if (sortDescending)
                        ApplyOrderByDescending(p => p.Price);
                    else
                        ApplyOrderBy(p => p.Price);
                    break;
                case "id":
                    if (sortDescending)
                        ApplyOrderByDescending(p => p.Id);
                    else
                        ApplyOrderBy(p => p.Id);
                    break;
                default:
                    ApplyOrderBy(p => p.Name);
                    break;
            }
        }
    }
}