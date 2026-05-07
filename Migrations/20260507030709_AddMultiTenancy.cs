using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnackFlowMES.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "WorkOrders",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "QcResults",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ProductionPlans",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ProductionLogs",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "MrpRuns",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Items",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "InventoryLedgers",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "InventoryBalances",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "BillsOfMaterials",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AuditLogs",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QcResults");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductionPlans");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductionLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MrpRuns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryLedgers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryBalances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BillsOfMaterials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");
        }
    }
}
