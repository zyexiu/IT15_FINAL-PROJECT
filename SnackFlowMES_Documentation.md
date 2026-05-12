# SnackFlow MES

## Project Title
Web-Based Production Planning and Work Order System for Small Snack Food Manufacturing (SnackFlow MES)

## System Overview
SnackFlow MES is a web-based Manufacturing Execution System designed for small snack food manufacturers. It supports production planning, materials management, bill of materials management, inventory tracking, work order execution, production reporting, quality control, downtime reporting, notifications, and reporting for management.

The system is built with ASP.NET Core, Entity Framework Core, ASP.NET Core Identity, and a MySQL database in normal deployment. It also supports SQLite when the `RENDER_USE_SQLITE` environment variable is enabled.

## Business Scope
The system is intended for snack food production operations such as chips, crackers, and baked snack products. It helps the organization plan production, check material availability, create and track work orders, record actual production results, monitor quality checks, and summarize costs and operational issues.

## Technology Stack
| Layer | Technology |
| --- | --- |
| Backend | ASP.NET Core |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Database | MySQL, with SQLite support for Render deployment |
| UI | Razor Views |
| Access Control | Custom role-based menu and authorization filter |
| Deployment Support | Environment-based configuration for local and hosted deployment |

## Security Features
| Feature | Description |
| --- | --- |
| Authentication | Users sign in through the account system using ASP.NET Core Identity. |
| Role-Based Access Control | Access is controlled by a custom `RoleAccessFilter` and `RoleMenuService`. |
| Roles | The system uses five roles: Admin, Planner, Operator, QC, and Manager. |
| Account Status | Users can be marked active or inactive. |
| Password Policy | Passwords require a digit, lowercase letter, uppercase letter, non-alphanumeric character, and a minimum length of 8. |
| Lockout Policy | Accounts lock after 5 failed attempts for 15 minutes. |
| Multi-Tenancy | Most business data is linked to a tenant through `TenantId`, which ties users and records to an Admin workspace. |
| Login Protection | The login page includes reCAPTCHA support when enabled in configuration. |

## User Roles
### 1. Admin
Admin users manage the system administration side of the application. Their accessible areas include dashboard, user management, work orders, material requests, downtime reports, inventory master data, reports and analytics, and system settings.

### 2. Planner
Planners handle production planning and scheduling. Their accessible areas include dashboard, production planning, work orders, bill of materials, material requests, inventory, and reports.

### 3. Operator
Operators focus on floor execution. Their accessible areas include dashboard, assigned work orders, and downtime reporting.

### 4. QC
QC users perform inspections and review quality data. Their accessible areas include dashboard, quality inspection, and inspection history.

### 5. Manager
Managers monitor production performance and business results. Their accessible areas include dashboard, production overview, and monitoring-related views.

## Main Modules
| Module | Purpose |
| --- | --- |
| Account and Authentication | Login, logout, access denied handling, password management, and profile-related account functions. |
| Dashboard | Role-specific dashboard views for Admin, Planner, Operator, QC, and Manager. |
| User Management | Admin management of users and roles. |
| System Settings | Application settings and configuration management. |
| Items / Materials Master | Central list of raw materials, packaging materials, semi-finished items, and finished goods. |
| Bill of Materials | BOM header and BOM line management for finished goods. |
| Inventory | Current stock balance and historical stock movement ledger. |
| Production Planning | Production plan creation, scheduling, and line assignment. |
| MRP | Material requirement planning and shortage identification. |
| Work Order Management | Work order creation, release, status tracking, and completion. |
| Production Reporting | Recording produced quantity, scrap, labor hours, and machine hours. |
| Quality Control | QC inspection recording with pass/fail results and notes. |
| Reports | Production, inventory, and cost-related summaries for management. |
| Material Requests | Planner requests for stock replenishment or upcoming production needs. |
| Notifications | Role-based and user-based alerts tied to production, inventory, QC, and system events. |
| Downtime Reporting | Recording machine or process downtime, reasons, and resolution details. |
| Audit Logging | System activity tracking for important changes and actions. |

## Project Objectives
1. To create production plans and schedules for a small snack food manufacturing enterprise.
2. To compute required materials using MRP and identify shortages.
3. To create and monitor work orders from release to completion.
4. To record production output, scrap, material usage, QC results, and downtime.
5. To provide reports and cost summaries that support production monitoring and decision-making.

## Authentication and Access Flow
1. A user logs in using the account page.
2. The system verifies the user through ASP.NET Core Identity.
3. If reCAPTCHA is enabled, the login request is validated before authentication completes.
4. After login, the system reads the user’s role and loads the matching menu and dashboard.
5. The global role access filter blocks unauthorized controller access and redirects to Access Denied when needed.

## Default Demo Accounts
The repository seeds one account for each main role.

| Role | Username / Email | Default Password |
| --- | --- | --- |
| Admin | admin@snackflow.local | Admin@1234! |
| Planner | planner@snackflow.local | Planner@1234! |
| Operator | operator@snackflow.local | Operator@1234! |
| QC | qc@snackflow.local | QcUser@1234! |
| Manager | manager@snackflow.local | Manager@1234! |

