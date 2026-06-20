using AutoMapper;
using MediatR;
using YourApp.Application.Common.Models;
using YourApp.Application.Products.DTOs;
using YourApp.Application.Products.Specifications;
using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;

namespace YourApp.Application.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IRepository<Product> productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            // ✅ Create the main specification with pagination
            var specification = new ProductSpecification(
                request.SearchTerm,
                request.MinPrice,
                request.MaxPrice,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            // ✅ Get the products
            var products = await _productRepository.GetAsync(specification);

            // ✅ Get the total count (separate query)
            var countSpec = ProductSpecification.CreateCountSpecification(
                request.SearchTerm,
                request.MinPrice,
                request.MaxPrice);

            var totalCount = await _productRepository.CountAsync(countSpec);

            // ✅ Map to DTOs
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            // ✅ Return paged result
            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}