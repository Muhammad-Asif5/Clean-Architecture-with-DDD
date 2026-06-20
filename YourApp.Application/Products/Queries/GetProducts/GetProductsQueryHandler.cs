using AutoMapper;
using MediatR;
using YourApp.Application.Products.DTOs;
using YourApp.Application.Products.Specifications; // ← Add this
using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;

namespace YourApp.Application.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IRepository<Product> productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var specification = new ProductSpecification(request.SearchTerm, request.MinPrice, request.MaxPrice);
            var products = await _productRepository.GetAsync(specification);
            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}