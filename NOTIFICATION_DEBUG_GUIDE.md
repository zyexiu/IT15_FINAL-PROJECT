# Notification System Debug Guide

## Current Status

**Problem**: Notifications show in "System Alerts" panel but NOT in notification bell dropdown.

**What We Know**:
1. ✅ Notifications ARE being created (visible in System Alerts)
2. ✅ Notifications ARE in the database
3. ❌ Notifications NOT showing in notification bell dropdown
4. ❌ Notifications NOT showing in Notification page

## Step-by-Step Debugging

### STEP 1: Check Database

Run the SQL queries in `DEBUG_NOTIFICATIONS.sql` to verify:

1. **Check notifications exist**:
   ```sql
   SELECT * FROM Notifications 
   WHERE Type = 'MaterialRequest' 
   AND IsRead = 0;
   ```
   
   **Expected**: Should see notifications with:
   - `RecipientRole` = 'Admin'
   - `TenantId` = (some user ID)
   - `IsRead` = 0

2. **Check Admin user**:
   ```sql
   SELECT Id, FullName, TenantId 
   FROM AspNetUsers 
   WHERE Id IN (
       SELECT UserId FROM AspNetUserRoles 
       WHERE RoleId IN (
           SELECT Id FROM AspNetRoles WHERE Name = 'Admin'
       )
   );
   ```
   
   **Expected**: Should see:
   - `Id` = (Admin's user ID, e.g., "abc123")
   - `TenantId` = NULL or empty

3. **Check if TenantIds match**:
   ```sql
   -- Get Admin's user ID
   SET @adminId = (SELECT Id FROM AspNetUsers WHERE ...);
   
   -- Check if notifications have matching TenantId
   SELECT COUNT(*) FROM Notifications 
   WHERE RecipientRole = 'Admin' 
   AND IsRead = 0
   AND TenantId = @adminId;
   ```
   
   **Expected**: Should return count > 0

### STEP 2: Check Application Logs

After building with debug logging, check the application logs when Admin logs in:

1. **Start the application**:
   ```bash
   dotnet run
   ```

2. **Login as Admin**

3. **Check logs** for these messages:
   ```
   GetUnreadCount - User: {UserId}, Role: {Role}, TenantId: {TenantId}
   GetUnreadRoleCountAsync - Role: {Role}, TenantId: {TenantId}, Count: {Count}
   GetUnreadCount - UserCount: {UserCount}, RoleCount: {RoleCount}, Total: {Total}
   ```

4. **Verify**:
   - Does `TenantId` in log match Admin's user ID?
   - Does `RoleCount` show > 0?
   - Does `Total` show > 0?

### STEP 3: Identify the Issue

Based on logs and database, identify which scenario applies:

#### Scenario A: TenantId Mismatch
**Symptoms**:
- Notifications in DB have TenantId = "admin-123"
- Log shows TenantId = "different-id"
- RoleCount = 0

**Fix**: The `ResolveTenantIdAsync` is returning wrong ID

#### Scenario B: Query Not Finding Notifications
**Symptoms**:
- Notifications in DB have TenantId = "admin-123"
- Log shows TenantId = "admin-123" (matches!)
- RoleCount = 0 (but should be > 0)

**Fix**: The query logic in `GetUnreadRoleCountAsync` is wrong

#### Scenario C: Notifications Have Wrong TenantId
**Symptoms**:
- Notifications in DB have TenantId = NULL or empty
- Log shows TenantId = "admin-123"
- RoleCount = 0

**Fix**: Notification creation is not setting TenantId correctly

### STEP 4: Apply the Fix

#### Fix for Scenario A (TenantId Mismatch):

**File**: `Controllers/NotificationController.cs`

```csharp
private async Task<string> ResolveTenantIdAsync(ApplicationUser user, IList<string> roles)
{
    // CRITICAL: Check Admin role FIRST
    if (roles.Contains("Admin"))
    {
        return user.Id;  // Admin's tenant is their own ID
    }

    // For other roles, use their TenantId
    if (!string.IsNullOrWhiteSpace(user.TenantId))
    {
        return user.TenantId;
    }

    // Fallback
    return user.Id;
}
```

#### Fix for Scenario B (Query Logic):

**File**: `Services/NotificationService.cs`

```csharp
public async Task<int> GetUnreadRoleCountAsync(string role, string? tenantId = null)
{
    var query = _db.Notifications
        .Where(n => n.RecipientRole == role && !n.IsRead);

    if (!string.IsNullOrWhiteSpace(tenantId))
    {
        // CRITICAL: This should match notifications where:
        // - TenantId is empty/null (system-wide), OR
        // - TenantId matches the provided tenantId
        query = query.Where(n => string.IsNullOrEmpty(n.TenantId) || n.TenantId == tenantId);
    }

    return await query.CountAsync();
}
```

#### Fix for Scenario C (Notification Creation):

**File**: `Controllers/MaterialRequestController.cs`

```csharp
// When creating material request
model.TenantId = tenantId;  // Make sure this is set

_db.MaterialRequests.Add(model);
await _db.SaveChangesAsync();

// Create notification - MUST pass correct tenantId
await _notifications.CreateMaterialRequestNotificationAsync(model, model.TenantId);
```

### STEP 5: Test the Fix

1. **Clear existing notifications** (optional):
   ```sql
   DELETE FROM Notifications WHERE Type = 'MaterialRequest';
   ```

2. **Restart application**:
   ```bash
   dotnet run
   ```

3. **Test flow**:
   - Login as Planner
   - Create new material request
   - Logout
   - Login as Admin
   - **CHECK**: Notification bell should show badge
   - Click notification bell
   - **CHECK**: Should see "New Material Request"
   - Click notification
   - **CHECK**: Should navigate to Material Request page

4. **Verify logs**:
   ```
   GetUnreadCount - User: admin-123, Role: Admin, TenantId: admin-123
   GetUnreadRoleCountAsync - Role: Admin, TenantId: admin-123, Count: 1
   GetUnreadCount - UserCount: 0, RoleCount: 1, Total: 1
   ```

### STEP 6: Remove Debug Logging

Once fixed, remove debug logging:

1. **File**: `Controllers/NotificationController.cs`
   - Remove `_logger.LogInformation` lines

2. **File**: `Services/NotificationService.cs`
   - Remove `_log.LogInformation` lines

3. **Rebuild**:
   ```bash
   dotnet build
   ```

## Common Mistakes

### Mistake 1: Checking TenantId Before Role
```csharp
// WRONG
if (!string.IsNullOrWhiteSpace(user.TenantId))
    return user.TenantId;  // Admin has NULL TenantId, this fails!
if (roles.Contains("Admin"))
    return user.Id;

// RIGHT
if (roles.Contains("Admin"))
    return user.Id;  // Check Admin FIRST
if (!string.IsNullOrWhiteSpace(user.TenantId))
    return user.TenantId;
```

### Mistake 2: Wrong Query Logic
```csharp
// WRONG
query = query.Where(n => n.TenantId == tenantId);  // Excludes NULL TenantId

// RIGHT
query = query.Where(n => string.IsNullOrEmpty(n.TenantId) || n.TenantId == tenantId);
```

### Mistake 3: Not Passing TenantId
```csharp
// WRONG
await _notifications.CreateMaterialRequestNotificationAsync(model, "");  // Empty!

// RIGHT
await _notifications.CreateMaterialRequestNotificationAsync(model, model.TenantId);
```

## Expected Behavior

### When Planner Creates Request:
1. Material request saved with `TenantId = "admin-123"`
2. Notification created with:
   - `RecipientRole = "Admin"`
   - `TenantId = "admin-123"`
   - `IsRead = false`

### When Admin Logs In:
1. `ResolveTenantIdAsync` returns `"admin-123"` (Admin's user ID)
2. Query notifications WHERE:
   - `RecipientRole = "Admin"` ✅
   - `IsRead = false` ✅
   - `TenantId IS NULL OR TenantId = "admin-123"` ✅
3. Finds notification ✅
4. Badge shows count ✅
5. Dropdown shows notification ✅

### When Admin Clicks Notification:
1. Mark as read
2. Navigate to `/MaterialRequest?status=Pending`
3. Yellow banner shows notification
4. Badge count decreases

## Files to Check

1. **Controllers/NotificationController.cs**
   - `ResolveTenantIdAsync` method
   - `GetUnreadCount` method
   - `GetRecent` method

2. **Services/NotificationService.cs**
   - `GetUnreadRoleCountAsync` method
   - `GetUnreadRoleNotificationsAsync` method
   - `CreateMaterialRequestNotificationAsync` method

3. **Controllers/MaterialRequestController.cs**
   - `Create` method (POST)
   - `ResolveTenantIdAsync` method

4. **Views/Shared/_DashboardLayout.cshtml**
   - Notification loading query (line ~36)
   - Tenant ID resolution (line ~29)

## Success Criteria

- [ ] Notifications show in bell dropdown
- [ ] Badge count is correct
- [ ] Clicking notification navigates correctly
- [ ] Notification is marked as read
- [ ] Material Request page shows banner
- [ ] Multi-tenancy still works (Admin A can't see Admin B's notifications)
- [ ] No errors in logs
- [ ] No JavaScript errors in browser console

---

**Next Steps**: Run the SQL queries, check the logs, identify the scenario, apply the fix, test, and verify.
