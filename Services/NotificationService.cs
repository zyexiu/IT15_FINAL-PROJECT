using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;

namespace SnackFlowMES.Services;

/// <summary>
/// Service for creating and managing notifications across the system.
/// Enables upward reporting flow (Planner→Admin, Operator→Manager, QC→Admin/Manager).
/// </summary>
public class NotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationService> _log;

    public NotificationService(ApplicationDbContext db, ILogger<NotificationService> log)
    {
        _db = db;
        _log = log;
    }

    /// <summary>
    /// Create a notification for a specific user
    /// </summary>
    public async Task CreateUserNotificationAsync(
        string recipientUserId,
        string type,
        string title,
        string message,
        string priority = "Medium",
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        string? actionUrl = null,
        string? createdByUserId = null,
        string tenantId = "")
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            Priority = priority,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            CreatedByUserId = createdByUserId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        _log.LogInformation("Notification created for user {UserId}: {Title}", recipientUserId, title);
    }

    /// <summary>
    /// Create a notification for all users in a specific role
    /// </summary>
    public async Task CreateRoleNotificationAsync(
        string recipientRole,
        string type,
        string title,
        string message,
        string priority = "Medium",
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        string? actionUrl = null,
        string? createdByUserId = null,
        string tenantId = "")
    {
        var notification = new Notification
        {
            RecipientRole = recipientRole,
            Type = type,
            Title = title,
            Message = message,
            Priority = priority,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            CreatedByUserId = createdByUserId,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        _log.LogInformation("Notification created for role {Role}: Type={Type}, Title={Title}, TenantId={TenantId}, NotifId={NotifId}", 
            recipientRole, type, title, tenantId, notification.NotificationId);
    }

    /// <summary>
    /// Get unread notifications for a user
    /// </summary>
    public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId, int limit = 10, string? tenantId = null)
    {
        var query = _db.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // FIXED: Check for NULL TenantId in database, not just empty string
            query = query.Where(n => n.TenantId == null || n.TenantId == "" || n.TenantId == tenantId);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get unread notifications for a role
    /// </summary>
    public async Task<List<Notification>> GetUnreadRoleNotificationsAsync(string role, int limit = 10, string? tenantId = null)
    {
        var query = _db.Notifications
            .Where(n => n.RecipientRole == role && !n.IsRead);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // FIXED: Check for NULL TenantId in database, not just empty string
            query = query.Where(n => n.TenantId == null || n.TenantId == "" || n.TenantId == tenantId);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get unread notification count for a user
    /// </summary>
    public async Task<int> GetUnreadCountAsync(string userId, string? tenantId = null)
    {
        var query = _db.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // FIXED: Check for NULL TenantId in database, not just empty string
            query = query.Where(n => n.TenantId == null || n.TenantId == "" || n.TenantId == tenantId);
        }

        return await query.CountAsync();
    }

    /// <summary>
    /// Get unread notification count for a role
    /// </summary>
    public async Task<int> GetUnreadRoleCountAsync(string role, string? tenantId = null)
    {
        var query = _db.Notifications
            .Where(n => n.RecipientRole == role && !n.IsRead);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // FIXED: Check for NULL TenantId in database, not just empty string
            query = query.Where(n => n.TenantId == null || n.TenantId == "" || n.TenantId == tenantId);
        }

        return await query.CountAsync();
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    public async Task MarkAllAsReadAsync(string userId, string? role = null, string? tenantId = null)
    {
        var query = _db.Notifications
            .Where(n => !n.IsRead && (n.RecipientUserId == userId || (!string.IsNullOrWhiteSpace(role) && n.RecipientRole == role)));

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            // FIXED: Check for NULL TenantId in database, not just empty string
            query = query.Where(n => n.TenantId == null || n.TenantId == "" || n.TenantId == tenantId);
        }

        var notifications = await query.ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Create material request notification for Admin
    /// </summary>
    public async Task CreateMaterialRequestNotificationAsync(MaterialRequest request, string tenantId)
    {
        var item = await _db.Items.FindAsync(request.ItemId);
        _log.LogInformation("Creating MaterialRequest notification: RequestId={RequestId}, ItemId={ItemId}, TenantId={TenantId}, RequestedByUserId={UserId}",
            request.RequestId, request.ItemId, tenantId, request.RequestedByUserId);
        
        await CreateRoleNotificationAsync(
            recipientRole: "Admin",
            type: "MaterialRequest",
            title: "New Material Request",
            message: $"Material request for {item?.ItemName}: {request.RequestedQty:N2} {request.UnitOfMeasure}. Reason: {request.Reason}",
            priority: request.Priority,
            relatedEntityType: "MaterialRequest",
            relatedEntityId: request.RequestId,
            actionUrl: $"/MaterialRequest?status=Pending",
            createdByUserId: request.RequestedByUserId,
            tenantId: tenantId
        );
        
        _log.LogInformation("MaterialRequest notification created successfully");
    }

    /// <summary>
    /// Create QC failed notification for Admin and Manager
    /// </summary>
    public async Task CreateQCFailedNotificationAsync(QcResult qcResult, WorkOrder workOrder, string tenantId)
    {
        var item = await _db.Items.FindAsync(workOrder.ItemId);
        var message = $"QC Failed for {workOrder.WoNumber} - {item?.ItemName}. Result: {qcResult.Result}, Disposition: {qcResult.Disposition}";

        // Notify Admin
        await CreateRoleNotificationAsync(
            recipientRole: "Admin",
            type: "QCFailed",
            title: "QC Inspection Failed",
            message: message,
            priority: "Critical",
            relatedEntityType: "WorkOrder",
            relatedEntityId: workOrder.WorkOrderId,
            actionUrl: $"/WorkOrder/Details/{workOrder.WorkOrderId}",
            createdByUserId: qcResult.InspectedByUserId,
            tenantId: tenantId
        );

        // Notify Manager
        await CreateRoleNotificationAsync(
            recipientRole: "Manager",
            type: "QCFailed",
            title: "QC Inspection Failed",
            message: message,
            priority: "High",
            relatedEntityType: "WorkOrder",
            relatedEntityId: workOrder.WorkOrderId,
            createdByUserId: qcResult.InspectedByUserId,
            tenantId: tenantId
        );
    }

    /// <summary>
    /// Create QC passed notification for Manager
    /// </summary>
    public async Task CreateQCPassedNotificationAsync(QcResult qcResult, WorkOrder workOrder, string tenantId)
    {
        var item = await _db.Items.FindAsync(workOrder.ItemId);
        await CreateRoleNotificationAsync(
            recipientRole: "Manager",
            type: "QCPassed",
            title: "QC Inspection Passed",
            message: $"QC Passed for {workOrder.WoNumber} - {item?.ItemName}. Disposition: {qcResult.Disposition}",
            priority: "Low",
            relatedEntityType: "WorkOrder",
            relatedEntityId: workOrder.WorkOrderId,
            actionUrl: $"/WorkOrder/Details/{workOrder.WorkOrderId}",
            createdByUserId: qcResult.InspectedByUserId,
            tenantId: tenantId
        );
    }

    /// <summary>
    /// Create downtime report notification for Admin and Manager
    /// </summary>
    public async Task CreateDowntimeReportNotificationAsync(
        int downtimeId,
        int workOrderId,
        string reason,
        int durationMinutes,
        string tenantId)
    {
        var workOrder = await _db.WorkOrders
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        var message = $"Downtime reported for {workOrder?.WoNumber} - {workOrder?.Item?.ItemName}. Reason: {reason}, Duration: {durationMinutes} minutes";

        // Notify Admin
        await CreateRoleNotificationAsync(
            recipientRole: "Admin",
            type: "DowntimeReport",
            title: "Production Downtime Reported",
            message: message,
            priority: durationMinutes > 60 ? "High" : "Medium",
            relatedEntityType: "DowntimeReport",
            relatedEntityId: downtimeId,
            actionUrl: $"/Downtime/Details/{downtimeId}",
            tenantId: tenantId
        );

        // Notify Manager
        await CreateRoleNotificationAsync(
            recipientRole: "Manager",
            type: "DowntimeReport",
            title: "Production Downtime Reported",
            message: message,
            priority: durationMinutes > 60 ? "High" : "Medium",
            relatedEntityType: "DowntimeReport",
            relatedEntityId: downtimeId,
            actionUrl: $"/Downtime/Details/{downtimeId}",
            tenantId: tenantId
        );
    }

    /// <summary>
    /// Generic method to create a notification (used by controllers)
    /// </summary>
    public async Task CreateNotificationAsync(
        string? recipientUserId = null,
        string? recipientRole = null,
        string type = "System",
        string title = "",
        string message = "",
        string priority = "Medium",
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        string? actionUrl = null,
        string? createdByUserId = null,
        string organizationId = "")
    {
        if (string.IsNullOrEmpty(recipientUserId) && string.IsNullOrEmpty(recipientRole))
        {
            throw new ArgumentException("Either recipientUserId or recipientRole must be provided");
        }

        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            RecipientRole = recipientRole,
            Type = type,
            Title = title,
            Message = message,
            Priority = priority,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            CreatedByUserId = createdByUserId,
            TenantId = organizationId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        _log.LogInformation("Notification created: {Title}", title);
    }
}
