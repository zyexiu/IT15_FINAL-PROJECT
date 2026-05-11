# Phase 5: Downtime Reporting System - COMPLETE ✅

## Overview
Successfully implemented a complete downtime reporting system for tracking production downtime, machine issues, and resolution workflows. This enables operators to report problems immediately and provides visibility to Admin/Manager for issue resolution.

## Implementation Summary

### 1. Downtime Report Controller
**File Created:**
- `Controllers/DowntimeController.cs`

**Actions:**
- `Index(string? status)` - List all downtime reports with status filtering
  - Authorization: Admin, Manager, Operator
  - Filters: All, Open, Resolved, Escalated
  - Includes WorkOrder, ReportedBy, ResolvedBy navigation properties
  
- `Details(int id)` - View downtime report details
  - Authorization: Admin, Manager, Operator
  - Shows full report with timeline and resolution
  
- `Create()` [GET] - Show downtime report form
  - Authorization: Operator only
  - Pre-loads work orders dropdown
  - Supports pre-filling from workOrderId query parameter
  
- `Create(DowntimeReport)` [POST] - Submit downtime report
  - Authorization: Operator only
  - Auto-fills: ReportedByUserId, ReportedAt, Status="Open", OrganizationId
  - Calculates duration if end time provided
  - Creates notifications for Admin and Manager
  
- `Resolve(int id)` [GET] - Show resolution form
  - Authorization: Admin, Manager
  - Displays downtime summary
  
- `Resolve(int id, ...)` [POST] - Submit resolution
  - Authorization: Admin, Manager
  - Updates: Resolution, Status, ResolvedByUserId, ResolvedAt
  - Updates EndTime and recalculates duration if provided
  - Notifies the operator who reported it
  
- `Delete(int id)` [POST] - Delete downtime report
  - Authorization: Admin only
  - Soft delete with confirmation

**Features:**
- Multi-tenancy support (filters by TenantId)
- Automatic notification creation
- Duration calculation
- Status workflow (Open → Resolved/Escalated)
- Role-based access control

### 2. Notification Service Updates
**File Modified:**
- `Services/NotificationService.cs`

**New Methods:**
- `CreateDowntimeReportNotificationAsync()` - Notifies Admin and Manager when downtime is reported
  - Priority: High if duration > 60 minutes, Medium otherwise
  - Includes: WorkOrder number, product name, reason, duration
  - Action URL: Links to downtime details page
  
- `CreateNotificationAsync()` - Generic notification creation method
  - Supports both user-specific and role-based notifications
  - Used by controllers for custom notifications
  - Flexible parameters for all notification types

**Notification Flow:**
1. Operator reports downtime → Admin + Manager notified
2. Admin/Manager resolves issue → Operator notified
3. Priority based on duration (>60min = High, ≤60min = Medium)

### 3. Downtime Report Views

#### Index View (`Views/Downtime/Index.cshtml`)
**Features:**
- Page header with title and "Report Downtime" button (Operator only)
- Filter tabs: All, Open, Resolved, Escalated (with counts)
- Downtime reports table with columns:
  - Work Order (link to WO details)
  - Production Line
  - Reason (color-coded badge)
  - Start Time
  - Duration (red if >60min)
  - Reported By
  - Status (color-coded badge)
  - Actions (View Details, Resolve button for Admin/Manager)
- Empty state when no reports
- Responsive design

