using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenToolSharing.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddToolIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tools",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tools");
        }
    }
}