## Data Dictionary
| Entity | Purpose | Key Fields |
| --- | --- | --- |
| ApplicationUser | Stores user accounts and role-related identity data. | UserName, Email, FullName, IsActive, TenantId |
| Item | Stores raw materials, packaging, semi-finished items, and finished goods. | ItemCode, ItemName, ItemType, UnitOfMeasure, Category, UnitCost, ReorderPoint, IsActive, IsArchived, TenantId |
| BillOfMaterials | BOM header for finished goods. | ItemId, Version, BatchOutputQty, BatchOutputUom, EstMachineHours, EstLaborHours, IsActive, Notes, TenantId |
| BomLine | BOM component line items. | BomId, ItemId, QtyPerBatch, UnitOfMeasure, ScrapFactor, Notes |
| InventoryBalance | Current stock snapshot per item. | ItemId, QtyOnHand, QtyReserved, QtyAvailable, LastUpdated, TenantId |
| InventoryLedger | Immutable stock movement log. | ItemId, MovementType, Qty, BalanceAfter, WorkOrderId, Reference, Notes, PostedByUserId, PostedAt, TenantId |
| ProductionPlan | Production plan header for a date range. | PlanName, PlanDateFrom, PlanDateTo, Status, Notes, CreatedByUserId, TenantId |
| PlanLine | Planned production entry for one item on one date. | PlanId, ItemId, PlannedQty, UnitOfMeasure, ScheduledDate, ProductionLine, Status, Notes |
| MrpRun | Header for one MRP calculation run. | PlanId, RunAt, RunByUserId, Status, Notes, TenantId |
| MrpRequirement | Computed material requirement line from MRP. | MrpRunId, ItemId, GrossRequirement, StockOnHand, NetRequirement, IsShortage |
| WorkOrder | Work order header and lifecycle data. | WoNumber, ItemId, BomId, PlanLineId, PlannedQty, ActualQty, UnitOfMeasure, Status, ProductionLine, ScheduledStart, ScheduledEnd, ActualStart, ActualEnd, CreatedByUserId, TenantId |
| WorkOrderMaterial | Planned and actual material usage for a work order. | WorkOrderId, ItemId, PlannedQty, ActualQty, UnitOfMeasure, UnitCostSnapshot |
| ProductionLog | Actual production output per work order and shift. | WorkOrderId, LogDate, Shift, ProducedQty, ScrapQty, UnitOfMeasure, LaborHours, MachineHours, RecordedByUserId, TenantId |
| QcResult | QC inspection record for a work order. | WorkOrderId, InspectedAt, Result, CheckType, SampleQty, DefectQty, Notes, Disposition, InspectedByUserId, TenantId |
| WorkOrderCost | Final cost summary for one work order. | WorkOrderId, MaterialCost, LaborCost, MachineCost, OtherCost, CostPerUnit, LaborRatePerHour, MachineRatePerHour, ComputedAt, ComputedByUserId |
| MaterialRequest | Request for materials from planning or operations. | ItemId, RequestedQty, UnitOfMeasure, Reason, Status, Priority, RequiredByDate, RequestedByUserId, ApprovedByUserId, ApprovedAt, FulfilledAt, FulfilledQty, TenantId |
| Notification | User or role-based system notification. | RecipientUserId, RecipientRole, Type, Title, Message, RelatedEntityType, RelatedEntityId, ActionUrl, IsRead, Priority, CreatedByUserId, TenantId |
| DowntimeReport | Tracks production downtime events. | WorkOrderId, ProductionLine, StartTime, EndTime, DurationMinutes, Reason, Description, ReportedByUserId, ReportedAt, Status, Resolution, ResolvedByUserId, ResolvedAt, OrganizationId |
| AuditLog | Security and activity trail for important actions. | UserId, UserName, Module, Action, EntityId, Description, OldValues, NewValues, IpAddress, Timestamp, TenantId |

## ERD Summary
The system follows a production-centered relational design.

| Relationship | Description |
| --- | --- |
| Admin to Tenant Data | Each Admin acts as a tenant owner. Most records store `TenantId` to keep data isolated by Admin workspace. |
| Item to BOM | A finished good item can have one BOM header, and one BOM contains many BOM lines. |
| Item to Inventory | Each stockable item has one inventory balance and many inventory ledger transactions. |
| Production Plan to Plan Lines | One production plan contains multiple plan lines. |
| Plan Line to Work Order | One plan line can generate one or more work orders. |
| Work Order to Materials | One work order has multiple planned material lines. |
| Work Order to Production Logs | One work order can have multiple production logs. |
| Work Order to QC Results | One work order can have inspection records. |
| Work Order to Cost Summary | One completed work order has one cost summary record. |
| Work Order to Downtime | One work order can have multiple downtime reports. |
| MRP Run to Requirements | One MRP run generates multiple material requirement rows. |
| Material Requests and Notifications | Material requests can generate notifications for the appropriate role or user. |

## Notes
1. The project uses `TenantId` for most business entities, but `DowntimeReport` currently uses `OrganizationId` in the model.
2. The account seed data in the application and the local `account.txt` file should be treated as demo credentials only.
3. The application is structured for role-based navigation, so each user sees only the menu items assigned to that role.
