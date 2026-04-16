using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Application.CQRS.Products;
using ASP.NET_Hands_on.Domain.Model;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/product
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IMediator _mediator;
        private readonly ProductValidator validator = new ProductValidator();
        private readonly IProductsFetchingApiByUrl _productsFetchingApiByUrl;
        private readonly IConfiguration _configuration;

        public ProductsController(ILogger<ProductsController> logger, IProductsFetchingApiByUrl productsFetchingApiByUrl, IConfiguration configuration, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            _productsFetchingApiByUrl = productsFetchingApiByUrl;
            _configuration = configuration;
        }

        /// <summary>
        /// This api is used to get all products in database, only admin can access this api, if you want to test, please login with admin account to get token and add it to header with key "Authorization" and value "Bearer {token}"
        /// </summary>
        // api/products?pageNumber=1&numberOrProduct=30
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int? pageNumber, [FromQuery] int? numberOrProduct, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to ProductsController.GetAll");
            // read defaults from configuration
            var defaultPageNumber = _configuration.GetValue<int?>("Paging:PageNumber") ?? 1;
            var defaultPageSize = _configuration.GetValue<int?>("Paging:PageSize") ?? 30;

            var page = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : defaultPageNumber;
            var size = numberOrProduct.HasValue && numberOrProduct.Value > 0 ? numberOrProduct.Value : defaultPageSize;

            var (items, totalCount) = await _mediator.Send(new GetAllProductsQuery(page, size), cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            var pageResult = new
            {
                PageNumber = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };

            var apiResp = new ApiResponse<object>(pageResult, 200, "Request successful");
            return StatusCode(apiResp.StatusCode, apiResp);
        }

        [HttpGet("search")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetByProductName([FromQuery] string keyword, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to ProductsController.GetByProductName - keyword: {Keyword}", keyword);
            var productsFound = await _mediator.Send(new SearchProductsQuery(keyword), cancellationToken);
            var apiResp = new ApiResponse<object>(productsFound, 200, "Request successful");
            return Ok(apiResp);
        }

        //POST: api/products
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] Product newProduct, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(newProduct);

            if (!validationResult.IsValid) { 
                return BadRequest(validationResult.Errors);
            }

            _logger.LogInformation("Run to ProductsController.Create - creating product {ProductId}", newProduct.ProductId);
            var created = await _mediator.Send(new CreateProductCommand(newProduct), cancellationToken);
            var apiResp = new ApiResponse<object>(created, 201, "Created");
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, apiResp);
        }

        //POST: api/products/many
        //[Authorize(Roles = "Admin")]
        [HttpPost("many")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<object>>> CreateMany([FromBody] List<Product> productList, CancellationToken cancellationToken)
        {
            if (productList == null || productList.Count == 0)
            {
                return BadRequest("Product list cannot be empty.");
            }

            var created = await _mediator.Send(new CreateManyProductsCommand(productList), cancellationToken);
            var apiResp = new ApiResponse<object>(created, 201, "Created");
            return CreatedAtAction(nameof(GetAll), null, apiResp);
        }

        //PUT: api/products/5
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] Product updateData, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(updateData);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            _logger.LogInformation("Run to ProductsController.Update - id: {Id}", id);
            var updated = await _mediator.Send(new UpdateProductCommand(id, updateData), cancellationToken);
            var apiResp = new ApiResponse<object>(updated, 200, "Updated");
            return Ok(apiResp);
            
        }

        //[Authorize(Roles = "Admin")]
        //PATCH: api/products/5
        [HttpPatch("{id}/update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> Patch(int id, [FromQuery] ProductPatchRequest patchRequest, CancellationToken cancellationToken)
        {
            var validationResult = new ProductPatchRequestValidator().Validate(patchRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var patched = await _mediator.Send(new PatchProductCommand(id, patchRequest), cancellationToken);
            var apiResp = new ApiResponse<object>(patched, 200, "Patched");
            return Ok(apiResp);
        }

        //[Authorize(Roles = "Admin")]
        //DELETE: api/products/5
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            var apiResp = new ApiResponse<object>(null, 200, "Deleted");
            return Ok(apiResp);
        }

        //GET: api/products/dummyjson
        [HttpGet("dummyjson")]
        public async Task<IActionResult> GetProductsFromDummyJson()
        {
            var response = await _productsFetchingApiByUrl.GetProducts();

            if (response != null && response.Products.Count > 0)
            {
                var data = new
                {
                    Message = "Data fetching successfully!",
                    TotalCount = response.Total,
                    Data = response.Products
                };
                var apiResp = new ApiResponse<object>(data, 200, "Request successful");
                return Ok(apiResp);
            }

            var apiErr = new ApiResponse<object>(null, 400, "Failed to fetch data from other API");
            return BadRequest(apiErr);
        }

        // GET: api/products/mocktest-refit
        // This endpoint calls the local MockController via the Refit client so the Polly retry policy is used.
        [HttpGet("mocktest-refit")]
        public async Task<IActionResult> TestMockViaRefit(CancellationToken cancellationToken)
        {
            // This will call http://localhost:5225/api/mock through the Refit client configured in Program.cs
            var result = await _productsFetchingApiByUrl.Test();
            return Ok(result);
        }
    }
}
