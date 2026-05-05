namespace SnackFlowMES.ViewModels;

public class ReportViewModel
{
    // Work Order Statistics
    public int TotalWorkOrders { get; set; }
    public int DraftWorkOrders { get; set; }
    public int ReleasedWorkOrders { get; set; }
    public int InProgressWorkOrders { get; set; }
    public int CompletedWorkOrders { get; set; }
    public int CancelledWorkOrders { get; set; }
    public decimal CompletionRate { get; set; }

    // Production Statistics
    public decimal TotalPlannedQty { get; set; }
    public decimal TotalActualQty { get; set; }
    public decimal YieldRate { get; set; }

    // Inventory Statistics
    public int TotalItems { get; set; }
    public int RawMaterialsCount { get; set; }
    public int PackagingCount { get; set; }
    public int FinishedGoodsCount { get; set; }
    public int LowStockItems { get; set; }

    // Production Plans
    public int TotalProductionPlans { get; set; }
    public int ActiveProductionPlans { get; set; }

    // Recent Activity
    public List<WorkOrderSummary> RecentWorkOrders { get; set; } = [];
}
