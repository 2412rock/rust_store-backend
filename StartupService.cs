using Rust_store_backend.Models.DB;

namespace Rust_store_backend
{
    public class StartupService
    {
        private readonly RustDBContext _context;
        private readonly List<(string Name, int Price)> Products = new List<(string, int)>
        {
            ("120 shop points", 1),
            ("500 shop points", 3),
            ("1000 shop points", 5),
            ("2300 shop points", 10),
            ("7000 shop points", 30),
            ("1 crafting permit", 1),
            ("5 crafting permits", 3),
            ("10 crafting permits", 5),
            ("23 crafting permits", 10),
            ("70 crafting permits", 30)
        };

        public StartupService(RustDBContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            Products.ForEach(product =>
            {
                var any = _context.Products.Any(element => element.ProductName == product.Name);
                if (!any)
                {
                    _context.Products.Add(new DBProduct()
                    {
                        ProductName = product.Name,
                        Price = product.Price
                    });
                }
            });
            _context.SaveChanges();
        }
    }
}
