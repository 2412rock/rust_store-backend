using Microsoft.AspNetCore.Mvc;

namespace Rust_store_backend.Controllers
{
    [Route("api")]
    public class HealthCheck: Controller
    {
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            return Ok("success");
        }
    }
}
