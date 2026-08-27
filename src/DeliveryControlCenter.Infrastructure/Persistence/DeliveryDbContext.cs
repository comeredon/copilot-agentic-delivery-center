using DeliveryControlCenter.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.Infrastructure.Persistence;

public sealed class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
{
    public DbSet<Coffee> Coffees => Set<Coffee>();

    public DbSet<CoffeeDescriptor> CoffeeDescriptors => Set<CoffeeDescriptor>();

    public DbSet<BrewRecommendation> BrewRecommendations => Set<BrewRecommendation>();

    public DbSet<CoffeeImage> CoffeeImages => Set<CoffeeImage>();

    public DbSet<Producer> Producers => Set<Producer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
    }
}
