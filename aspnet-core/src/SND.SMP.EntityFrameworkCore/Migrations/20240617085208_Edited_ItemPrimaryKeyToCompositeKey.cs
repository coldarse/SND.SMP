using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Edited_ItemPrimaryKeyToCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMins_Dispatches_DispatchID",
                table: "ItemMins");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemMins",
                table: "ItemMins");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchID",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DispatchID",
                table: "ItemMins",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                columns: new[] { "Id", "DispatchID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemMins",
                table: "ItemMins",
                columns: new[] { "Id", "DispatchID" });

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMins_Dispatches_DispatchID",
                table: "ItemMins",
                column: "DispatchID",
                principalTable: "Dispatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items",
                column: "DispatchID",
                principalTable: "Dispatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMins_Dispatches_DispatchID",
                table: "ItemMins");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemMins",
                table: "ItemMins");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchID",
                table: "Items",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchID",
                table: "ItemMins",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemMins",
                table: "ItemMins",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMins_Dispatches_DispatchID",
                table: "ItemMins",
                column: "DispatchID",
                principalTable: "Dispatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Dispatches_DispatchID",
                table: "Items",
                column: "DispatchID",
                principalTable: "Dispatches",
                principalColumn: "Id");
        }
    }
}
