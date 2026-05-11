# Planner Inventory Access - Fixed to View-Only

## Issue
Planners had **Create/Edit** permissions on Inventory, which is incorrect. Inventory master data management should be an **Admin-only** function. Planners should only have **view access** to check material availability for planning purposes.

## Solution
Removed Planner from all Inventory Create/Edit/Update authorization attributes, making inventory management **Admin-only**.

---

## Changes Made

### 1. Controller Authorization (InventoryController.cs)

#### **Before (Incorrect):**
```csharp
[Authorize(Roles = "Admin,Planner")] // ❌ Planner could create/edit
public IActionResult Create() { ... }

[Authorize(Roles = "Admin,Planner")] // ❌ Planner could create/edit
public async Task<IActionResult> Create(ItemFormViewModel model) { ... }

[Authorize(Roles = "Admin,Planner")] // ❌ Planner could edit
public async Task<IActionResult> Edit(int id) { ... }

[Authorize(Roles = "Admin,Planner")] // ❌ Planner could edit
public async Task<IActionResult> Edit(int id, ItemFormViewModel model) { ... }

[Authorize(Roles = "Admin,Planner")] // ❌ Planner could adjust stock
public async Task<IActionResult> UpdateQuantity(...) { ... }
```

#### **After (Correct):**
```csharp
[Authorize(Roles = "Admin")] // ✅ Admin-only
public IActionResult Create() { ... }

[Authorize(Roles = "Admin")] // ✅ Admin-only
public async Task<IActionResult> Create(ItemFormViewModel model) { ... }

[Authorize(Roles = "Admin")] // ✅ Admin-only
public async Task<IActionResult> Edit(int id) { ... }

[Authorize(Roles = "Admin")] // ✅ Admin-only
public async Task<IActionResult> Edit(int id, ItemFormViewModel model) { ... }

[Authorize(Roles = "Admin")] // ✅ Admin-only
public async Task<IActionResult> UpdateQuantity(...) { ... }
```

### 2. View Updates

#### **Index View (Views/Inventory/Index.cshtml)**

**Before:**
```razor
@if (User.IsInRole("Admin") || User.IsInRole("Planner"))
{
    <a asp-action="Create" class="dash-btn dash-btn--primary">
        Add New Item
    </a>
}
```

**After:**
```razor
@if (User.IsInRole("Admin"))
{
    <a asp-action="Create" class="dash-btn dash-btn--primary">
        Add New Item
    </a>
}
```

#### **Details View (Views/Inventory/Details.cshtml)**

**Before:**
```razor
@if (User.IsInRole("Admin") || User.IsInRole("Planner"))
{
    @if (!Model.IsArchived)
    {
        <a asp-action="Edit">Edit</a>
    }
}

<!-- Stock Adjustment (Admin/Planner only) -->
@if ((User.IsInRole("Admin") || User.IsInRole("Planner")) && !Model.IsArchived)
{
    <!-- Stock adjustment form -->
}
```

**After:**
```razor
@if (User.IsInRole("Admin"))
{
    @if (!Model.IsArchived)
    {
        <a asp-action="Edit">Edit</a>
    }
}

<!-- Stock Adjustment (Admin only) -->
@if (User.IsInRole("Admin") && !Model.IsArchived)
{
    <!-- Stock adjustment form -->
}
```

---

## Access Control Summary

### Planner Inventory Access

| Action | Before | After | Reason |
|--------|--------|-------|--------|
| **View Index** | ✅ Allowed | ✅ Allowed | Need to check stock levels |
| **View Details** | ✅ Allowed | ✅ Allowed | Need to see item info |
| **Search/Filter** | ✅ Allowed | ✅ Allowed | Need to find items |
| **Create Item** | ❌ **Allowed** | ✅ **Blocked** | Master data = Admin job |
| **Edit Item** | ❌ **Allowed** | ✅ **Blocked** | Master data = Admin job |
| **Update Quantity** | ❌ **Allowed** | ✅ **Blocked** | Stock control = Admin job |
| **Archive Item** | ❌ Blocked | ❌ Blocked | Admin-only function |
| **Delete Item** | ❌ Blocked | ❌ Blocked | Admin-only function |

### What Planner CAN Do with Inventory

✅ **View inventory list** - See all items  
✅ **Search items** - Find specific materials  
✅ **Filter by type** - Raw materials, packaging, etc.  
✅ **View stock levels** - Check availability  
✅ **View item details** - See specifications  
✅ **Check low stock alerts** - Plan accordingly  
✅ **View inventory history** - See transactions  

### What Planner CANNOT Do with Inventory

❌ **Create new items** - Admin creates master data  
❌ **Edit item details** - Admin manages master data  
❌ **Adjust stock quantities** - Admin controls stock  
❌ **Archive items** - Admin manages lifecycle  
❌ **Delete items** - Admin manages lifecycle  
❌ **Change item codes** - Admin manages identifiers  
❌ **Set reorder points** - Admin sets thresholds  

---

## Why This Matters

### Separation of Concerns

**Admin Role:**
- **Master Data Management** - Define what items exist
- **System Configuration** - Set up the system
- **Data Integrity** - Ensure data quality
- **Stock Control** - Manage physical inventory

