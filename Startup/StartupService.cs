using Rust_store_backend.Services;

namespace Rust_store_backend.Startup
{
    public class StartupService
    {
        private readonly RCONService _rcon;
        public StartupService(RCONService rcon)
        {
            _rcon = rcon;
        }

        private void AuthorizeShop()
        {
            _rcon.AuthoriseShop();
        }

        public void Initialize()
        {
           // AuthorizeShop();
        }
    }
}
