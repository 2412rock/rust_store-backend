﻿using Microsoft.AspNetCore.Mvc;
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
            var response = await _rcon.RawCommandAsync(command.CommandText, command.Password);
            return Ok(response);
        }
    }

    public class Command
    {
        public string CommandText { get; set; }
        public string Password { get; set; }
    }
}
