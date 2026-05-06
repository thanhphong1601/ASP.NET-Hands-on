using Microsoft.AspNetCore.Mvc;
using Refit;

namespace ASP.NET_Hands_on.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MockController : ControllerBase
    {
        private static int count = 0;

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            if (count < 2)
            {
                Console.WriteLine($"[Mock API] Nhận request lần {count}: Cố tình giả lập lỗi sập Server!");
                count++;
                return StatusCode(500, new { Message = "This is a mock error response." });
            }
            Console.WriteLine($"[Mock API] Nhận request lần {count}: Xử lý thành công!");
            count = 0;

            return Ok(9);
        }
    }
}
