using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Removed_ForeignKeyFromItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Bags_BagID",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_BagID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DispatchID",
                table: "Items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_BagID",
                table: "Items",
                column: "BagID");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DispatchID",
                table: "Items",
                column: "DispatchID");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Bags_BagID",
                table: "Items",
                column: "BagID",
                principalTable: "Bags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items",
                column: "DispatchID",
                principalTable: "Dispatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
