using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/product
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        private readonly ProductValidator validator = new ProductValidator();
        private readonly IProductsFetchingApiByUrl _productsFetchingApiByUrl;
        private readonly IConfiguration _configuration;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger, IProductsFetchingApiByUrl productsFetchingApiByUrl, IConfiguration configuration)
        {
            _productService = productService;
            _logger = logger;
            _productsFetchingApiByUrl = productsFetchingApiByUrl;
            _configuration = configuration;
        }

        /// <summary>
        /// This api is used to get all products in database, only admin can access this api, if you want to test, please login with admin account to get token and add it to header with key "Authorization" and value "Bearer {token}"
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? numberOrProduct, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to ProductsController.GetAll");
            // read defaults from configuration
            var defaultPageNumber = _configuration.GetValue<int?>("Paging:PageNumber") ?? 1;
            var defaultPageSize = _configuration.GetValue<int?>("Paging:PageSize") ?? 10;

            var page = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : defaultPageNumber;
            var size = numberOrProduct.HasValue && numberOrProduct.Value > 0 ? numberOrProduct.Value : defaultPageSize;

            var (items, totalCount) = await _productService.GetAllAsync(page, size, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            var result = new
            {
                PageNumber = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetByProductName([FromQuery] string keyword, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to ProductsController.GetByProductName - keyword: {Keyword}", keyword);
            var productsFound = await _productService.SearchByNameOrProductIdAsync(keyword, cancellationToken);
            return Ok(productsFound);
        }

        //POST: api/products
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product newProduct, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(newProduct);

            if (!validationResult.IsValid) { 
                return BadRequest(validationResult.Errors);
            }

            _logger.LogInformation("Run to ProductsController.Create - creating product {ProductId}", newProduct.ProductId);
            var created = await _productService.CreateAsync(newProduct, cancellationToken);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        //POST: api/products/many
        //[Authorize(Roles = "Admin")]
        [HttpPost("many")]
        public async Task<IActionResult> CreateMany([FromBody] List<Product> productList, CancellationToken cancellationToken)
        {
            if (productList == null || productList.Count == 0)
            {
                return BadRequest("Product list cannot be empty.");
            }

            var created = await _productService.CreateManyAsync(productList, cancellationToken);
            return CreatedAtAction(nameof(GetAll), null, created);
        }

        //PUT: api/products/5
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product updateData, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(updateData);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            _logger.LogInformation("Run to ProductsController.Update - id: {Id}", id);
            var updated = await _productService.UpdateAsync(id, updateData, cancellationToken);
            return Ok(updated);
            
        }

        //[Authorize(Roles = "Admin")]
        //PATCH: api/products/5
        [HttpPatch("{id}/update")]
        public async Task<IActionResult> Patch(int id, [FromQuery] ProductPatchRequest patchRequest, CancellationToken cancellationToken)
        {
            var validationResult = new ProductPatchRequestValidator().Validate(patchRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var patched = await _productService.PatchAsync(id, patchRequest, cancellationToken);
            return Ok(patched);
        }

        //[Authorize(Roles = "Admin")]
        //DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _productService.DeleteAsync(id, cancellationToken);
            return Ok();
        }

        //GET: api/products/dummyjson
        [HttpGet("dummyjson")]
        public async Task<IActionResult> GetProductsFromDummyJson()
        {
            var response = await _productsFetchingApiByUrl.GetProducts();

            if (response != null && response.Products.Count > 0)
            {
                return Ok(new
                {
                    Message = "Data fetching successfully!",
                    TotalCount = response.Total,
                    Data = response.Products
                });
            }

            return BadRequest("Failed to fetch data from other API");
        }
    }
}
