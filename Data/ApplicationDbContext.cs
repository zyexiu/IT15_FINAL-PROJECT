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
    // MRP tables removed — feature not implemented

    // ── Module 7: Work Orders ───────────────────────────────
    public DbSet<WorkOrder>         WorkOrders         => Set<WorkOrder>();
    public DbSet<WorkOrderMaterial> WorkOrderMaterials => Set<WorkOrderMaterial>();

    // ── Module 8: Production Reporting ──────────────────────
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();

    // ── Module 9: Quality Control ───────────────────────────
    public DbSet<QcResult> QcResults => Set<QcResult>();

    // ── Module 10: Cost Summary ─────────────────────────────
    // WorkOrderCosts table removed — feature not implemented

    // ── Notification System ─────────────────────────────────
    public DbSet<Notification> Notifications => Set<Notification>();

    // ── Material Request System ─────────────────────────────
    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();

    // ── Downtime Reporting ──────────────────────────────────
    public DbSet<DowntimeReport> DowntimeReports => Set<DowntimeReport>();

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

        // ── BOM → Item: one item can have multiple BOM versions ─
        builder.Entity<BillOfMaterials>()
            .HasOne(b => b.Item)
            .WithMany(i => i.Boms)
            .HasForeignKey(b => b.ItemId)
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

        // ── WorkOrderCost: removed — feature not implemented

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
            .OnDelete(DeleteBehavior.SetNull);

        // ── ProductionPlan → ApplicationUser: set null on user delete
        builder.Entity<ProductionPlan>()
            .HasOne(p => p.CreatedBy)
            .WithMany(u => u.CreatedPlans)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

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

        // ── Notification → ApplicationUser (Recipient): restrict ─
        builder.Entity<Notification>()
            .HasOne(n => n.RecipientUser)
            .WithMany(u => u.ReceivedNotifications)
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Notification → ApplicationUser (Creator): set null ────
        builder.Entity<Notification>()
            .HasOne(n => n.CreatedBy)
            .WithMany(u => u.CreatedNotifications)
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── MaterialRequest → ApplicationUser (Requester): restrict
        builder.Entity<MaterialRequest>()
            .HasOne(m => m.RequestedBy)
            .WithMany(u => u.MaterialRequests)
            .HasForeignKey(m => m.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── MaterialRequest → ApplicationUser (Approver): set null
        builder.Entity<MaterialRequest>()
            .HasOne(m => m.ApprovedBy)
            .WithMany()
            .HasForeignKey(m => m.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── DowntimeReport → ApplicationUser (Reporter): restrict
        builder.Entity<DowntimeReport>()
            .HasOne(d => d.ReportedBy)
            .WithMany(u => u.DowntimeReports)
            .HasForeignKey(d => d.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── DowntimeReport → ApplicationUser (Resolver): set null
        builder.Entity<DowntimeReport>()
            .HasOne(d => d.ResolvedBy)
            .WithMany()
            .HasForeignKey(d => d.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── DowntimeReport → WorkOrder: restrict
        builder.Entity<DowntimeReport>()
            .HasOne(d => d.WorkOrder)
            .WithMany(w => w.DowntimeReports)
            .HasForeignKey(d => d.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
