using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Models;

namespace SnackFlowMES.Data;

/// <summary>
/// SnackFlow MES — EF Core DbContext.
/// Inherits IdentityDbContext so all ASP.NET Core Identity tables are included.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Module 2: Items / Materials Master ──────────────────
    public DbSet<Item> Items => Set<Item>();

    // ── Module 3: BOM ───────────────────────────────────────
    public DbSet<BillOfMaterials> BillsOfMaterials => Set<BillOfMaterials>();
    public DbSet<BomLine>         BomLines          => Set<BomLine>();

    // ── Module 4: Inventory ─────────────────────────────────
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryLedger>  InventoryLedgers  => Set<InventoryLedger>();

    // ── Module 5: Production Planning ───────────────────────
    public DbSet<ProductionPlan> ProductionPlans => Set<ProductionPlan>();
    public DbSet<PlanLine>       PlanLines       => Set<PlanLine>();

    // ── Module 6: MRP ───────────────────────────────────────
    public DbSet<MrpRun>         MrpRuns         => Set<MrpRun>();
    public DbSet<MrpRequirement> MrpRequirements => Set<MrpRequirement>();

    // ── Module 7: Work Orders ───────────────────────────────
    public DbSet<WorkOrder>         WorkOrders         => Set<WorkOrder>();
    public DbSet<WorkOrderMaterial> WorkOrderMaterials => Set<WorkOrderMaterial>();

    // ── Module 8: Production Reporting ──────────────────────
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();

    // ── Module 9: Quality Control ───────────────────────────
    public DbSet<QcResult> QcResults => Set<QcResult>();

    // ── Module 10: Cost Summary ─────────────────────────────
    public DbSet<WorkOrderCost> WorkOrderCosts => Set<WorkOrderCost>();

    // ── Security: Audit Log ─────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity tables first

        // ── Rename ApplicationUser table ─────────────────────
        builder.Entity<ApplicationUser>().ToTable("Users");

        // ── Item: unique ItemCode ────────────────────────────
        builder.Entity<Item>()
            .HasIndex(i => i.ItemCode)
            .IsUnique()
            .HasDatabaseName("IX_Items_ItemCode");

        // ── BOM: one active BOM per item per version ─────────
        builder.Entity<BillOfMaterials>()
            .HasIndex(b => new { b.ItemId, b.Version })
            .IsUnique()
            .HasDatabaseName("IX_Bom_ItemId_Version");

        // ── BOM → Item: one-to-one (each FG has one BOM) ─────
        builder.Entity<BillOfMaterials>()
            .HasOne(b => b.Item)
            .WithOne(i => i.Bom)
            .HasForeignKey<BillOfMaterials>(b => b.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── InventoryBalance: one row per item ───────────────
        builder.Entity<InventoryBalance>()
            .HasIndex(s => s.ItemId)
            .IsUnique()
            .HasDatabaseName("IX_InventoryBalance_ItemId");

        builder.Entity<InventoryBalance>()
            .HasOne(b => b.Item)
            .WithOne(i => i.InventoryBalance)
            .HasForeignKey<InventoryBalance>(b => b.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── WorkOrder: unique WO number ──────────────────────
        builder.Entity<WorkOrder>()
            .HasIndex(w => w.WoNumber)
            .IsUnique()
            .HasDatabaseName("IX_WorkOrders_WoNumber");

        // ── WorkOrderMaterial: one row per WO + Item ─────────
        builder.Entity<WorkOrderMaterial>()
            .HasIndex(m => new { m.WorkOrderId, m.ItemId })
            .IsUnique()
            .HasDatabaseName("IX_WoMaterial_WoId_ItemId");

        // ── WorkOrderCost: one cost row per WO ───────────────
        builder.Entity<WorkOrderCost>()
            .HasOne(c => c.WorkOrder)
            .WithOne(w => w.Cost)
            .HasForeignKey<WorkOrderCost>(c => c.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── InventoryLedger → WorkOrder: restrict delete ──────
        builder.Entity<InventoryLedger>()
            .HasOne(l => l.WorkOrder)
            .WithMany(w => w.InventoryLedgers)
            .HasForeignKey(l => l.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── WorkOrder → ApplicationUser: set null on user delete
        builder.Entity<WorkOrder>()
            .HasOne(w => w.CreatedBy)
            .WithMany(u => u.CreatedWorkOrders)
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── ProductionPlan → ApplicationUser: restrict ────────
        builder.Entity<ProductionPlan>()
            .HasOne(p => p.CreatedBy)
            .WithMany(u => u.CreatedPlans)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── ProductionLog → ApplicationUser: restrict ─────────
        builder.Entity<ProductionLog>()
            .HasOne(l => l.RecordedBy)
            .WithMany(u => u.ProductionLogs)
            .HasForeignKey(l => l.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── QcResult → ApplicationUser: restrict ──────────────
        builder.Entity<QcResult>()
            .HasOne(q => q.InspectedBy)
            .WithMany(u => u.QcResults)
            .HasForeignKey(q => q.InspectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── AuditLog → ApplicationUser: set null on user delete
        builder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
