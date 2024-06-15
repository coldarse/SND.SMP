using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_ClientKeyAndSecretAndToken_ToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "APIAccessToken",
                table: "Customers",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientKey",
                table: "Customers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "Customers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APIAccessToken",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ClientKey",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Customers");
        }
    }
}
