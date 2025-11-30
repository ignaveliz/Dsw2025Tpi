using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data
{
    // Contexto principal: Identity + dominio
    public class AuthenticateContext : IdentityDbContext<User>
    {
        public AuthenticateContext(DbContextOptions<AuthenticateContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ====== Tablas de Identity renombradas ======

            // Usuarios (User de Identity)
            builder.Entity<User>(b =>
            {
                b.ToTable("Usuarios");
            });

            // Roles
            builder.Entity<IdentityRole>(b =>
            {
                b.ToTable("Roles");
            });

            // UsuariosRoles (UserRole)
            builder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UsuariosRoles");
            });

            // Claims de usuario
            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UsuariosClaims");
            });

            // Logins externos
            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UsuariosLogins");
            });

            // Claims de rol
            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RolesClaims");
            });

            // Tokens de usuario
            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UsuariosTokens");
            });

            // ====== Configuración dominio ======

            // PRODUCT
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.Property(p => p.Sku)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(p => p.Sku)
                      .IsUnique();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(60);

                entity.Property(p => p.CurrentUnitPrice)
                      .IsRequired()
                      .HasPrecision(15, 2);
            });

            // CUSTOMER (queda suelto, ya no está ligado a Orders)
            builder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.Property(c => c.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ORDER -> ahora solo FK a User (Usuarios)
            builder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                // Valor por defecto de estado
                entity.Property(o => o.Status)
                      .HasDefaultValue(OrderStatus.Pending);

                // Relación: Order.UserID -> User.Id (tabla Usuarios)
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserID)
                      .IsRequired();

                // Campo calculado en memoria
                entity.Ignore(o => o.TotalAmount);
            });

            // ORDER ITEM
            builder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.Property(i => i.UnitPrice)
                      .IsRequired()
                      .HasPrecision(15, 2);

                entity.Property(i => i.Quantity)
                      .IsRequired();

                // FK a Product
                entity.HasOne(i => i.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(i => i.ProductId)
                      .IsRequired();

                // FK a Order
                entity.HasOne(i => i.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(i => i.OrderId)
                      .IsRequired();

                // Subtotal calculado en código
                entity.Ignore(i => i.SubTotal);
            });
        }
    }
}
