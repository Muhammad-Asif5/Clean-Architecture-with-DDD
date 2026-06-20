using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourApp.Application.Common.Models;
using YourApp.Application.Products.Commands.CreateProduct;
using YourApp.Application.Products.DTOs;
using YourApp.Application.Products.Queries.GetProducts;
using YourApp.Domain.Constants;
using static YourApp.Domain.Constants.Permissions;

namespace YourApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.Product},{Roles.SuperAdmin},{Roles.ManageRole},{Roles.AcademicManager}", Policy = ProductPermission.CanRead)]
    public class ProductsController : ApiControllerExtensions
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = ProductPermission.CanRead)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get([FromQuery] GetProductsQuery query)
        {
            var profile = GetProfile();
            _logger.LogInformation("User {UserId} requested products", profile?.UserId);

            var result = await _mediator.Send(query);

            if (result == null || !result.Any())
            {
                return Ok(ApiResponse<List<ProductDto>>.NoContentResponse());
            }

            return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(result));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            var profile = GetProfile();
            _logger.LogInformation("User {UserId} creating product", profile?.UserId);

            if (!profile.CanCreate)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.ForbiddenResponse());
            }

            var productId = await _mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<int>.CreatedResponse(productId, ResponseType.Created, "Product created successfully"));
        }

        //[HttpPut("{id}")]
        //[Authorize(Policy = Permissions.CoursePermission.CanUpdate)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        //public async Task<IActionResult> Update(int id, UpdateProductCommand command)
        //{
        //    var profile = GetProfile();
        //    _logger.LogInformation("User {UserId} updating product {ProductId}", profile?.UserId, id);

        //    if (!profile.CanUpdate)
        //    {
        //        return StatusCode(StatusCodes.Status403Forbidden,
        //            ApiResponse<object>.ForbiddenResponse());
        //    }

        //    command.Id = id;
        //    await _mediator.Send(command);

        //    return StatusCode(StatusCodes.Status204NoContent,
        //        ApiResponse<object>.NoContentResponse());
        //}

        [HttpDelete("{id}")]
        [Authorize(Policy = ProductPermission.CanDelete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = GetProfile();
            _logger.LogInformation("User {UserId} deleting product {ProductId}", profile?.UserId, id);

            if (!profile.CanDelete)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.ForbiddenResponse());
            }

            //await _mediator.Send(new DeleteProductCommand { Id = id });

            return StatusCode(StatusCodes.Status204NoContent,
                ApiResponse<object>.NoContentResponse());
        }

        [HttpGet("export")]
        [Authorize(Policy = ProductPermission.CanExport)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Export()
        {
            var profile = GetProfile();
            _logger.LogInformation("User {UserId} exporting products", profile?.UserId);

            if (!profile.CanExport)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.ForbiddenResponse());
            }

            // Export logic here
            var exportData = await Task.FromResult(new { message = "Export successful" });

            return Ok(ApiResponse<object>.SuccessResponse(exportData, "Export", "Products exported successfully"));
        }
    }
}