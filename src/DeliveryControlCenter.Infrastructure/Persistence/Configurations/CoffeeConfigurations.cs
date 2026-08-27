using DeliveryControlCenter.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryControlCenter.Infrastructure.Persistence.Configurations;

public sealed class CoffeeConfiguration : IEntityTypeConfiguration<Coffee>
{
    public void Configure(EntityTypeBuilder<Coffee> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("coffees").ApplyEntityConventions();
        builder.HasIndex(coffee => coffee.Slug).IsUnique();
        builder.HasIndex(coffee => new { coffee.Status, coffee.Country });
        builder.Property(coffee => coffee.Slug).HasMaxLength(128).IsRequired();
        builder.Property(coffee => coffee.Name).HasMaxLength(256).IsRequired();
        builder.Property(coffee => coffee.ShortDescription).HasMaxLength(512);
        builder.Property(coffee => coffee.Story).HasMaxLength(4096);
        builder.Property(coffee => coffee.Country).HasMaxLength(128).IsRequired();
        builder.Property(coffee => coffee.Region).HasMaxLength(128);
        builder.Property(coffee => coffee.Subregion).HasMaxLength(128);
        builder.Property(coffee => coffee.FarmName).HasMaxLength(256);
        builder.Property(coffee => coffee.Process).HasConversion<string>().HasMaxLength(32);
        builder.Property(coffee => coffee.DryingMethod).HasMaxLength(128);
        builder.Property(coffee => coffee.HarvestLabel).HasMaxLength(128);
        builder.Property(coffee => coffee.RoastLevel).HasConversion<string>().HasMaxLength(32);
        builder.Property(coffee => coffee.RoastIntent).HasConversion<string>().HasMaxLength(32);
        builder.Property(coffee => coffee.CupScore).HasPrecision(4, 1);
        builder.Property(coffee => coffee.AcidityType).HasMaxLength(64);
        builder.Property(coffee => coffee.LotCode).HasMaxLength(64);
        builder.Property(coffee => coffee.ImportPartner).HasMaxLength(128);
        builder.Property(coffee => coffee.FobPriceUsdPerLb).HasPrecision(8, 2);
        builder.Property(coffee => coffee.Status).HasConversion<string>().HasMaxLength(24);

        builder.HasOne(coffee => coffee.Producer)
            .WithMany()
            .HasForeignKey(coffee => coffee.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(coffee => coffee.Descriptors)
            .WithOne()
            .HasForeignKey(descriptor => descriptor.CoffeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(coffee => coffee.Descriptors).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(coffee => coffee.Variants)
            .WithOne()
            .HasForeignKey(variant => variant.CoffeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(coffee => coffee.Variants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(coffee => coffee.BrewRecommendations)
            .WithOne()
            .HasForeignKey(recipe => recipe.CoffeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(coffee => coffee.BrewRecommendations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(coffee => coffee.Images)
            .WithOne()
            .HasForeignKey(image => image.CoffeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(coffee => coffee.Images).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ProducerConfiguration : IEntityTypeConfiguration<Producer>
{
    public void Configure(EntityTypeBuilder<Producer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("producers").ApplyEntityConventions();
        builder.HasIndex(producer => producer.Slug).IsUnique();
        builder.Property(producer => producer.Slug).HasMaxLength(128).IsRequired();
        builder.Property(producer => producer.Name).HasMaxLength(256).IsRequired();
        builder.Property(producer => producer.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(producer => producer.Country).HasMaxLength(128).IsRequired();
        builder.Property(producer => producer.Region).HasMaxLength(128);
        builder.Property(producer => producer.Story).HasMaxLength(4096);
        builder.Property(producer => producer.FarmSizeHectares).HasPrecision(8, 2);
        builder.Property(producer => producer.ImageUrl).HasMaxLength(1024);
        builder.Property(producer => producer.ImageAlt).HasMaxLength(512);
    }
}

public sealed class CoffeeDescriptorConfiguration : IEntityTypeConfiguration<CoffeeDescriptor>
{
    public void Configure(EntityTypeBuilder<CoffeeDescriptor> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("coffee_descriptors").ApplyEntityConventions();
        builder.HasIndex(descriptor => new { descriptor.CoffeeId, descriptor.Kind, descriptor.Value }).IsUnique();
        builder.Property(descriptor => descriptor.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(descriptor => descriptor.Value).HasMaxLength(128).IsRequired();
    }
}

public sealed class CoffeeVariantConfiguration : IEntityTypeConfiguration<CoffeeVariant>
{
    public void Configure(EntityTypeBuilder<CoffeeVariant> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("coffee_variants").ApplyEntityConventions();
        builder.HasIndex(variant => variant.Sku).IsUnique();
        builder.Property(variant => variant.Sku).HasMaxLength(64).IsRequired();
        builder.Property(variant => variant.Grind).HasConversion<string>().HasMaxLength(32);
        builder.Property(variant => variant.PriceUsd).HasPrecision(8, 2);
    }
}

public sealed class BrewRecommendationConfiguration : IEntityTypeConfiguration<BrewRecommendation>
{
    public void Configure(EntityTypeBuilder<BrewRecommendation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("brew_recommendations").ApplyEntityConventions();
        builder.HasIndex(recipe => new { recipe.CoffeeId, recipe.Method }).IsUnique();
        builder.Property(recipe => recipe.Method).HasConversion<string>().HasMaxLength(32);
        builder.Property(recipe => recipe.DoseGrams).HasPrecision(6, 1);
        builder.Property(recipe => recipe.YieldGrams).HasPrecision(7, 1);
        builder.Property(recipe => recipe.GrindDescriptor).HasMaxLength(128);
        builder.Property(recipe => recipe.Notes).HasMaxLength(1024);
        builder.Ignore(recipe => recipe.Ratio);
    }
}

public sealed class CoffeeImageConfiguration : IEntityTypeConfiguration<CoffeeImage>
{
    public void Configure(EntityTypeBuilder<CoffeeImage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("coffee_images").ApplyEntityConventions();
        builder.Property(image => image.Url).HasMaxLength(1024).IsRequired();
        builder.Property(image => image.AltText).HasMaxLength(512).IsRequired();
    }
}
