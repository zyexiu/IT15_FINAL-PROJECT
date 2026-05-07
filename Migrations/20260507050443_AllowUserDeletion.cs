using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnackFlowMES.Migrations
{
    /// <inheritdoc />
    public partial class AllowUserDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlans_Users_CreatedByUserId",
                table: "ProductionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "WorkOrders",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldMaxLength: 450)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "ProductionPlans",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldMaxLength: 450)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlans_Users_CreatedByUserId",
                table: "ProductionPlans",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlans_Users_CreatedByUserId",
                table: "ProductionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders");

            migrationBuilder.UpdateData(
                table: "WorkOrders",
                keyColumn: "CreatedByUserId",
                keyValue: null,
                column: "CreatedByUserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "WorkOrders",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldMaxLength: 450,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ProductionPlans",
                keyColumn: "CreatedByUserId",
                keyValue: null,
                column: "CreatedByUserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "ProductionPlans",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)",
                oldMaxLength: 450,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlans_Users_CreatedByUserId",
                table: "ProductionPlans",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
