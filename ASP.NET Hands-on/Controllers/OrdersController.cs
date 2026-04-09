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

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var orders = await _orderService.GetOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
                return Ok(order);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
        }

        // CLient only needs to send a List consisting of products' id: [1, 2, 1]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<int> productIds, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(productIds, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
        }

        [HttpPost("{orderId}/products/{productId}")]
        public async Task<IActionResult> AddProductToOrder(int orderId, int productId, [FromQuery] int quantity = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _orderService.AddProductToOrderAsync(orderId, productId, quantity, cancellationToken);
                if (result) return NoContent();
                return BadRequest("Could not add product to order.");
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _orderService.DeleteOrderAsync(id, cancellationToken);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
