using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_RateWeightBreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateWeightBreaks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RateId = table.Column<int>(type: "int", nullable: false),
                    PostalOrgId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeightMin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightMax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                    ItemRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsExceedRule = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PaymentMode = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateWeightBreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RateWeightBreaks_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RateWeightBreaks_PostalOrgs_PostalOrgId",
                        column: x => x.PostalOrgId,
                        principalTable: "PostalOrgs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RateWeightBreaks_Rates_RateId",
                        column: x => x.RateId,
                        principalTable: "Rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RateWeightBreaks_CurrencyId",
                table: "RateWeightBreaks",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RateWeightBreaks_PostalOrgId",
                table: "RateWeightBreaks",
                column: "PostalOrgId");

            migrationBuilder.CreateIndex(
                name: "IX_RateWeightBreaks_RateId",
                table: "RateWeightBreaks",
                column: "RateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateWeightBreaks");
        }
    }
}
