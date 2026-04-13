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

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetAll");
            var orders = await _orderService.GetOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetById - Id: {id}", id);

            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            return Ok(order);

        }

        // CLient only needs to send a List consisting of products' id: [1, 2, 1]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<int> productIds, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.CreateOrder - List Of Product Id: {@Ids}", productIds);

            var order = await _orderService.CreateOrderAsync(productIds, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);

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
