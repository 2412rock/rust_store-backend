using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Rust_store_backend.Models.DB
{
    public class RustDBContext : DbContext
    {
        public DbSet<DBUser> Users { get; set; }
        public DbSet<DBOrder> Orders { get; set; }
        public DbSet<DBProduct> Products { get; set; }
        public DbSet<DBOrderItem> OrderItems { get; set; }

        public RustDBContext(DbContextOptions<RustDBContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring User and Order relationships
            modelBuilder.Entity<DBUser>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring Order and OrderItem relationships
            modelBuilder.Entity<DBOrder>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring Product and OrderItem relationships
            modelBuilder.Entity<DBProduct>()
                .HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Setting default values for fields
            modelBuilder.Entity<DBOrder>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<DBOrder>()
                .Property(o => o.TransactionFinalized)
                .HasDefaultValue(false);

            modelBuilder.Entity<DBOrder>()
                .Property(o => o.TransactionFinalizedButPlayerDidNotGet)
                .HasDefaultValue(false);
            // Assuming you want Subtotal to be calculated based on quantity and price
        }
    }

    //public class DBOrder
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int Id { get; set; }
    //    public string OrderId { get; set; }
    //    public int Amount { get; set; }
    //    public string SteamId { get; set; }
    //    public bool TransactionFinalized { get; set; }
    //    public bool TransactionFinalizedButPlayerDidNotGet { get; set; }
    //}

    public class DBUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string SteamId { get; set; }

        // Navigation property for related orders
       public ICollection<DBOrder> Orders { get; set; } = new List<DBOrder>();
    }
    public class DBOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Game { get; set; }

        public int UserId { get; set; }

        public int TotalNumberOfItems { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool TransactionFinalized { get; set; } = false;

        public bool TransactionFinalizedButPlayerDidNotGet { get; set; } = false;
        public int Total { get; set; }
        public string PaypalOrderId { get; set; }

        // Navigation properties
       public DBUser User { get; set; } = null!;
       public ICollection<DBOrderItem> OrderItems { get; set; } = new List<DBOrderItem>();
    }
    public class DBProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Price { get; set; }

        // Navigation property
        public ICollection<DBOrderItem> OrderItems { get; set; } = new List<DBOrderItem>();
    }

    public class DBOrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int NumberOfItems { get; set; }

        public int Subtotal { get; set; }

        // Navigation properties
        public DBOrder Order { get; set; } = null!;
        public DBProduct Product { get; set; } = null!;
    }
}