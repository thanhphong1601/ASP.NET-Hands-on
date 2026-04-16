using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Application.CQRS.Orders;
using MediatR;
using ASP.NET_Hands_on.Domain.Model;
using ASP.NET_Hands_on.Application.DTO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public OrdersController(ILogger<OrdersController> logger, IConfiguration configuration, IMediator mediator)
        {
            _logger = logger;
            _configuration = configuration;
            _mediator = mediator;
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetAll");

            var defaultPageNumber = _configuration.GetValue<int?>("Paging:PageNumber") ?? 1;
            var defaultPageSize = _configuration.GetValue<int?>("Paging:PageSize") ?? 30;

            var page = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : defaultPageNumber;
            var size = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : defaultPageSize;

            var (items, totalCount) = await _mediator.Send(new GetOrdersQuery(page, size), cancellationToken);

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

        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.GetById - Id: {id}", id);

            var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
            var apiResp = new ApiResponse<object>(order, 200, "Request successful");
            return Ok(apiResp);

        }

        // CLient only needs to send a List consisting of products' id: [1, 2, 1]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<object>>> CreateOrder([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Run to OrdersController.CreateOrder - List Of Product Id: {@Ids}", request.ProductIds);
            var order = await _mediator.Send(new CreateOrderCommand(request.ProductIds, request.Email), cancellationToken);
            var apiResp = new ApiResponse<object>(order, 201, "Created");
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, apiResp);
        }

        [HttpPost("{orderId}/products/{productId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> AddProductToOrder(int orderId, int productId, [FromQuery] int quantity = 1, CancellationToken cancellationToken = default)
        {

            var result = await _mediator.Send(new AddProductToOrderCommand(orderId, productId, quantity), cancellationToken);
            if (result) return NoContent();
            return BadRequest("Could not add product to order.");

        }

        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _mediator.Send(new DeleteOrderCommand(id), cancellationToken);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
