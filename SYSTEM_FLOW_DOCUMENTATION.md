# SnackFlow MES - System Flow Documentation
## Manufacturing Execution System for Snack Food Production

---

## Table of Contents
1. [System Overview](#system-overview)
2. [Role-Based Access Control (RBAC)](#role-based-access-control-rbac)
3. [Complete System Flow](#complete-system-flow)
4. [Module-by-Module Breakdown](#module-by-module-breakdown)
5. [User Workflows by Role](#user-workflows-by-role)

---

## System Overview

**SnackFlow MES** is a comprehensive Manufacturing Execution System designed specifically for snack food production facilities. The system manages the entire production lifecycle from planning to quality control, with role-based access ensuring each user only sees and performs tasks relevant to their position.

### Key Features:
- **Role-Based Access Control (RBAC)** - 5 distinct user roles with customized dashboards
- **Multi-Tenant Architecture** - Each Admin manages their own isolated workspace
- **End-to-End Production Management** - From raw materials to finished goods
- **Real-Time Inventory Tracking** - Automatic stock updates with every transaction
- **Quality Control Integration** - Built-in QC checkpoints and reporting
- **Cost Tracking** - Material, labor, and machine cost calculations
- **Audit Trail** - Complete history of all system activities

---

## Role-Based Access Control (RBAC)

The system has **5 user roles**, each with specific permissions and responsibilities:

### 1. **Admin (System Administrator)**
- **Access Level**: Full system access
- **Responsibilities**:
  - User management (create, edit, deactivate users)
  - System configuration (email, reCAPTCHA, settings)
  - View all reports and dashboards
  - Manage all modules
- **Dashboard**: Complete overview with access to all features

### 2. **Planner (Production Planner/Scheduler)**
- **Access Level**: Planning and scheduling modules
- **Responsibilities**:
  - Create production plans
  - Schedule work orders
  - Run MRP (Material Requirements Planning)
  - Monitor material shortages
  - View inventory levels
- **Dashboard**: Planning-focused with production calendar and MRP tools

### 3. **Operator (Production Operator)**
- **Access Level**: Execution and reporting modules
- **Responsibilities**:
  - Execute work orders
  - Record production output
  - Report scrap/waste
  - Issue materials from inventory
  - Update work order status
- **Dashboard**: Work order execution and production logging

### 4. **QC (Quality Control Inspector)**
- **Access Level**: Quality control and inspection modules
- **Responsibilities**:
  - Inspect finished goods
  - Record QC results (Pass/Fail)
  - Document defects
  - Approve or reject batches
  - Generate quality reports
- **Dashboard**: QC inspection queue and quality metrics

### 5. **Manager (Production Manager)**
- **Access Level**: Read-only access to reports and analytics
- **Responsibilities**:
  - View production reports
  - Monitor KPIs and metrics
  - Review cost summaries
  - Analyze production efficiency
  - Make strategic decisions
- **Dashboard**: Executive summary with charts and analytics

---

## Complete System Flow

### **Phase 1: Setup & Master Data**
```
Admin logs in
    ↓
Creates user accounts (Planner, Operator, QC, Manager)
    ↓
Sets up Items (Raw Materials, Packaging, Finished Goods)
    ↓
Creates Bill of Materials (BOM) for each finished good
    ↓
System is ready for production
```

### **Phase 2: Production Planning**
```
Planner logs in
    ↓
Creates Production Plan (date range, items to produce)
    ↓
Adds Plan Lines (specific items, quantities, dates)
    ↓
Runs MRP (Material Requirements Planning)
    ↓
System calculates material needs and identifies shortages
    ↓
Planner reviews MRP results
    ↓
Creates Work Orders from approved plan
```

### **Phase 3: Work Order Execution**
```
Operator logs in
    ↓
Views assigned Work Orders
    ↓
Releases Work Order (status: Draft → Released)
    ↓
System reserves materials from inventory
    ↓
Operator starts production (status: Released → InProgress)
    ↓
Issues materials (inventory deducted)
    ↓
Records production output (quantity produced, scrap)
    ↓
Logs labor hours and machine hours
    ↓
Completes Work Order (status: InProgress → Completed)
    ↓
System posts finished goods to inventory
```

### **Phase 4: Quality Control**
```
QC Inspector logs in
    ↓
Views completed Work Orders awaiting inspection
    ↓
Performs quality checks (appearance, weight, taste, etc.)
    ↓
Records QC Result (Pass/Fail/Conditional Pass)
    ↓
Documents defects (if any)
    ↓
Sets disposition (Accept/Rework/Reject)
    ↓
System updates Work Order status
```

### **Phase 5: Cost Calculation & Reporting**
```
System automatically calculates:
    - Material Cost (actual materials used × unit cost)
    - Labor Cost (labor hours × labor rate)
    - Machine Cost (machine hours × machine rate)
    - Total Cost per Work Order
    - Cost per Unit
    ↓
Manager logs in
    ↓
Views production reports
    ↓
Analyzes cost efficiency
    ↓
Reviews quality metrics
    ↓
Makes decisions for next production cycle
```

---

## Module-by-Module Breakdown

### **Module 1: Dashboard**
**Purpose**: Personalized home screen for each role

**Features**:
- Role-specific widgets and metrics
- Quick access to frequently used functions
- Real-time notifications and alerts
- Production status overview

**Example (Admin Dashboard)**:
- Total users count
- Active work orders
- Inventory alerts
- Recent activities
- Quick links to all modules

---

### **Module 2: Items / Materials Master**
**Purpose**: Central database of all materials and products

**Features**:
- Create/Edit/View items
- Item types: Raw Material, Packaging, Finished Good, Semi-Finished
- Track unit cost, reorder point, category
- Active/Inactive status

**Data Stored**:
- Item Code (e.g., RM-001, FG-001)
- Item Name
- Unit of Measure (kg, pcs, box)
- Unit Cost (for cost calculations)
- Reorder Point (minimum stock level)

**Example Items**:
- RM-001: Potato Chips (Raw Material)
- PK-001: Plastic Bag 100g (Packaging)
- FG-001: Cheese Potato Chips 100g (Finished Good)

---

### **Module 3: Bill of Materials (BOM)**
**Purpose**: Recipe/formula for producing finished goods

**Features**:
- Define ingredients and packaging for each product
- Specify quantities per batch
- Version control (v1, v2, v3)
- Estimate labor and machine hours

**BOM Structure**:
```
Finished Good: Cheese Potato Chips 100g
Batch Output: 1000 pcs
├── Potato Chips (Raw) - 80 kg
├── Cheese Powder - 15 kg
├── Salt - 2 kg
├── Plastic Bag 100g - 1000 pcs
└── Carton Box (50 pcs) - 20 boxes

Estimated Labor: 4 hours
Estimated Machine: 6 hours
```

**Why It's Important**:
- Ensures consistent product quality
- Enables automatic material calculation
- Supports cost estimation
- Facilitates MRP planning

---

### **Module 4: Inventory Management**
**Purpose**: Track stock levels in real-time

**Features**:
- **Inventory Balance**: Current stock snapshot per item
- **Inventory Ledger**: Complete transaction history
- Movement types: Goods Receipt, Adjustment, WO Issue, WO Return, Production Output, Scrap

**How It Works**:
1. **Initial Stock**: Admin records opening balance
2. **Goods Receipt**: Stock increases (purchase, delivery)
3. **WO Issue**: Stock decreases (materials issued to production)
4. **Production Output**: Finished goods stock increases
5. **Scrap**: Stock decreases (waste, damaged goods)

**Real-Time Updates**:
- Every transaction updates Inventory Balance
- Ledger maintains complete audit trail
- System alerts when stock falls below reorder point

**Example Ledger**:
```
Item: Potato Chips (Raw)
Date       | Type            | Qty    | Balance After
-----------|-----------------|--------|---------------
May 1      | Opening Balance | +500kg | 500kg
May 2      | WO Issue        | -80kg  | 420kg
May 3      | Goods Receipt   | +200kg | 620kg
May 4      | WO Issue        | -80kg  | 540kg
```

---

### **Module 5: Production Planning**
**Purpose**: Schedule what to produce and when

**Features**:
- Create production plans (weekly, monthly)
- Add multiple plan lines (different products)
- Assign to production lines
- Track plan status (Draft, Approved, InProgress, Completed)

**Planning Process**:
1. **Planner creates plan**: "May 2026 Production Plan"
2. **Adds plan lines**:
   - May 5: Cheese Chips - 5000 pcs
   - May 7: BBQ Chips - 3000 pcs
   - May 10: Sour Cream Chips - 4000 pcs
3. **Approves plan**: Status changes to Approved
4. **Creates work orders**: Converts plan lines to executable work orders

---

### **Module 6: Material Requirements Planning (MRP)**
**Purpose**: Calculate material needs and identify shortages

**Features**:
- Explode BOMs to calculate gross requirements
- Compare with current inventory
- Identify shortages (net requirements)
- Generate purchase recommendations

**MRP Calculation**:
```
Production Plan: 5000 pcs Cheese Chips

BOM Requirements (per 1000 pcs):
- Potato Chips: 80kg
- Cheese Powder: 15kg

Gross Requirement (for 5000 pcs):
- Potato Chips: 400kg
- Cheese Powder: 75kg

Current Stock:
- Potato Chips: 540kg ✓ (sufficient)
- Cheese Powder: 50kg ✗ (shortage: 25kg)

MRP Result:
- Potato Chips: No action needed
- Cheese Powder: Purchase 25kg (or more)
```

**Why It's Critical**:
- Prevents production delays due to material shortages
- Optimizes inventory levels
- Reduces waste and overstocking
- Supports just-in-time manufacturing

---

### **Module 7: Work Orders**
**Purpose**: Execute production based on approved plans

**Features**:
- Create work orders (manual or from plan)
- Link to BOM for material explosion
- Track status: Draft → Released → InProgress → Completed
- Record actual vs. planned quantities
- Assign to production lines and shifts

**Work Order Lifecycle**:

**1. Draft**:
- WO created by Planner
- Specifies: Item, Quantity, BOM, Schedule
- Not yet executable

**2. Released**:
- Planner releases WO
- System explodes BOM → creates material list
- Materials reserved in inventory
- Ready for production

**3. InProgress**:
- Operator starts production
- Issues materials (inventory deducted)
- Records production logs
- Updates actual quantities

**4. Completed**:
- Operator completes WO
- Final output recorded
- Finished goods posted to inventory
- Ready for QC inspection

**Example Work Order**:
```
WO Number: WO-2026-0001
Item: Cheese Potato Chips 100g
Planned Qty: 5000 pcs
BOM: v1
Status: InProgress
Scheduled: May 5, 2026 08:00 - 16:00
Production Line: Line 1

Materials (from BOM):
- Potato Chips: 400kg (Planned) | 395kg (Actual)
- Cheese Powder: 75kg (Planned) | 73kg (Actual)
- Plastic Bags: 5000 pcs (Planned) | 5000 pcs (Actual)

Production Output:
- Good Units: 4850 pcs
- Scrap: 150 pcs
- Total: 5000 pcs
```

---

### **Module 8: Production Logging**
**Purpose**: Record actual production activities

**Features**:
- Log produced quantity per shift
- Record scrap/waste
- Track labor hours and machine hours
- Multiple logs per work order (one per shift)

**Production Log Entry**:
```
Work Order: WO-2026-0001
Date: May 5, 2026
Shift: Morning (08:00 - 16:00)
Produced Qty: 4850 pcs
Scrap Qty: 150 pcs
Labor Hours: 4.5 hours
Machine Hours: 6.2 hours
Operator: John Doe
Notes: Minor jam at 14:30, resolved quickly
```

**Why It's Important**:
- Tracks actual vs. planned performance
- Identifies production bottlenecks
- Supports labor cost calculation
- Provides data for efficiency analysis

---

### **Module 9: Quality Control**
**Purpose**: Ensure product quality meets standards

**Features**:
- Inspect completed work orders
- Record pass/fail results
- Document defects and issues
- Set disposition (Accept/Rework/Reject)
- Generate quality reports

**QC Inspection Process**:
1. **QC Inspector selects completed WO**
2. **Performs checks**:
   - Appearance (color, shape, texture)
   - Weight (meets specification?)
   - Moisture content
   - Taste test
   - Packaging integrity
3. **Records result**: Pass / Fail / Conditional Pass
4. **Documents defects** (if any)
5. **Sets disposition**:
   - **Accept**: Ship to customers
   - **Rework**: Reprocess or repackage
   - **Reject**: Discard or downgrade

**QC Result Example**:
```
Work Order: WO-2026-0001
Inspected By: Jane Smith (QC Inspector)
Inspection Date: May 5, 2026 16:30
Result: Pass
Check Type: Overall Quality
Sample Qty: 50 pcs
Defect Qty: 2 pcs (minor packaging issues)
Disposition: Accept
Notes: Overall quality excellent, minor packaging defects within acceptable range
```

---

### **Module 10: Cost Summary**
**Purpose**: Calculate total production costs

**Features**:
- Automatic cost calculation per work order
- Break down by cost type (material, labor, machine)
- Calculate cost per unit
- Support profitability analysis

**Cost Calculation**:
```
Work Order: WO-2026-0001
Actual Output: 4850 pcs

Material Cost:
- Potato Chips: 395kg × ₱50/kg = ₱19,750
- Cheese Powder: 73kg × ₱200/kg = ₱14,600
- Plastic Bags: 5000 pcs × ₱0.50/pc = ₱2,500
Total Material Cost: ₱36,850

Labor Cost:
- Labor Hours: 4.5 hours × ₱150/hour = ₱675

Machine Cost:
- Machine Hours: 6.2 hours × ₱100/hour = ₱620

Total Cost: ₱38,145
Cost per Unit: ₱38,145 ÷ 4850 pcs = ₱7.87/pc
```

**Why It's Important**:
- Determines product profitability
- Identifies cost-saving opportunities
- Supports pricing decisions
- Enables variance analysis (actual vs. standard cost)

---

### **Module 11: Reports**
**Purpose**: Provide insights and analytics

**Available Reports**:
1. **Production Summary**: Total output by item, date range
2. **Inventory Report**: Current stock levels, shortages
3. **Work Order Status**: Active, completed, cancelled WOs
4. **QC Report**: Pass/fail rates, defect analysis
5. **Cost Analysis**: Cost trends, variance reports
6. **Material Consumption**: Usage by item, period
7. **Labor Efficiency**: Hours worked vs. output
8. **Scrap Report**: Waste analysis by product

**Report Features**:
- Filter by date range, item, status
- Export to PDF, Excel
- Print-friendly format
- Visual charts and graphs

---

### **Module 12: User Management** (Admin Only)
**Purpose**: Manage system users and access

**Features**:
- Create new users (Planner, Operator, QC, Manager)
- Assign roles (determines dashboard and permissions)
- Activate/Deactivate accounts
- Reset passwords
- Delete inactive users (with safety checks)

**User Creation Process**:
1. Admin clicks "Add New User"
2. Fills in: Full Name, Email, Username, Password
3. Selects Role: Planner / Operator / QC / Manager
4. User receives welcome email
5. User can log in with assigned credentials

**Security Features**:
- Admin role can only be created via signup (not in user management)
- Cannot delete users with associated records (data integrity)
- Cannot delete or deactivate own account
- Password requirements: 8+ chars, uppercase, lowercase, digit, special char

---

### **Module 13: Settings** (Admin Only)
**Purpose**: Configure system settings

**Settings Available**:
1. **Email Configuration**:
   - SMTP server settings
   - Welcome email templates
   - Notification settings

2. **reCAPTCHA Configuration**:
   - Site key and secret key
   - Minimum score threshold
   - Enable/disable for testing

3. **System Information**:
   - Application version
   - Database connection status
   - Server information

---

### **Module 14: Audit Log**
**Purpose**: Track all system activities for security and compliance

**Logged Activities**:
- User logins/logouts
- Work order status changes
- Inventory transactions
- QC results
- User management actions
- System configuration changes

**Audit Log Entry**:
```
Timestamp: May 5, 2026 14:30:15
User: john.operator
Module: WorkOrder
Action: StatusChange
Entity ID: WO-2026-0001
Description: Work Order status changed from Released to InProgress
Old Value: Released
New Value: InProgress
IP Address: 192.168.1.100
```

**Why It's Important**:
- Security and compliance
- Troubleshooting and debugging
- Performance monitoring
- Accountability and transparency

---

## User Workflows by Role

### **Admin Workflow**
```
1. Sign up (becomes tenant owner)
2. Create user accounts for team
3. Set up items (raw materials, packaging, finished goods)
4. Create BOMs for each product
5. Configure system settings
6. Monitor overall system health
7. View reports and analytics
8. Manage users (activate/deactivate/reset passwords)
```

---

### **Planner Workflow**
```
1. Log in to dashboard
2. Review current inventory levels
3. Create production plan for the week/month
4. Add plan lines (products to produce)
5. Run MRP to check material availability
6. Review MRP results (identify shortages)
7. Coordinate with purchasing for material procurement
8. Create work orders from approved plan
9. Release work orders to production floor
10. Monitor work order progress
11. Adjust plans based on actual performance
```

---

### **Operator Workflow**
```
1. Log in to dashboard
2. View assigned work orders
3. Select work order to execute
4. Release work order (if in Draft status)
5. Start production (status → InProgress)
6. Issue materials from inventory
7. Produce goods according to BOM
8. Record production output:
   - Good units produced
   - Scrap/waste quantity
   - Labor hours worked
   - Machine hours used
9. Complete work order
10. System posts finished goods to inventory
11. Move to next work order
```

---

### **QC Inspector Workflow**
```
1. Log in to dashboard
2. View completed work orders awaiting inspection
3. Select work order to inspect
4. Perform quality checks:
   - Visual inspection (appearance, color)
   - Weight verification
   - Moisture content test
   - Taste test (if applicable)
   - Packaging integrity
5. Record QC result (Pass/Fail/Conditional Pass)
6. Document any defects found
7. Set disposition:
   - Accept (ship to customers)
   - Rework (reprocess)
   - Reject (discard)
8. Submit QC result
9. System updates work order status
10. Move to next inspection
```

---

### **Manager Workflow**
```
1. Log in to dashboard
2. View executive summary (KPIs, metrics)
3. Review production reports:
   - Daily/weekly/monthly output
   - Production efficiency
   - Quality metrics (pass/fail rates)
4. Analyze cost reports:
   - Cost per unit trends
   - Material cost variance
   - Labor efficiency
5. Review inventory status:
   - Stock levels
   - Shortages and alerts
6. Identify improvement opportunities
7. Make strategic decisions:
   - Adjust production targets
   - Optimize resource allocation
   - Improve quality processes
8. Export reports for presentations
```

---

## System Benefits

### **For the Business**:
1. **Improved Efficiency**: Streamlined production processes
2. **Cost Reduction**: Better material planning, reduced waste
3. **Quality Assurance**: Consistent product quality
4. **Real-Time Visibility**: Know what's happening on the floor
5. **Data-Driven Decisions**: Reports and analytics for strategic planning
6. **Compliance**: Complete audit trail for regulatory requirements
7. **Scalability**: Multi-tenant architecture supports growth

### **For Users**:
1. **Role-Specific Interface**: Only see what's relevant to your job
2. **Easy to Use**: Intuitive dashboards and workflows
3. **Mobile-Friendly**: Access from any device
4. **Real-Time Updates**: Always working with current data
5. **Reduced Errors**: Automated calculations and validations
6. **Better Collaboration**: Seamless handoffs between roles

---

## Technology Stack

- **Backend**: ASP.NET Core 10.0 (C#)
- **Frontend**: Razor Views, HTML5, CSS3, JavaScript
- **Database**: MySQL (via Entity Framework Core)
- **Authentication**: ASP.NET Core Identity
- **Security**: reCAPTCHA v3, HTTPS, password hashing
- **Email**: SMTP (Gmail)
- **Architecture**: MVC (Model-View-Controller)
- **Design Pattern**: Repository Pattern, Dependency Injection

---

## Conclusion

**SnackFlow MES** is a complete, production-ready Manufacturing Execution System that manages the entire snack food production lifecycle. With its role-based access control, each user has a tailored experience focused on their specific responsibilities, while the system maintains data integrity and provides comprehensive visibility to management.

The system follows industry best practices for manufacturing execution, including:
- ✅ Master data management (Items, BOMs)
- ✅ Production planning and scheduling
- ✅ Material requirements planning (MRP)
- ✅ Work order execution and tracking
- ✅ Real-time inventory management
- ✅ Quality control integration
- ✅ Cost tracking and analysis
- ✅ Comprehensive reporting and analytics
- ✅ Complete audit trail

This makes **SnackFlow MES** suitable for small to medium-sized snack food manufacturers looking to digitize their production operations and improve efficiency, quality, and profitability.

---

**Document Version**: 1.0  
**Last Updated**: May 7, 2026  
**Author**: SnackFlow MES Development Team
