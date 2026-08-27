using DeliveryControlCenter.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.Infrastructure.Persistence;

public static class StoreDataSeeder
{
    private static readonly DateOnly ReferenceRoastDate = new(2026, 8, 18);

    public static async Task SeedAsync(DeliveryDbContext db, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (await db.Coffees.AnyAsync(cancellationToken))
        {
            return;
        }

        var producers = ProducerSeeds().Select(CreateProducer).ToDictionary(producer => producer.Slug);
        await db.Producers.AddRangeAsync(producers.Values, cancellationToken);

        var coffees = CoffeeSeeds()
            .Select(seed => CreateCoffee(seed, producers[seed.ProducerSlug].Id))
            .ToList();

        await db.Coffees.AddRangeAsync(coffees, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Producer CreateProducer(ProducerSeed seed) => Producer.Create(
        seed.Slug,
        seed.Name,
        seed.Kind,
        seed.Country,
        seed.Region,
        seed.Story,
        seed.ElevationMasl,
        seed.FarmSizeHectares,
        seed.PartnerSince,
        seed.ImageUrl,
        seed.ImageAlt,
        seed.Id);

    private static Coffee CreateCoffee(CoffeeSeed seed, Guid producerId)
    {
        var coffee = Coffee.Create(
            producerId,
            seed.Slug,
            seed.Name,
            seed.ShortDescription,
            seed.Story,
            seed.Country,
            seed.Region,
            seed.Subregion,
            seed.FarmName,
            seed.ElevationMinMasl,
            seed.ElevationMaxMasl,
            seed.Process,
            seed.FermentationHours,
            seed.DryingMethod,
            seed.HarvestLabel,
            seed.RoastLevel,
            seed.RoastIntent,
            ReferenceRoastDate.AddDays(seed.RoastDayOffset),
            seed.RestDays,
            seed.CupScore,
            seed.Acidity,
            seed.Body,
            seed.Sweetness,
            seed.AcidityType,
            seed.LotCode,
            seed.ImportPartner,
            seed.FobPriceUsdPerLb,
            seed.Id);

        foreach (var note in seed.TastingNotes)
        {
            coffee.AddDescriptor(DescriptorKind.TastingNote, note);
        }

        foreach (var variety in seed.Varieties)
        {
            coffee.AddDescriptor(DescriptorKind.Variety, variety);
        }

        foreach (var certification in seed.Certifications)
        {
            coffee.AddDescriptor(DescriptorKind.Certification, certification);
        }

        coffee.AddImage(seed.HeroImageUrl, seed.HeroImageAlt, isHero: true);
        coffee.AddVariant($"{seed.SkuPrefix}-250-WB", 250, GrindOption.WholeBean, seed.PriceUsd, seed.Stock);
        coffee.AddVariant($"{seed.SkuPrefix}-250-PO", 250, GrindOption.PourOver, seed.PriceUsd + 1m, seed.Stock);
        coffee.AddVariant($"{seed.SkuPrefix}-1K-WB", 1000, GrindOption.WholeBean, seed.PriceUsd * 3.45m, Math.Max(4, seed.Stock / 4));
        coffee.AddBrewRecommendation(
            BrewMethod.V60,
            15m,
            250m,
            seed.FilterTemperatureC,
            180,
            "Medium-fine, like table salt",
            "Bloom with 45 g for 40 seconds, then pour in slow concentric pulses.");
        coffee.AddBrewRecommendation(
            BrewMethod.AeroPress,
            16m,
            230m,
            seed.FilterTemperatureC - 3,
            105,
            "Medium, slightly finer than drip",
            "Steep for 75 seconds, swirl, then press gently for 30 seconds.");
        coffee.AddBrewRecommendation(
            BrewMethod.Espresso,
            18m,
            42m,
            94,
            30,
            "Fine; adjust until the shot lands between 28 and 32 seconds",
            "Rest the coffee fully before dialing in. Target a syrupy, transparent finish.");
        coffee.Publish();
        return coffee;
    }

    private static IReadOnlyList<ProducerSeed> ProducerSeeds() =>
    [
        new(Guid.Parse("10000000-0000-7000-8000-000000000001"), "halo-beriti", "Halo Beriti", ProducerKind.Cooperative, "Ethiopia", "Gedeb, Gedeo", "Seven hundred smallholders deliver ripe cherry to Halo Beriti, where cool nights and meticulous density sorting preserve the florals for which southern Ethiopia is known.", 2150, null, 2019, "https://images.unsplash.com/photo-1442512595331-e89e73853f31?auto=format&fit=crop&w=1400&q=85", "Coffee cherries ripening among green leaves in Ethiopia"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000002"), "finca-las-flores", "Finca Las Flores", ProducerKind.Farm, "Colombia", "Pitalito, Huila", "The Vergara family pairs careful variety selection with a small on-farm microbiology lab, building expressive lots without losing the clarity of Huila terroir.", 1780, 14.5m, 2021, "https://images.unsplash.com/photo-1498804103079-a6351b050096?auto=format&fit=crop&w=1400&q=85", "A coffee producer inspecting ripe coffee cherries"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000003"), "kii-factory", "Kii Factory", ProducerKind.WashingStation, "Kenya", "Kirinyaga", "Kii serves members of the Rungeto cooperative along the Kii River. Double fermentation and clean mountain water produce the electric fruit character of Kirinyaga.", 1650, null, 2018, "https://images.unsplash.com/photo-1524350876685-274059332603?auto=format&fit=crop&w=1400&q=85", "Coffee beans drying in raised beds at a washing station"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000004"), "finca-la-bolsa", "Finca La Bolsa", ProducerKind.Estate, "Guatemala", "Huehuetenango", "La Bolsa sits in a steep limestone valley where warm days, cold nights, and spring water yield balanced Bourbon lots with vivid stone-fruit sweetness.", 1500, 108m, 2017, "https://images.unsplash.com/photo-1501339847302-ac426a4a7cbb?auto=format&fit=crop&w=1400&q=85", "Mountain coffee farm beneath cloud cover in Guatemala"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000005"), "hacienda-jaramillo", "Hacienda Jaramillo", ProducerKind.Estate, "Panama", "Boquete", "Wind moving across the slopes of Volcan Baru slows cherry maturation at Jaramillo. The Peterson family separates tiny Gesha lots by day and picking team.", 1750, 67m, 2020, "https://images.unsplash.com/photo-1511537190424-bbbab87ac5eb?auto=format&fit=crop&w=1400&q=85", "Misty high-elevation coffee landscape in Panama"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000006"), "gitesi", "Gitesi", ProducerKind.WashingStation, "Rwanda", "Karongi", "Alexis and Aime Gasharu built Gitesi around long-term farmer relationships, organic composting, and meticulous separation of cherries grown above Lake Kivu.", 1760, null, 2019, "https://images.unsplash.com/photo-1511081692775-05d0f180a065?auto=format&fit=crop&w=1400&q=85", "Coffee cherries sorted by hand at a Rwandan washing station"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000007"), "fazenda-ambiental-fortaleza", "Fazenda Ambiental Fortaleza", ProducerKind.Farm, "Brazil", "Mococa, Sao Paulo", "FAF is a regenerative family farm using native shade, biological corridors, and selective harvesting to redefine what Brazilian natural coffee can taste like.", 1100, 90m, 2016, "https://images.unsplash.com/photo-1534778101976-62847782c213?auto=format&fit=crop&w=1400&q=85", "Coffee drying on a patio at a Brazilian farm"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000008"), "wahana-estate", "Wahana Estate", ProducerKind.Estate, "Indonesia", "Sidikalang, North Sumatra", "Wahana maintains a living collection of Indonesian and Ethiopian varieties, pairing wet-hulled tradition with controlled drying for unusual clarity.", 1350, 468m, 2022, "https://images.unsplash.com/photo-1501747315-124a0eaca060?auto=format&fit=crop&w=1400&q=85", "Coffee trees growing under tropical shade in Sumatra"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000009"), "las-lajas", "Las Lajas", ProducerKind.Farm, "Costa Rica", "Sabanilla de Alajuela", "Oscar and Francisca Chacon pioneered measured honey processing in Costa Rica, adjusting mucilage and drying to the weather of each harvest day.", 1450, 38m, 2018, "https://images.unsplash.com/photo-1545665225-b23b99e4d45e?auto=format&fit=crop&w=1400&q=85", "Fresh coffee fruit and leaves at a Costa Rican farm"),
        new(Guid.Parse("10000000-0000-7000-8000-000000000010"), "cruz-loma", "Finca Cruz Loma", ProducerKind.Farm, "Ecuador", "Pichincha", "At Cruz Loma, cold Andean nights extend fermentation and maturation. The farm's small Sidra parcels are processed with restrained inoculation for aromatic precision.", 2080, 12m, 2023, "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=1400&q=85", "Coffee farmer working among high-altitude coffee plants in Ecuador"),
    ];

    private static IReadOnlyList<CoffeeSeed> CoffeeSeeds() =>
    [
        new(Guid.Parse("20000000-0000-7000-8000-000000000001"), "halo-beriti-washed", "Halo Beriti", "Jasmine, white peach, and bergamot with tea-like clarity.", "A slow, cool washed process reveals the altitude of Gedeb: lifted florals first, ripe peach through the middle, and a finish that recalls Earl Grey.", "halo-beriti", "Ethiopia", "Gedeo", "Gedeb", "Halo Beriti", 2050, 2200, CoffeeProcess.Washed, null, "Raised beds, 14 days", "November 2025 - January 2026", RoastLevel.Light, RoastIntent.Filter, -2, 10, 88.4m, 5, 2, 4, "Citric", "ET-HB-26-01", "Osito Coffee", 4.25m, ["Jasmine", "White peach", "Bergamot"], ["74110", "74112"], ["Direct Trade"], "https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=1800&q=88", "Light-roast Ethiopian coffee in a ceramic cup beside jasmine flowers", "HB", 24m, 42, 94),
        new(Guid.Parse("20000000-0000-7000-8000-000000000002"), "banko-gotiti-natural", "Banko Gotiti", "Blueberry preserve, cacao nib, and violet.", "Whole cherries dry slowly under shade and are turned throughout the day. The result is saturated but controlled: fruit-forward without ferment masking the florals.", "halo-beriti", "Ethiopia", "Gedeo", "Gedeb", "Banko Gotiti", 1950, 2100, CoffeeProcess.Natural, null, "Raised beds under shade, 21 days", "December 2025 - February 2026", RoastLevel.Light, RoastIntent.Omni, -1, 12, 87.8m, 4, 3, 5, "Tartaric", "ET-BG-26-04", "Osito Coffee", 4.05m, ["Blueberry", "Cacao nib", "Violet"], ["Kurume", "Dega"], ["Direct Trade"], "https://images.unsplash.com/photo-1497636577773-f1231844b336?auto=format&fit=crop&w=1800&q=88", "Freshly roasted natural Ethiopian coffee beans and blueberry tasting notes", "BG", 23m, 36, 93),
        new(Guid.Parse("20000000-0000-7000-8000-000000000003"), "las-flores-sidra", "Las Flores Sidra", "Passion fruit, ripe mango, and cacao husk.", "Sidra cherries undergo a sealed, temperature-tracked fermentation before a gentle washed finish. It is tropical, high-definition, and unexpectedly clean.", "finca-las-flores", "Colombia", "Huila", "Pitalito", "Finca Las Flores", 1730, 1810, CoffeeProcess.Anaerobic, 72, "Parabolic beds, 18 days", "May - July 2026", RoastLevel.Light, RoastIntent.Omni, 0, 14, 89.1m, 5, 3, 5, "Malic", "CO-LF-SI-2606", "Caravela", 5.80m, ["Passion fruit", "Mango", "Cacao husk"], ["Sidra"], ["Direct Trade"], "https://images.unsplash.com/photo-1512568400610-62da28bc8a13?auto=format&fit=crop&w=1800&q=88", "A bright cup of Colombian Sidra coffee with tropical fruit", "LFS", 32m, 28, 92),
        new(Guid.Parse("20000000-0000-7000-8000-000000000004"), "narino-caturra", "Narino Caturra", "Red apple, panela, and cocoa.", "This high-grown Caturra is a portrait of Narino: structured sweetness, brisk apple acidity, and cocoa warmth that works equally well as filter or espresso.", "finca-las-flores", "Colombia", "Narino", "Buesaco", "El Sauce", 2000, 2150, CoffeeProcess.Washed, null, "Covered raised beds, 16 days", "April - June 2026", RoastLevel.MediumLight, RoastIntent.Omni, -3, 8, 86.2m, 4, 4, 5, "Malic", "CO-NA-CT-2604", "Caravela", 3.15m, ["Red apple", "Panela", "Cocoa"], ["Caturra"], ["Direct Trade"], "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=1800&q=88", "Colombian coffee served with red apple and panela tasting references", "NCT", 21m, 54, 94),
        new(Guid.Parse("20000000-0000-7000-8000-000000000005"), "kii-aa", "Kii AA", "Blackcurrant, pink grapefruit, and brown sugar.", "Kii's double-fermented AA selection has the unmistakable tension of Kirinyaga: juicy blackcurrant, sparkling citrus, and a dense sugar-browning finish.", "kii-factory", "Kenya", "Kirinyaga", "Ngariama", "Kii Factory", 1600, 1800, CoffeeProcess.Washed, null, "Raised beds, 15 days", "October - December 2025", RoastLevel.Light, RoastIntent.Filter, -1, 12, 88.7m, 5, 3, 4, "Phosphoric", "KE-KII-AA-26", "Sucafina", 4.60m, ["Blackcurrant", "Pink grapefruit", "Brown sugar"], ["SL28", "SL34", "Ruiru 11"], ["Rainforest Alliance"], "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=1800&q=88", "Kenyan coffee pouring into a glass server with grapefruit tones", "KII", 26m, 35, 94),
        new(Guid.Parse("20000000-0000-7000-8000-000000000006"), "la-bolsa-bourbon", "La Bolsa Bourbon", "Apricot, almond praline, and orange blossom.", "Protected by limestone ridges, Bourbon ripens slowly at La Bolsa. The cup balances soft florals with praline sweetness and a round, apricot finish.", "finca-la-bolsa", "Guatemala", "Huehuetenango", "La Libertad", "Finca La Bolsa", 1450, 1600, CoffeeProcess.Washed, null, "Patio and raised beds, 14 days", "January - March 2026", RoastLevel.MediumLight, RoastIntent.Omni, -4, 7, 86.8m, 4, 4, 5, "Citric", "GT-LB-BB-26", "Mercanta", 3.45m, ["Apricot", "Almond praline", "Orange blossom"], ["Bourbon", "Caturra"], ["Rainforest Alliance"], "https://images.unsplash.com/photo-1459755486867-b55449bb39ff?auto=format&fit=crop&w=1800&q=88", "Guatemalan coffee with apricot and almond praline notes", "LBB", 22m, 48, 94),
        new(Guid.Parse("20000000-0000-7000-8000-000000000007"), "jaramillo-gesha", "Jaramillo Gesha", "Orange blossom, lemon verbena, and honeydew.", "A tiny day lot from the high Jaramillo slopes. Its aromatics unfold from orange blossom to lemon verbena, with a cooling honeydew sweetness.", "hacienda-jaramillo", "Panama", "Chiriqui", "Boquete", "Hacienda Jaramillo", 1700, 1820, CoffeeProcess.Washed, null, "Raised beds, 12 days", "January - February 2026", RoastLevel.Light, RoastIntent.Filter, 1, 18, 91.3m, 5, 2, 5, "Citric", "PA-JG-D17", "Origin Coffee Lab", 18.75m, ["Orange blossom", "Lemon verbena", "Honeydew"], ["Gesha"], ["Direct Trade"], "https://images.unsplash.com/photo-1445116572660-236099ec97a0?auto=format&fit=crop&w=1800&q=88", "Panamanian Gesha coffee in a delicate glass cup", "JG", 58m, 18, 92),
        new(Guid.Parse("20000000-0000-7000-8000-000000000008"), "gitesi-red-honey", "Gitesi Red Honey", "Raspberry, rooibos, and caramel.", "A measured layer of mucilage remains through drying, adding raspberry sweetness and caramel depth while Gitesi's altitude keeps the finish clean.", "gitesi", "Rwanda", "Western Province", "Karongi", "Gitesi", 1700, 1900, CoffeeProcess.Honey, null, "Raised beds, 18 days", "May - July 2026", RoastLevel.Light, RoastIntent.Omni, -2, 10, 87.3m, 4, 3, 5, "Malic", "RW-GT-RH-26", "Raw Material", 3.90m, ["Raspberry", "Rooibos", "Caramel"], ["Red Bourbon"], ["Direct Trade"], "https://images.unsplash.com/photo-1461988091159-192b6df7054f?auto=format&fit=crop&w=1800&q=88", "Rwandan honey-process coffee with raspberry and caramel references", "GTR", 25m, 34, 93),
        new(Guid.Parse("20000000-0000-7000-8000-000000000009"), "faf-yellow-bourbon", "FAF Yellow Bourbon", "Dried fig, milk chocolate, and cashew.", "Selective hand picking and slow natural drying give this Yellow Bourbon a plush, comforting profile without sacrificing the farm's regenerative transparency.", "fazenda-ambiental-fortaleza", "Brazil", "Sao Paulo", "Mococa", "Fazenda Ambiental Fortaleza", 1000, 1200, CoffeeProcess.Natural, null, "Patio and static boxes, 20 days", "May - August 2026", RoastLevel.Medium, RoastIntent.Espresso, -5, 5, 85.6m, 2, 5, 5, "Malic", "BR-FAF-YB-26", "FAF Coffees", 2.85m, ["Dried fig", "Milk chocolate", "Cashew"], ["Yellow Bourbon"], ["Organic", "Direct Trade"], "https://images.unsplash.com/photo-1521302080334-4bebac2763a6?auto=format&fit=crop&w=1800&q=88", "Brazilian natural coffee with dried fig, chocolate, and cashew", "FAF", 19m, 70, 92),
        new(Guid.Parse("20000000-0000-7000-8000-000000000010"), "wahana-longberry", "Wahana Longberry", "Tamarind, cedar, and dark chocolate.", "Careful wet hulling and a final controlled dry preserve Sumatra's savory depth while revealing tamarind brightness and a long cocoa finish.", "wahana-estate", "Indonesia", "North Sumatra", "Sidikalang", "Wahana Estate", 1250, 1450, CoffeeProcess.WetHulled, null, "Raised beds after wet hulling, 10 days", "June - August 2026", RoastLevel.Medium, RoastIntent.Omni, -4, 6, 85.9m, 4, 5, 4, "Tartaric", "ID-WA-LB-26", "Covoya", 3.10m, ["Tamarind", "Cedar", "Dark chocolate"], ["Longberry"], ["Rainforest Alliance"], "https://images.unsplash.com/photo-1473923377535-0002805f57e8?auto=format&fit=crop&w=1800&q=88", "Sumatran coffee with tamarind, cedar, and dark chocolate tones", "WHL", 20m, 62, 92),
        new(Guid.Parse("20000000-0000-7000-8000-000000000011"), "las-lajas-black-honey", "Las Lajas Black Honey", "Concord grape, date syrup, and cocoa.", "Nearly all mucilage remains as the cherry dries in thin layers. The Chacons' black-honey method creates immense sweetness with a clean grape-like acidity.", "las-lajas", "Costa Rica", "Alajuela", "Sabanilla", "Las Lajas", 1400, 1550, CoffeeProcess.Honey, null, "Patio under shade, 22 days", "February - April 2026", RoastLevel.MediumLight, RoastIntent.Omni, 0, 12, 88.0m, 4, 4, 5, "Tartaric", "CR-LL-BH-26", "Exclusive Coffees", 4.75m, ["Concord grape", "Date syrup", "Cocoa"], ["Villa Sarchi", "Catuai"], ["Organic"], "https://images.unsplash.com/photo-1485808191679-5f86510681a2?auto=format&fit=crop&w=1800&q=88", "Costa Rican black-honey coffee with grape and date sweetness", "LLH", 28m, 31, 93),
        new(Guid.Parse("20000000-0000-7000-8000-000000000012"), "cruz-loma-sidra", "Cruz Loma Sidra", "Lemongrass, nectarine, and vanilla.", "A restrained yeast inoculation builds aroma rather than ferment character. At altitude, Sidra delivers lemongrass lift, ripe nectarine, and a vanilla-sweet finish.", "cruz-loma", "Ecuador", "Pichincha", "San Jose de Minas", "Finca Cruz Loma", 1950, 2150, CoffeeProcess.YeastInoculated, 48, "Covered raised beds, 17 days", "June - August 2026", RoastLevel.Light, RoastIntent.Filter, 1, 16, 89.4m, 5, 2, 5, "Citric", "EC-CL-SI-26", "Café Imports", 7.40m, ["Lemongrass", "Nectarine", "Vanilla"], ["Sidra"], ["Direct Trade"], "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=1800&q=88", "Ecuadorian Sidra coffee with lemongrass, nectarine, and vanilla", "CLS", 36m, 24, 92),
    ];

    private sealed record ProducerSeed(
        Guid Id,
        string Slug,
        string Name,
        ProducerKind Kind,
        string Country,
        string Region,
        string Story,
        int ElevationMasl,
        decimal? FarmSizeHectares,
        int PartnerSince,
        string ImageUrl,
        string ImageAlt);

    private sealed record CoffeeSeed(
        Guid Id,
        string Slug,
        string Name,
        string ShortDescription,
        string Story,
        string ProducerSlug,
        string Country,
        string Region,
        string Subregion,
        string FarmName,
        int ElevationMinMasl,
        int ElevationMaxMasl,
        CoffeeProcess Process,
        int? FermentationHours,
        string DryingMethod,
        string HarvestLabel,
        RoastLevel RoastLevel,
        RoastIntent RoastIntent,
        int RoastDayOffset,
        int RestDays,
        decimal CupScore,
        int Acidity,
        int Body,
        int Sweetness,
        string AcidityType,
        string LotCode,
        string ImportPartner,
        decimal? FobPriceUsdPerLb,
        string[] TastingNotes,
        string[] Varieties,
        string[] Certifications,
        string HeroImageUrl,
        string HeroImageAlt,
        string SkuPrefix,
        decimal PriceUsd,
        int Stock,
        int FilterTemperatureC);
}
