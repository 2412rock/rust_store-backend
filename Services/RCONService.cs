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

        private async Task<RconClient> CreateClientAsync(bool usePassword = false, string password ="")
        {
            // var results = _context.Orders.Where(e => e.SteamId == "POTUS").ToList();
            string rconHost = Environment.GetEnvironmentVariable("DB_IP"); // Change to your server IP
            int rconPort = 28016;          // Default RCON port
            string rconPassword = "";
            if (usePassword)
            {
                rconPassword = password;
            }
            else
            {
                rconPassword = Environment.GetEnvironmentVariable("RCON_PASS");
            }
            var client = new RconClient();
            var connected = await client.ConnectAsync(rconHost, rconPort);
            var authenticated = await client.AuthenticateAsync(rconPassword);
            if (!client.Authenticated)
            {
                throw new Exception("Failed to connect to rcon");
            }
            return client;
        }

        private async Task<RconClient> EcoCreateClientAsync(bool usePassword = false, string password = "")
        {
            // var results = _context.Orders.Where(e => e.SteamId == "POTUS").ToList();
            string rconHost = Environment.GetEnvironmentVariable("DB_IP"); // Change to your server IP
            int rconPort = 3002;          // Default RCON port
            string rconPassword = "";
            if (usePassword)
            {
                rconPassword = password;
            }
            else
            {
                rconPassword = Environment.GetEnvironmentVariable("RCON_PASS");
            }
            var client = new RconClient();
            var connected = await client.ConnectAsync(rconHost, rconPort);
            var authenticated = await client.AuthenticateAsync(rconPassword);
            if (!client.Authenticated)
            {
                throw new Exception("Failed to connect to rcon");
            }
            return client;
        }

        private RconClient CreateClient(bool usePassword = false, string password = "")
        {
            // var results = _context.Orders.Where(e => e.SteamId == "POTUS").ToList();
            string rconHost = Environment.GetEnvironmentVariable("DB_IP"); // Change to your server IP
            int rconPort = 28016;          // Default RCON port
            string rconPassword = "";
            if (usePassword)
            {
                rconPassword = password;
            }
            else
            {
                rconPassword = Environment.GetEnvironmentVariable("RCON_PASS");
            }
            var client = new RconClient();
            var connected = client.Connect(rconHost, rconPort);
            var authenticated =  client.Authenticate(rconPassword);
            if (!client.Authenticated)
            {
                throw new Exception("Failed to connect to rcon");
            }
            return client;
        }
        public async Task<string> DepositCommandAsync(int amount, string steamId)
        {
            var client = await CreateClientAsync();
            string response = await client.SendCommandAsync($"deposit {steamId} {amount}");
            return response;
        }
        public async Task<string> RawCommandAsync(string command, string password)
        {
            var client = await CreateClientAsync(usePassword:true, password);
            string response = await client.SendCommandAsync(command);
            return response;
        }

        public async Task<string> EcoRawCommandAsync(string command)
        {
            var client = await EcoCreateClientAsync(usePassword: false);
            string response = await client.SendCommandAsync(command);
            return response;
        }

        public string RawCommand(string command)
        {
            var client = CreateClient(usePassword: false);
            string response = client.SendCommand(command);
            return response;
        }


        public void AuthoriseShop()
        {
            string rconHost = Environment.GetEnvironmentVariable("DB_IP"); // Change to your server IP
            int rconPort = 28016;          // Default RCON port
            string rconPassword = Environment.GetEnvironmentVariable("RCON_PASS");
            var client = new RconClient();
            var connected =  client.Connect(rconHost, rconPort);
            var authenticated = client.Authenticate(rconPassword);
            if (client.Authenticated)
            {
                string response = client.SendCommand($"oxide.grant group default guishop.use");
                Console.WriteLine("Server Response: " + response);
            }
            else
            {
                Console.WriteLine("Failed to connect to RCON.");
            }
        }
    }
}