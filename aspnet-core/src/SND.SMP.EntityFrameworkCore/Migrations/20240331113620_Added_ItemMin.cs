using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_ItemMin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemMins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExtID = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispatchID = table.Column<int>(type: "int", nullable: false),
                    BagID = table.Column<int>(type: "int", nullable: false),
                    DispatchDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecpName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemDesc = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelNo = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveredInDays = table.Column<int>(type: "int", nullable: false),
                    IsDelivered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemMins_Bags_BagID",
                        column: x => x.BagID,
                        principalTable: "Bags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemMins_Dispatches_DispatchID",
                        column: x => x.DispatchID,
                        principalTable: "Dispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMins_BagID",
                table: "ItemMins",
                column: "BagID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMins_DispatchID",
                table: "ItemMins",
                column: "DispatchID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemMins");
        }
    }
}
