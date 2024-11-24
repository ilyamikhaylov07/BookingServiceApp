using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.API.Migrations
{
    /// <inheritdoc />
    public partial class Adduserid_in_schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Schedules");
        }
    }
}
