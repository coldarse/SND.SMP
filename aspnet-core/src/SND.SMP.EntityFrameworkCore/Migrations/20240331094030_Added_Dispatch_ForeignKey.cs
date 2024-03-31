using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_Dispatch_ForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_CustomerCode",
                table: "Dispatches",
                column: "CustomerCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_Customers_CustomerCode",
                table: "Dispatches",
                column: "CustomerCode",
                principalTable: "Customers",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_Customers_CustomerCode",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_CustomerCode",
                table: "Dispatches");
        }
    }
}
