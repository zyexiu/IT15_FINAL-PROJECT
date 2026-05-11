# Inventory Archive Feature Implementation

## Overview
Implemented a two-step deletion process for inventory items:
1. **Archive** - Soft delete that hides items from active inventory
2. **Delete** - Permanent deletion (only available for archived items)

## Changes Made

### 1. Database Changes
- **Added `IsArchived` field** to `Item` model (boolean, default: false)
- **Migration created and applied**: `20260510184049_AddIsArchivedToItem`
- Database updated successfully

### 2. Controller Changes (`Controllers/InventoryController.cs`)

#### Updated Index Action
- Added `archived` parameter to filter between active and archived items
- Added `ViewBag.ArchivedCount` to show count of archived items
- Added `ViewBag.ShowArchived` flag for UI state
- Default view shows only active items (IsArchived = false)

#### New Actions Added

**Archive (POST)**
- **Authorization**: Admin only
- **Validation checks**:
  - Item not already archived
  - Item not used in active BOMs
  - Item not used in active/in-progress work orders
- **Action**: Sets `IsArchived = true` and `IsActive = false`
- **Result**: Item hidden from active inventory, can be restored or deleted

**Unarchive (POST)**
- **Authorization**: Admin only
- **Validation**: Item must be archived
- **Action**: Sets `IsArchived = false` and `IsActive = true`
- **Result**: Item restored to active inventory

**Delete (POST)**
- **Authorization**: Admin only
- **Safety check**: Only archived items can be deleted
- **Cascade deletion**:
  - Removes InventoryBalance
  - Removes InventoryLedgers
  - Removes BomLines
  - Removes Item record
- **Error handling**: Catches foreign key constraints and provides user-friendly messages
- **Result**: Permanent deletion from database

### 3. View Changes

#### Index View (`Views/Inventory/Index.cshtml`)

**Header Section**
- Added "Show Archived" button (shows count)
- Added "Show Active Items" button (when viewing archived)
- Buttons toggle between active and archived views

**Alert Section**
- Added info alert when viewing archived items
- Added error alert display for TempData["Error"]

**Tabs & Filters**
- All tabs now preserve `archived` parameter
- Search form includes hidden `archived` field
- Filter form includes hidden `archived` field
- Pagination links include `archived` parameter

**Item Cards**
- Archived items show "Archived" badge (warning color) instead of item type
- All functionality preserved for both active and archived items

#### Details View (`Views/Inventory/Details.cshtml`)

**Header Section**
- **Active items**: Show Edit + Archive buttons
- **Archived items**: Show Restore + Delete Permanently buttons
- Edit button hidden for archived items
- Back button returns to correct list (active or archived)

**Alert Section**
- Added warning alert for archived items
- Explains that archived items cannot be edited or used
- Added error alert display

**Status Badge**
- Shows "Archived" (warning) for archived items
- Shows "Active" (success) for active items
- Shows "Inactive" (secondary) for inactive items

**Stock Adjustment Form**
- Hidden for archived items
- Only visible for active items with Admin/Planner role

## User Flow

### Archiving an Item
1. Admin navigates to item details
2. Clicks "Archive" button
3. Confirms action in dialog
4. System validates:
   - Not used in active BOMs
   - Not used in active work orders
5. Item is archived and hidden from active inventory
6. Success message displayed

### Restoring an Item
1. Admin clicks "Show Archived" button
2. Navigates to archived item details
3. Clicks "Restore" button
4. Confirms action
5. Item restored to active inventory
6. Success message displayed

### Permanently Deleting an Item
1. Admin views archived items
2. Navigates to archived item details
3. Clicks "Delete Permanently" button
4. Confirms with strong warning dialog
5. System deletes all related records
6. Item permanently removed from database
7. Success message displayed

## Safety Features

### Archive Validation
- ✅ Cannot archive items used in active BOMs
- ✅ Cannot archive items used in active/in-progress work orders
- ✅ Automatically deactivates item when archiving

### Delete Validation
- ✅ Only archived items can be deleted
- ✅ Cascade deletion of related records
- ✅ Foreign key constraint error handling
- ✅ Strong confirmation dialog with warning
- ✅ Detailed error messages for users

### UI/UX Features
- ✅ Clear visual distinction (badges, alerts)
- ✅ Separate views for active vs archived
- ✅ Count display for archived items
- ✅ Confirmation dialogs for destructive actions
- ✅ Disabled editing for archived items
- ✅ Contextual back button navigation

## Authorization
- **Archive**: Admin only
- **Unarchive**: Admin only
- **Delete**: Admin only
- **View archived**: All authenticated users
- **Edit**: Admin/Planner (active items only)

## Database Schema
```sql
ALTER TABLE `Items` ADD `IsArchived` tinyint(1) NOT NULL DEFAULT FALSE;
```

## Testing Checklist
- [ ] Archive an active item
- [ ] Try to archive item used in active BOM (should fail)
- [ ] Try to archive item used in active work order (should fail)
- [ ] View archived items list
- [ ] Restore an archived item
- [ ] Delete an archived item permanently
- [ ] Try to delete an active item (should fail)
- [ ] Verify archived items don't appear in active inventory
- [ ] Verify archived items can't be edited
- [ ] Verify archived items can't have stock adjusted
- [ ] Verify pagination works with archived filter
- [ ] Verify search works with archived filter
- [ ] Verify type tabs work with archived filter

## Files Modified
1. `Models/Item.cs` - Added IsArchived property
2. `Controllers/InventoryController.cs` - Added Archive/Unarchive/Delete actions, updated Index
3. `Views/Inventory/Index.cshtml` - Added archived filter, badges, alerts
4. `Views/Inventory/Details.cshtml` - Added Archive/Restore/Delete buttons, alerts
5. `Migrations/20260510184049_AddIsArchivedToItem.cs` - Database migration (auto-generated)

## Build Status
✅ Build succeeded with no errors
✅ Migration applied successfully
✅ All changes compiled correctly
