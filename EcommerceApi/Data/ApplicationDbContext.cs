// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;

namespace EcommerceApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Ware> Waren => Set<Ware>();

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ware>()
            .Property(p => p.Price)
            .HasColumnType("numeric");
    }
}
