namespace SnackFlowMES.ViewModels;

public class DashboardViewModel
{
    // ── KPIs ─────────────────────────────────────────────────
    public int TotalWorkOrders  { get; set; }
    public int ActiveProduction { get; set; }
    public int CompletedOrders  { get; set; }
    public int TotalItems       { get; set; }
    public int LowStockCount    { get; set; }

    // ── Status breakdown ─────────────────────────────────────
    public int DraftCount      { get; set; }
    public int ReleasedCount   { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount  { get; set; }
    public int CancelledCount  { get; set; }

    // ── Recent work orders ───────────────────────────────────
    public List<WorkOrderSummary> RecentWorkOrders { get; set; } = [];

    // ── User context ─────────────────────────────────────────
    public string CurrentUserName { get; set; } = string.Empty;
    public string CurrentUserRole { get; set; } = string.Empty;
}

public class WorkOrderSummary
{
    public int      WorkOrderId    { get; set; }
    public string   WoNumber       { get; set; } = string.Empty;
    public string   ItemName       { get; set; } = string.Empty;
    public decimal  PlannedQty     { get; set; }
    public decimal  ActualQty      { get; set; }
    public string   UnitOfMeasure  { get; set; } = string.Empty;
    public string   Status         { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string?  ProductionLine { get; set; }
}
