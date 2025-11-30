using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data;

public class AuthenticateContext: IdentityDbContext<User>
{
    public AuthenticateContext(DbContextOptions<AuthenticateContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(b => { b.ToTable("Usuarios"); });
        builder.Entity<IdentityRole>(b => { b.ToTable("Roles"); });
        builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UsuariosRoles"); });
        builder.Entity<IdentityUserClaim<string>>(b => { b.ToTable("UsuariosClaims"); });
        builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UsuariosLogins"); });
        builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RolesClaims"); });
        builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UsuariosTokens"); });

        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(20);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(60);
            entity.Property(p => p.CurrentUnitPrice).IsRequired().HasPrecision(15, 2);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status).HasDefaultValue(OrderStatus.Pending);
            entity.HasOne(o => o.User)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.UserID);

            entity.Ignore(o => o.TotalAmount);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).IsRequired().HasPrecision(15, 2);
            entity.Property(i => i.Quantity).IsRequired();

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(i => i.ProductId);

            entity.HasOne(i => i.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(i => i.OrderId);

            entity.Ignore(i => i.SubTotal);
        });
    }
}