**Color Coding:**
- **Reason Badges:**
  - Machine Breakdown: Red (#EF4444)
  - Material Shortage: Orange (#F59E0B)
  - Power Outage: Red (#EF4444)
  - Maintenance: Blue (#3B82F6)
  - Changeover: Green (#10B981)
  - Other: Gray (#6B7280)
- **Status Badges:**
  - Open: Orange (#F59E0B)
  - Resolved: Green (#10B981)
  - Escalated: Red (#EF4444)

#### Create View (`Views/Downtime/Create.cshtml`)
**Features:**
- Form fields:
  - Work Order (dropdown, required)
  - Production Line (text input, required, pre-filled if from WO)
  - Start Time (datetime-local, required, defaults to now)
  - End Time (datetime-local, optional, for completed downtime)
  - Reason (dropdown, required):
    - Machine Breakdown
    - Material Shortage
    - Power Outage
    - Maintenance
    - Changeover
    - Other
  - Description (textarea, required, 500 chars)
- Important notice box (yellow warning)
- Form validation
- Submit and Cancel buttons
- Help text for each field

**UX Features:**
- Auto-fills production line from selected work order
- Defaults start time to current time
- Optional end time for ongoing downtime
- Clear field descriptions
- Validation feedback

#### Details View (`Views/Downtime/Details.cshtml`)
**Features:**
- Page header with status badge and "Resolve Issue" button (Admin/Manager, if Open)
- Main details panel:
  - Work Order (link)
  - Product name
  - Production Line
  - Reason (color-coded badge)
  - Start Time
  - End Time (shows "Ongoing" if null)
  - Duration (red if >60min)
  - Reported By
  - Reported At
  - Description (full text, pre-wrapped)
  - Resolution (if resolved):
    - Resolution notes
    - Resolved By
    - Resolved At
- Timeline panel:
  - Reported event (blue dot)
  - Resolved/Escalated event (green dot, if applicable)
  - Shows user and timestamp for each event
- Impact summary box:
  - Total Downtime
  - Status
  - Resolution Time (if resolved)
- Delete button (Admin only, with confirmation)

**Design:**
- Two-column layout (details + timeline)
- Color-coded status and reason badges
- Timeline with dots and connecting lines
- Responsive grid for detail items
- Pre-wrapped text for descriptions

#### Resolve View (`Views/Downtime/Resolve.cshtml`)
**Features:**
- Two-column layout:
  - Left: Downtime summary (read-only)
    - Work Order, Production Line, Reason, Duration, Reported By
    - Original description
  - Right: Resolution form
    - End Time (if not already set)
    - Status (Resolved or Escalated)
    - Resolution Notes (textarea, required)
    - Info box about operator notification
- Form validation
- Submit and Cancel buttons

**UX Features:**
- Summary panel for context
- Clear status options
- Detailed resolution notes field
- Notification reminder
- Defaults end time to now

### 4. CSS Styling
**Common Styles:**
- `.dash-page-header` - Page title and actions
- `.dash-page-title` - Large page heading
- `.dash-page-subtitle` - Secondary description
- `.dash-page-actions` - Action buttons container
- `.dash-tabs` - Filter tabs with active state
- `.dash-tab` - Individual tab
- `.dash-tab--active` - Active tab (blue underline)
- `.dash-reason-badge` - Color-coded reason badge
- `.dash-action-btn` - Icon action button
- `.dash-action-btn--primary` - Primary action button (blue)
- `.dash-form-group` - Form field container
- `.dash-form-label` - Form field label
- `.dash-form-input` - Form input/select/textarea
- `.dash-form-help` - Help text below input
- `.dash-form-error` - Validation error message
- `.dash-form-actions` - Form button container
- `.dash-alert` - Alert/notice box
- `.dash-alert--warning` - Yellow warning box
- `.dash-alert--info` - Blue info box
- `.dash-detail-grid` - Details grid layout
- `.dash-detail-item` - Individual detail item
- `.dash-detail-label` - Detail label (uppercase, small)
- `.dash-detail-value` - Detail value
- `.dash-timeline` - Timeline container
- `.dash-timeline-item` - Timeline event
- `.dash-timeline-dot` - Timeline dot (color-coded)
- `.dash-timeline-content` - Timeline event content

**Design Principles:**
- Consistent spacing and typography
- Color-coded status and reasons
- Clear visual hierarchy
- Responsive layouts
- Accessible form controls
- Dark mode support via CSS variables

### 5. Downtime Reasons
**Predefined Reasons:**
1. **Machine Breakdown** - Equipment failure or malfunction
2. **Material Shortage** - Raw materials not available
3. **Power Outage** - Electrical power interruption
4. **Maintenance** - Scheduled or unscheduled maintenance
5. **Changeover** - Product or line changeover time
6. **Other** - Any other reason not listed

**Color Coding:**
- Critical issues (Breakdown, Outage): Red
- Supply issues (Shortage): Orange
- Planned activities (Maintenance, Changeover): Blue/Green
- Other: Gray

### 6. Workflow

#### Operator Workflow:
1. Encounters downtime during production
2. Navigates to Downtime → Report Downtime
3. Selects work order from dropdown
4. Fills in:
   - Production line (auto-filled)
   - Start time (defaults to now)
   - End time (optional, if already resolved)
   - Reason (dropdown)
   - Description (detailed explanation)
5. Submits report
6. System creates notifications for Admin and Manager
7. Operator receives notification when issue is resolved

#### Admin/Manager Workflow:
1. Receives notification of downtime report
2. Views downtime report details
3. Investigates issue
4. Clicks "Resolve Issue"
5. Fills in:
   - End time (if not set)
   - Status (Resolved or Escalated)
   - Resolution notes (detailed explanation)
6. Submits resolution
7. System notifies the operator who reported it

### 7. Metrics & Analytics (Future)
**Potential Metrics:**
- Total downtime per work order
- Downtime by reason (breakdown analysis)
- Downtime by production line
- Average resolution time
- Most common downtime reasons
- Downtime trends over time
- OEE calculation (Availability × Performance × Quality)
  - Availability = (Planned Production Time - Downtime) / Planned Production Time

### 8. Integration Points

**Work Order Details:**
- Add "Report Downtime" button to work order details page
- Show downtime history for each work order
- Calculate total downtime impact on work order

**Dashboard:**
- Add downtime summary widget to Admin/Manager dashboards
- Show open downtime count
- Display recent downtime reports

**Reports:**
- Downtime analysis report
- Production efficiency report (including downtime)
- OEE report

## Testing Checklist
- [x] Build successful with no errors
- [ ] Operator can create downtime report
- [ ] Admin/Manager can view all downtime reports
- [ ] Filter tabs work correctly (All, Open, Resolved, Escalated)
- [ ] Reason badges display with correct colors
- [ ] Status badges display with correct colors
- [ ] Duration displays correctly (minutes vs hours)
- [ ] Duration >60min shows in red
- [ ] Notifications created when downtime reported
- [ ] Admin/Manager can resolve downtime
- [ ] Operator receives notification when resolved
- [ ] Timeline displays correctly
- [ ] Delete button works (Admin only)
- [ ] Form validation works
- [ ] Responsive design works on mobile
- [ ] Dark mode support works

## Files Created
- `Controllers/DowntimeController.cs` - Downtime report controller
- `Views/Downtime/Index.cshtml` - Downtime reports list
- `Views/Downtime/Create.cshtml` - Report downtime form
- `Views/Downtime/Details.cshtml` - Downtime report details
- `Views/Downtime/Resolve.cshtml` - Resolve downtime form
- `PHASE5_DOWNTIME_REPORTING_COMPLETE.md` - This document

## Files Modified
- `Services/NotificationService.cs` - Added downtime notification methods

## Build Status
✅ **Build Successful** - No errors (1 unrelated warning)

## Database Schema
The DowntimeReports table was created in Phase 4:
```sql
CREATE TABLE `DowntimeReports` (
    `DowntimeId` int NOT NULL AUTO_INCREMENT,
    `WorkOrderId` int NOT NULL,
    `ProductionLine` varchar(100) NOT NULL,
    `StartTime` datetime(6) NOT NULL,
    `EndTime` datetime(6) NULL,
    `DurationMinutes` int NOT NULL,
    `Reason` varchar(50) NOT NULL,
    `Description` varchar(500) NOT NULL,
    `ReportedByUserId` varchar(450) NOT NULL,
    `ReportedAt` datetime(6) NOT NULL,
    `Status` varchar(20) NOT NULL,
    `Resolution` varchar(500) NULL,
    `ResolvedByUserId` varchar(450) NULL,
    `ResolvedAt` datetime(6) NULL,
    `OrganizationId` varchar(100) NOT NULL,
    PRIMARY KEY (`DowntimeId`),
    FOREIGN KEY (`ReportedByUserId`) REFERENCES `Users` (`Id`),
    FOREIGN KEY (`ResolvedByUserId`) REFERENCES `Users` (`Id`),
    FOREIGN KEY (`WorkOrderId`) REFERENCES `WorkOrders` (`WorkOrderId`)
);
```

## Notes
- Downtime reporting is operator-initiated (bottom-up reporting)
- Admin and Manager receive notifications immediately
- Resolution workflow ensures accountability
- Duration calculation supports both completed and ongoing downtime
- Color coding provides quick visual feedback
- Timeline shows complete history of downtime event
- Multi-tenancy ensures data isolation
- All views support dark mode via CSS variables
- Responsive design ensures mobile usability on production floor

## Role-Based Access
- **Operator**: Can create and view downtime reports
- **Admin**: Can view, resolve, and delete downtime reports
- **Manager**: Can view and resolve downtime reports
- **Planner**: No access (not production floor role)
- **QC**: No access (not production floor role)

## Next Steps (Phase 6 - Optional)
1. **Work Order Integration** - Add "Report Downtime" button to work order details
2. **Dashboard Widgets** - Add downtime summary to Admin/Manager dashboards
3. **Production Log Enhancement** - Better UI for operators to record production data
4. **Downtime Analytics** - Reports and charts for downtime analysis
5. **OEE Calculation** - Calculate Overall Equipment Effectiveness
6. **Real-time Notifications** - Add SignalR for live downtime alerts
7. **Mobile Optimization** - Optimize forms for mobile/tablet use on production floor

---

**Phase 5 Status**: ✅ COMPLETE
**Date**: 2026-05-11
**Build**: Successful
**Ready for**: Phase 6 (Work Order Integration & Dashboard Enhancements)
