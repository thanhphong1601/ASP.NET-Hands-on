using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;
        private readonly IConfiguration _configuration;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger, IConfiguration configuration)
        {
            _orderService = orderService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetAll");

            var defaultPageNumber = _configuration.GetValue<int?>("Paging:PageNumber") ?? 1;
            var defaultPageSize = _configuration.GetValue<int?>("Paging:PageSize") ?? 30;

            var page = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : defaultPageNumber;
            var size = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : defaultPageSize;

            var (items, totalCount) = await _orderService.GetOrdersAsync(page, size, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            var pageResult = new
            {
                PageNumber = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };

            var apiResp = new Model.ApiResponse<object>(pageResult, 200, "Request successful");
            return StatusCode(apiResp.StatusCode, apiResp);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetById - Id: {id}", id);

            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            var apiResp = new Model.ApiResponse<object>(order, 200, "Request successful");
            return Ok(apiResp);

        }

        // CLient only needs to send a List consisting of products' id: [1, 2, 1]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<int> productIds, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.CreateOrder - List Of Product Id: {@Ids}", productIds);

            var order = await _orderService.CreateOrderAsync(productIds, cancellationToken);
            var apiResp = new Model.ApiResponse<object>(order, 201, "Created");
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, apiResp);

        }

        [HttpPost("{orderId}/products/{productId}")]
        public async Task<IActionResult> AddProductToOrder(int orderId, int productId, [FromQuery] int quantity = 1, CancellationToken cancellationToken = default)
        {

            var result = await _orderService.AddProductToOrderAsync(orderId, productId, quantity, cancellationToken);
            if (result) return NoContent();
            return BadRequest("Could not add product to order.");

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _orderService.DeleteOrderAsync(id, cancellationToken);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
