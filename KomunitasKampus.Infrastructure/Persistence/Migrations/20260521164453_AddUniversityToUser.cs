using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomunitasKampus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversityToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "university",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "university",
                table: "users");
        }
    }
}
