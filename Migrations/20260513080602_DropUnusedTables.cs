using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnackFlowMES.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MrpRequirements");

            migrationBuilder.DropTable(
                name: "WorkOrderCosts");

            migrationBuilder.DropTable(
                name: "MrpRuns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MrpRuns",
                columns: table => new
                {
                    MrpRunId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RunByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrpRuns", x => x.MrpRunId);
                    table.ForeignKey(
                        name: "FK_MrpRuns_ProductionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ProductionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkOrderCosts",
                columns: table => new
                {
                    CostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ComputedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostPerUnit = table.Column<decimal>(type: "decimal(14,4)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    LaborRatePerHour = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MachineCost = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    MachineRatePerHour = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OtherCost = table.Column<decimal>(type: "decimal(14,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCosts", x => x.CostId);
                    table.ForeignKey(
                        name: "FK_WorkOrderCosts_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MrpRequirements",
                columns: table => new
                {
                    MrpReqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    MrpRunId = table.Column<int>(type: "int", nullable: false),
                    GrossRequirement = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    IsShortage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NetRequirement = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    StockOnHand = table.Column<decimal>(type: "decimal(12,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrpRequirements", x => x.MrpReqId);
                    table.ForeignKey(
                        name: "FK_MrpRequirements_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MrpRequirements_MrpRuns_MrpRunId",
                        column: x => x.MrpRunId,
                        principalTable: "MrpRuns",
                        principalColumn: "MrpRunId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MrpRequirements_ItemId",
                table: "MrpRequirements",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpRequirements_MrpRunId",
                table: "MrpRequirements",
                column: "MrpRunId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpRuns_PlanId",
                table: "MrpRuns",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCosts_WorkOrderId",
                table: "WorkOrderCosts",
                column: "WorkOrderId",
                unique: true);
        }
    }
}
