using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnackFlowMES.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Items");
        }
    }
}
