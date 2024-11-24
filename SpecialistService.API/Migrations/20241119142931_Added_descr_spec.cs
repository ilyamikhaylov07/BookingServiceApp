using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpecialistService.API.Migrations
{
    /// <inheritdoc />
    public partial class Added_descr_spec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Specialists",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Specialists");
        }
    }
}
