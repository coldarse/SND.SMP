using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SND.SMP.Migrations
{
    /// <inheritdoc />
    public partial class Added_Status_To_TrackingNoForUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "TrackingNoForUpdates",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "TrackingNoForUpdates");
        }
    }
}
