using System.Reflection;
using DeliveryControlCenter.Application.Catalog;
using DeliveryControlCenter.Domain.Catalog;
using DeliveryControlCenter.Infrastructure.Persistence;
using DeliveryControlCenter.Web.Services;
using NetArchTest.Rules;

namespace DeliveryControlCenter.ArchitectureTests;

/// <summary>
/// These rules are the machine-checked version of the layering diagram. They exist so an agent
/// cannot quietly introduce a shortcut that a human reviewer would have to spot by eye.
/// </summary>
public sealed class LayeringTests
{
    private static readonly Assembly Domain = typeof(Coffee).Assembly;
    private static readonly Assembly Application = typeof(CatalogQueries).Assembly;
    private static readonly Assembly Infrastructure = typeof(DeliveryDbContext).Assembly;

    [Fact]
    public void Domain_DependsOnNothingElseInTheSolution()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Application",
                "DeliveryControlCenter.Infrastructure",
                "DeliveryControlCenter.Contracts",
                "DeliveryControlCenter.Api",
                "DeliveryControlCenter.Web",
                "DeliveryControlCenter.Worker")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Domain_DoesNotDependOnEntityFrameworkOrAspNet()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Npgsql", "StackExchange.Redis")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrHosts()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Infrastructure",
                "DeliveryControlCenter.Api",
                "DeliveryControlCenter.Web",
                "DeliveryControlCenter.Worker")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Application_TalksToInfrastructureOnlyThroughPorts()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "StackExchange.Redis", "System.Net.Http")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnHosts()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Api",
                "DeliveryControlCenter.Web",
                "DeliveryControlCenter.Worker")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Web_NeverReachesTheDatabaseOrGitHubDirectly()
    {
        // The front end must go through the API so every read stays audited and rate limited.
        var result = Types.InAssembly(typeof(CoffeeStoreApiClient).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Infrastructure",
                "DeliveryControlCenter.Application",
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Octokit",
                "DeliveryControlCenter.Infrastructure.GitHub")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void CoffeeDomain_DoesNotDependOnDeliveryHarnessNamespaces()
    {
        var result = Types.InAssembly(Domain)
            .That()
            .ResideInNamespace("DeliveryControlCenter.Domain.Catalog")
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Domain.Delivery",
                "DeliveryControlCenter.Domain.Gates",
                "DeliveryControlCenter.Domain.Evidence",
                "DeliveryControlCenter.Domain.Sync")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void CatalogApplication_DoesNotDependOnDeliveryHarnessNamespaces()
    {
        var result = Types.InAssembly(Application)
            .That()
            .ResideInNamespace("DeliveryControlCenter.Application.Catalog")
            .ShouldNot()
            .HaveDependencyOnAny(
                "DeliveryControlCenter.Application.Delivery",
                "DeliveryControlCenter.Infrastructure.GitHub")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }
}
