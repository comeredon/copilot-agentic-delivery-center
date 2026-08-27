using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoffeeStoreCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "producers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Country = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Story = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    ElevationMasl = table.Column<int>(type: "integer", nullable: false),
                    FarmSizeHectares = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    PartnerSince = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ImageAlt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "coffees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Story = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Country = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Subregion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FarmName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ElevationMinMasl = table.Column<int>(type: "integer", nullable: false),
                    ElevationMaxMasl = table.Column<int>(type: "integer", nullable: false),
                    Process = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FermentationHours = table.Column<int>(type: "integer", nullable: true),
                    DryingMethod = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HarvestLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoastLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoastIntent = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoastedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RestDays = table.Column<int>(type: "integer", nullable: false),
                    CupScore = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    Acidity = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<int>(type: "integer", nullable: false),
                    Sweetness = table.Column<int>(type: "integer", nullable: false),
                    AcidityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LotCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ImportPartner = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FobPriceUsdPerLb = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coffees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coffees_producers_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "producers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "brew_recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DoseGrams = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false),
                    YieldGrams = table.Column<decimal>(type: "numeric(7,1)", precision: 7, scale: 1, nullable: false),
                    WaterTemperatureC = table.Column<int>(type: "integer", nullable: false),
                    BrewSeconds = table.Column<int>(type: "integer", nullable: false),
                    GrindDescriptor = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brew_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_brew_recommendations_coffees_CoffeeId",
                        column: x => x.CoffeeId,
                        principalTable: "coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coffee_descriptors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coffee_descriptors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coffee_descriptors_coffees_CoffeeId",
                        column: x => x.CoffeeId,
                        principalTable: "coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coffee_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    AltText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsHero = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coffee_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coffee_images_coffees_CoffeeId",
                        column: x => x.CoffeeId,
                        principalTable: "coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coffee_variants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BagSizeGrams = table.Column<int>(type: "integer", nullable: false),
                    Grind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PriceUsd = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    StockOnHand = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coffee_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coffee_variants_coffees_CoffeeId",
                        column: x => x.CoffeeId,
                        principalTable: "coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_brew_recommendations_CoffeeId_Method",
                table: "brew_recommendations",
                columns: new[] { "CoffeeId", "Method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coffee_descriptors_CoffeeId_Kind_Value",
                table: "coffee_descriptors",
                columns: new[] { "CoffeeId", "Kind", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coffee_images_CoffeeId",
                table: "coffee_images",
                column: "CoffeeId");

            migrationBuilder.CreateIndex(
                name: "IX_coffee_variants_CoffeeId",
                table: "coffee_variants",
                column: "CoffeeId");

            migrationBuilder.CreateIndex(
                name: "IX_coffee_variants_Sku",
                table: "coffee_variants",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coffees_ProducerId",
                table: "coffees",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_coffees_Slug",
                table: "coffees",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coffees_Status_Country",
                table: "coffees",
                columns: new[] { "Status", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_producers_Slug",
                table: "producers",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brew_recommendations");

            migrationBuilder.DropTable(
                name: "coffee_descriptors");

            migrationBuilder.DropTable(
                name: "coffee_images");

            migrationBuilder.DropTable(
                name: "coffee_variants");

            migrationBuilder.DropTable(
                name: "coffees");

            migrationBuilder.DropTable(
                name: "producers");
        }
    }
}
