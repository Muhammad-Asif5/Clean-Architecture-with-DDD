using MediatR;
using YourApp.Application.Common.Models;
using YourApp.Application.Products.DTOs;

namespace YourApp.Application.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}