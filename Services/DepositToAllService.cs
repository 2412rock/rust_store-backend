using Rcon;
using System.Text.RegularExpressions;

namespace Rust_store_backend.Services
{
    public class DepositToAllService
    {
        public DepositToAllService()
        {
            Thread thread = new Thread(new ThreadStart(MatchCheck));
            thread.Start();
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
            var authenticated = client.Authenticate(rconPassword);
            if (!client.Authenticated)
            {
                throw new Exception("Failed to connect to rcon");
            }
            return client;
        }

        public string RawCommand(RconClient client, string command)
        {
            string response = client.SendCommand(command);
            return response;
        }

        private void MatchCheck()
        {
            while (true)
            {
                try
                {
                    using(var client = CreateClient())
                    {
                        RawCommand(client, "deposit * 20");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Unable to send command");
                }
                
                Thread.Sleep(1200000); // 1200000 Sleep for 20 minutes (1,200,000 milliseconds)
            }

        }
    }
}
