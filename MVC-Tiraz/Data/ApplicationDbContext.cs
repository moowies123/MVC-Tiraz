using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVC_Tiraz.Models.TIRAZ.Models;

namespace TIRAZ.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Wishlist> Wishlists { get; set; } = null!;
        public DbSet<Has> HasCustomers { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Contains> Contains { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Tiraz;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Has>()
                .HasKey(h => new { h.UserId, h.ProductId });
            builder.Entity<Has>()
                .HasOne(h => h.User)
                .WithMany(u => u.HasProducts)
                .HasForeignKey(h => h.UserId);
            builder.Entity<Has>()
                .HasOne(h => h.Product)
                .WithMany(p => p.HasCustomers)
                .HasForeignKey(h => h.ProductId);
            
            builder.Entity<Contains>()
                .HasKey(c => new { c.OrderId, c.ProductId });
            builder.Entity<Contains>()
                .HasOne(c => c.Order)
                .WithMany(o => o.Products)
                .HasForeignKey(c => c.OrderId);
            builder.Entity<Contains>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(c => c.ProductId);
            builder.Entity<Wishlist>()
                .HasKey(w => new { w.UserId, w.ProductId });
            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId);
            builder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId);
            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(o => o.OrderId);

        }
    }
}
