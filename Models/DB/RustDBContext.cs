using Microsoft.EntityFrameworkCore;
using PaypalServerSdk.Standard.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Rust_store_backend.Models.DB
{
    public class RustDBContext : DbContext
    {
        public DbSet<OrderDB> Orders { get; set; }
        public RustDBContext(DbContextOptions<RustDBContext> options) : base(options)
        { }
    }

    public class OrderDB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public string SteamId { get; set; }
    }
}