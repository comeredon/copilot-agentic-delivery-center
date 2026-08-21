using System.Reflection;
using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Infrastructure.Persistence;
using NetArchTest.Rules;

namespace DeliveryControlCenter.ArchitectureTests;

public sealed class ConventionTests
{
    private static readonly Assembly Domain = typeof(Entity).Assembly;
    private static readonly Assembly Infrastructure = typeof(DeliveryDbContext).Assembly;

    [Fact]
    public void Aggregates_AreSealedSoBehaviourCannotBeOverridden()
    {
        var result = Types.InAssembly(Domain)
            .That()
            .Inherit(typeof(Entity))
            .Should()
            .BeSealed()
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void RepositoryImplementations_AreInternal()
    {
        // Only the DI registration should be able to see them; callers depend on the port.
        var result = Types.InAssembly(Infrastructure)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .NotBePublic()
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Domain_HasNoPublicSetters()
    {
        var offenders = Domain.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity).IsAssignableFrom(t))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p => p.SetMethod is { IsPublic: true } && !p.SetMethod.IsInitOnly())
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();

        offenders.ShouldBeEmpty();
    }
}

internal static class PropertyExtensions
{
    /// <summary><c>init</c> accessors are compiled as setters with a required modifier.</summary>
    public static bool IsInitOnly(this MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers()
            .Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit");
}
