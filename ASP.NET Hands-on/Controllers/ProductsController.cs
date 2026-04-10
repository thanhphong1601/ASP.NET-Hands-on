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
        readonly ProductValidator validator = new ProductValidator();


        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Run to ProductsController.GetAll");
            return Ok(_productService.GetAll());
        }

        [HttpGet("search")]
        public IActionResult GetByProductName([FromQuery] string keyword)
        {
            _logger.LogInformation("Run to ProductsController.GetByProductName - keyword: {Keyword}", keyword);
            var productsFound = _productService.SearchByNameOrProductId(keyword);
            return Ok(productsFound);
        }

        //POST: api/products
        //[Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Product newProduct)
        {
            var validationResult = validator.Validate(newProduct);

            if (!validationResult.IsValid) { 
                return BadRequest(validationResult.Errors);
            }

            _logger.LogInformation("Run to ProductsController.Create - creating product {ProductId}", newProduct.ProductId);
            var created = _productService.Create(newProduct);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        //POST: api/products/many
        [HttpPost("many")]
        public IActionResult CreateMany([FromBody] List<Product> productList)
        {
            if (productList == null || productList.Count == 0)
            {
                return BadRequest("Product list cannot be empty.");
            }

            var created = _productService.CreateMany(productList);
            return CreatedAtAction(nameof(GetAll), null, created);
        }

        //PUT: api/products/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product updateData)
        {
            var validationResult = validator.Validate(updateData);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                _logger.LogInformation("Run to ProductsController.Update - id: {Id}", id);
                var updated = _productService.Update(id, updateData);
                if (updated == null) return NotFound("Product not found");
                return Ok(updated);
            }
            catch (ArgumentException argumentExcemtion)
            {
                return BadRequest(argumentExcemtion.Message);
            }
            
        }

        //PATCH: api/products/5
        [HttpPatch("{id}/update")]
        public IActionResult Patch(int id, [FromQuery] ProductPatchRequest patchRequest)
        {
            var validationResult = new ProductPatchRequestValidator().Validate(patchRequest);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var patched = _productService.Patch(id, patchRequest);
                if (patched == null) return NotFound("Product not found");
                return Ok(patched);
            }
            catch (ArgumentException argumentExcemtion)
            {
                return BadRequest(argumentExcemtion.Message);
            }
        }

        //DELETE: api/products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _productService.Delete(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
