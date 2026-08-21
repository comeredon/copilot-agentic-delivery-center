using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeliveryControlCenter.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF tooling. It never connects, so migrations can be generated without a
/// running database or any credential.
/// </summary>
public sealed class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Port=5432;Database=deliverydb;Username=design;Password=design";

    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(DesignTimeConnectionString)
            .Options;

        return new DeliveryDbContext(options);
    }
}
