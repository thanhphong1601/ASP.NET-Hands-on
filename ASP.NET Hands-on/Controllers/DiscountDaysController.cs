using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountDaysController : ControllerBase
    {
        private readonly IDiscountDayService _service;

        public DiscountDaysController(IDiscountDayService service)
        {
            _service = service;
        }

        // GET: api/discountdays
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetAll(CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            var apiResp = new ApiResponse<object>(items, 200, "Request successful");
            return Ok(apiResp);
        }

        // POST: api/discountdays
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] DiscountDayRequestDto dto, CancellationToken cancellationToken)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            var apiResp = new ApiResponse<object>(created, 201, "Created");
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, apiResp);
        }
    }
}
