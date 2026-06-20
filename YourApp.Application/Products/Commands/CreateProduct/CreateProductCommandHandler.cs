using MediatR;
using YourApp.Domain.Entities;
using YourApp.Domain.Interfaces;

namespace YourApp.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IRepository<Product> _productRepository;

        public CreateProductCommandHandler(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product(request.Name, request.Price, request.Description);
            var result = await _productRepository.AddAsync(product);
            return result.Id;
        }
    }
}