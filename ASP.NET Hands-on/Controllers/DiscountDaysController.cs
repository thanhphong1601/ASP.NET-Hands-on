using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Interface;
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
        [ProducesResponseType(typeof(Model.ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Model.ApiResponse<object>>> GetAll(CancellationToken cancellationToken)
        {
            var items = await _service.GetAllAsync(cancellationToken);
            var apiResp = new Model.ApiResponse<object>(items, 200, "Request successful");
            return Ok(apiResp);
        }

        // POST: api/discountdays
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Model.ApiResponse<object>), StatusCodes.Status201Created)]
        public async Task<ActionResult<Model.ApiResponse<object>>> Create([FromBody] DiscountDayRequestDto dto, CancellationToken cancellationToken)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            var apiResp = new Model.ApiResponse<object>(created, 201, "Created");
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, apiResp);
        }
    }
}
