using Microsoft.AspNetCore.Mvc;
using Rust_store_backend.Services;

namespace Rust_store_backend.Controllers
{
    [Route("rcon")]
    public class RCONController: Controller
    {
        private readonly RCONService _rcon;
        public RCONController(RCONService rcon)
        {
            _rcon = rcon;
        }

        [HttpPost("command")]
        public async Task<IActionResult> ExecuteCommand([FromBody] Command command)
        {
            await _rcon.RawCommand(command.CommandText);
            return Ok("success");
        }
    }

    public class Command
    {
        public string CommandText { get; set; }
    }
}
