using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Http;

namespace Rust_store_backend.Controllers
{
   [Route("auth")]
public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "3916E29B764656240B547A54EBFC4540";  // Replace with your Steam API key

    public AccountController(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }
         
    [HttpGet("profile")]
    public async Task<IActionResult> GetSteamProfile([FromQuery] string steamId)
    {
      string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_apiKey}&steamids={steamId}"; 
      var response = await _httpClient.GetStringAsync(url);
      return Ok(response);
    }
  }
}
