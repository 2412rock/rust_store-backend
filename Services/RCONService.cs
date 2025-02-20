using Rcon;
using Rust_store_backend.Models.DB;
namespace Rust_store_backend.Services
{
    public class RCONService
    {
        private readonly RustDBContext _context;
        public RCONService(RustDBContext context)
        {
            _context = context;
        }
        public async Task DepositCommand(int amount, string steamId)
        {
            // var results = _context.Orders.Where(e => e.SteamId == "POTUS").ToList();
            string rconHost = "10.244.17.98"; // Change to your server IP
            int rconPort = 28016;          // Default RCON port
            string rconPassword = "your_rcon_password";
            var client = new RconClient();
            var connected = await client.ConnectAsync(rconHost, rconPort);
            var authenticated = await client.AuthenticateAsync(rconPassword);
            if (client.Authenticated)
            {
                string response = await client.SendCommandAsync($"deposit {steamId} {amount}");
                Console.WriteLine("Server Response: " + response);
            }
            else
            {
                Console.WriteLine("Failed to connect to RCON.");
            }
        }
    }
}