using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_Wallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Customer = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EWalletType = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => new { x.Customer, x.EWalletType, x.Currency });
                    table.ForeignKey(
                        name: "FK_Wallets_Currencies_Currency",
                        column: x => x.Currency,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Customers_Customer",
                        column: x => x.Customer,
                        principalTable: "Customers",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_EWalletTypes_EWalletType",
                        column: x => x.EWalletType,
                        principalTable: "EWalletTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Currency",
                table: "Wallets",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Customer",
                table: "Wallets",
                column: "Customer");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_EWalletType",
                table: "Wallets",
                column: "EWalletType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
