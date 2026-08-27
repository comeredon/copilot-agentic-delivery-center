using DeliveryControlCenter.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryControlCenter.Infrastructure.Persistence.Configurations;

internal static class EntityBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ApplyEntityConventions<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.Version).IsConcurrencyToken().HasConversion<long>();
        return builder;
    }
}
