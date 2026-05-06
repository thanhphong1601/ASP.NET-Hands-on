using ASP.NET_Hands_on.Application.CQRS.Customers;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Domain.Model;
using Domain.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/customers/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var c = await _mediator.Send(new GetCustomerQuery(id), cancellationToken);
            if (c == null) return NotFound();
            return Ok(new ApiResponse<CustomerDto>(c, 200, "Request successful"));
        }

        // GET /api/customers?pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 30)
        {

            var page = pageNumber > 0 ? pageNumber : 1;
            var size = pageSize > 0 ? pageSize : 30;

            var (items, totalCount) = await _mediator.Send(new GetCustomersQuery(page, size), cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)size); //int size

            var pageResult = new
            {
                PageNumber = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };


            return Ok(new ApiResponse<object>(pageResult, 200, "Request successful"));
        }

        // POST /api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateRequest req, CancellationToken cancellationToken)
        {
            var created = await _mediator.Send(new CreateCustomerCommand(req), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, new ApiResponse<object>(created, 201, "Created"));
        }

        // PATCH /api/customers/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] CustomerPatchRequest patch, CancellationToken cancellationToken)
        {
            var updated = await _mediator.Send(new PatchCustomerCommand(id, patch), cancellationToken);
            return Ok(new ApiResponse<object>(updated, 200, "Patched"));
        }

        // GET /api/customers/5/orders?pageNumber=1&pageSize=20
        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetOrders(int id, CancellationToken cancellationToken, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 30)
        {
            var page = pageNumber > 0 ? pageNumber : 1;
            var size = pageSize > 0 ? pageSize : 30;

            var (items, totalCount) = await _mediator.Send(new GetCustomerOrdersQuery(id, page, size), cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            var pageResult = new
            {
                PageNumber = page,
                PageSize = size,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };

            //nên nhét 2 totalPages và pageResult vào 1 helper class để tái sử dụng, tránh lặp code

            return Ok(new ApiResponse<object>(pageResult, 200, "Request successful"));
        }
    }
}