**Planner Role:**
- **Production Planning** - Schedule what to make
- **Resource Visibility** - See what's available
- **Material Requirements** - Calculate needs
- **Work Order Creation** - Plan production

### Security & Data Integrity

1. **Prevents Accidental Changes**
   - Planners can't accidentally modify item codes
   - Planners can't change unit costs
   - Planners can't alter reorder points

2. **Maintains Data Quality**
   - Only Admin creates/edits master data
   - Consistent naming conventions
   - Proper categorization

3. **Clear Accountability**
   - Admin responsible for inventory accuracy
   - Planner responsible for production planning
   - Clear audit trail

4. **Reduces Errors**
   - Fewer people with edit access
   - Less chance of conflicting changes
   - Better data governance

---

## Planner Workflow with Inventory

### Scenario: Creating a Work Order

1. **Check Material Availability**
   - Navigate to **Inventory**
   - Search for required materials
   - View stock levels
   - Check if sufficient quantity available

2. **If Materials Available**
   - Proceed to create work order
   - System will reserve materials

3. **If Materials Low/Unavailable**
   - **Option A:** Inform Admin to order materials
   - **Option B:** Adjust production schedule
   - **Option C:** Use alternative BOM if available

### Scenario: Planning Production

1. **Review Inventory Status**
   - Check finished goods stock
   - Check raw material levels
   - Identify low stock items

2. **Create Production Plan**
   - Based on available materials
   - Consider lead times
   - Schedule accordingly

3. **If Need New Items**
   - Request Admin to create new item codes
   - Wait for Admin to set up master data
   - Then proceed with planning

---

## User Experience Impact

### For Planners

**What Changes:**
- ❌ No "Add New Item" button visible
- ❌ No "Edit" button on item details
- ❌ No stock adjustment form
- ✅ Can still view everything
- ✅ Can still search and filter
- ✅ Can still check availability

**Workflow Adjustment:**
- If need new item → Request Admin to create it
- If need stock adjustment → Inform Admin
- If see data error → Report to Admin

**Benefits:**
- ✅ Clearer role boundaries
- ✅ Less confusion about responsibilities
- ✅ Focus on planning, not data entry
- ✅ Reduced risk of errors

### For Admins

**Benefits:**
- ✅ Full control over master data
- ✅ Better data quality
- ✅ Clear ownership
- ✅ Easier to maintain standards

**Responsibilities:**
- Create new inventory items
- Update item specifications
- Adjust stock quantities
- Manage item lifecycle

---

## Testing Checklist

### As Planner:
- [ ] Can view inventory list
- [ ] Can search items
- [ ] Can filter by type
- [ ] Can view item details
- [ ] Can see stock levels
- [ ] **Cannot** see "Add New Item" button
- [ ] **Cannot** see "Edit" button on details
- [ ] **Cannot** see stock adjustment form
- [ ] **Cannot** access `/Inventory/Create` URL (403 Forbidden)
- [ ] **Cannot** access `/Inventory/Edit/1` URL (403 Forbidden)

### As Admin:
- [ ] Can view inventory list
- [ ] Can see "Add New Item" button
- [ ] Can create new items
- [ ] Can edit items
- [ ] Can adjust stock
- [ ] Can archive items
- [ ] Can delete archived items
- [ ] All functionality works as before

---

## Files Modified

1. **Controllers/InventoryController.cs**
   - Changed `Create()` GET from `[Authorize(Roles = "Admin,Planner")]` to `[Authorize(Roles = "Admin")]`
   - Changed `Create()` POST from `[Authorize(Roles = "Admin,Planner")]` to `[Authorize(Roles = "Admin")]`
   - Changed `Edit()` GET from `[Authorize(Roles = "Admin,Planner")]` to `[Authorize(Roles = "Admin")]`
   - Changed `Edit()` POST from `[Authorize(Roles = "Admin,Planner")]` to `[Authorize(Roles = "Admin")]`
   - Changed `UpdateQuantity()` from `[Authorize(Roles = "Admin,Planner")]` to `[Authorize(Roles = "Admin")]`

2. **Views/Inventory/Index.cshtml**
   - Changed "Add New Item" button from `Admin || Planner` to `Admin` only
   - Changed empty state button from `Admin || Planner` to `Admin` only

3. **Views/Inventory/Details.cshtml**
   - Changed "Edit" button from `Admin || Planner` to `Admin` only
   - Changed stock adjustment form from `Admin || Planner` to `Admin` only
   - Updated comment from "Admin/Planner only" to "Admin only"

---

## Build Status
✅ **Build succeeded** - All changes compiled successfully  
✅ **No errors** - Ready for deployment  
✅ **Access control fixed** - Planner now view-only  
✅ **UI updated** - Buttons hidden from Planner  

---

## Conclusion

Planner inventory access is now correctly set to **view-only**. This change:

✅ **Improves security** - Fewer users with edit access  
✅ **Enhances data quality** - Admin controls master data  
✅ **Clarifies roles** - Clear separation of responsibilities  
✅ **Reduces errors** - Less chance of accidental changes  
✅ **Maintains functionality** - Planners can still view everything they need  

Planners can now focus on their core responsibility: **planning and scheduling production**, while Admins maintain control over **inventory master data management**.
