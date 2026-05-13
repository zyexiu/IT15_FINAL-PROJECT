# SnackFlow MES — System Flow Guide

This document explains how the system works from start to finish, role by role.

---

## Roles Overview

| Role | What They Do |
|------|-------------|
| **Admin** | Sets up the system, manages users, approves requests, oversees everything |
| **Planner** | Plans production, creates work orders, requests materials |
| **Operator** | Executes work orders on the production floor, reports downtime |
| **QC Inspector** | Inspects completed batches and records quality results |
| **Manager** | Monitors production, views reports and analytics (read-only) |

---

## Full System Flow

```
ADMIN SETUP → PLANNER PLANS → OPERATOR EXECUTES → QC INSPECTS → ADMIN MONITORS
```

---

## Step-by-Step Flow

---

### STEP 1 — Admin: System Setup

Before anything can happen, the Admin sets up the foundation.

**1.1 Create Users**
- Go to **User Management → Add User**
- Create accounts for Planner, Operator, QC Inspector, Manager
- Assign each user their role

**1.2 Set Up Inventory (Items Master)**
- Go to **Inventory Master → Add New Item**
- Add all raw materials (e.g., Potato Flour, Salt, Oil)
- Add all finished goods (e.g., BBQ Chips 100g, Sea Salt Chips 50g)
- Add packaging materials (e.g., Foil Bag, Cardboard Box)
- Set reorder points so the system alerts when stock is low

**1.3 Set Up Bill of Materials (BOM)**
- Go to **Bill of Materials → Create BOM**
- For each finished good, define what raw materials are needed and in what quantities
- Example: "BBQ Chips 100g" needs 80g Potato Flour + 15g Seasoning + 5g Oil per batch
- A BOM must exist before a work order can be created for that product

---

### STEP 2 — Planner: Production Planning

Once the system is set up, the Planner schedules what to produce.

**2.1 Create a Production Plan**
- Go to **Production Planning → Create Production Plan**
- Give it a name (e.g., "Week 20 Production Schedule")
- Set the start and end dates
- Set status to **Draft**

**2.2 Add Plan Lines**
- Inside the plan, click **Add Line**
- Select the finished good to produce
- Enter the planned quantity and unit of measure
- Set the scheduled date and production line
- Repeat for each product in the plan

**2.3 Generate Work Orders**
- Once all plan lines are added, click **Generate Work Orders**
- The system automatically creates a Work Order for each pending plan line
- Each work order is created in **Draft** status
- The plan line status changes to **WorkOrderCreated**

**2.4 Request Materials (if needed)**
- If raw materials are running low, go to **Material Request → Create Request**
- Select the item, enter the quantity needed, set priority and reason
- Submit — the Admin gets notified automatically

---

### STEP 3 — Admin: Approve Material Requests

When a Planner submits a material request, the Admin reviews it.

**3.1 Review the Request**
- Go to **Material Request** — new requests show as **Pending**
- Click the request to view details

**3.2 Approve or Reject**
- Click **Approve** (with optional notes) → status becomes **Approved**
- Or click **Reject** (with reason) → status becomes **Rejected**
- The Planner gets a notification either way

**3.3 Fulfill the Request**
- Once materials are physically received, open the approved request
- Click **Mark as Fulfilled** and enter the actual quantity received
- Status becomes **Fulfilled** — the Planner is notified

---

### STEP 4 — Planner/Admin: Release Work Orders

Work orders start as Draft and must be released before operators can work on them.

**4.1 Review the Work Order**
- Go to **Work Orders** — find the Draft work order
- Check the details (product, BOM, quantity, schedule)

**4.2 Release the Work Order**
- Click **Release Work Order** → status changes to **Released**
- The work order is now visible to Operators

---

### STEP 5 — Operator: Execute Production

The Operator works on the production floor and updates the system.

**5.1 View Assigned Work Orders**
- Go to **My Work Orders**
- See all Released and In Progress work orders

**5.2 Start Production**
- Open the work order
- Click **Start Production** → status changes to **In Progress**
- The actual start time is recorded automatically

**5.3 Report Downtime (if something goes wrong)**
- Go to **Report Downtime** (or click from the work order)
- Select the work order, enter the reason (Machine Breakdown, Material Shortage, etc.)
- Enter start time, end time, and description
- Submit — Admin and Manager get notified automatically

**5.4 Complete the Work Order**
- Once production is done, open the work order
- Click **Complete Work Order** → status changes to **Completed**
- The actual end time is recorded automatically

---

### STEP 6 — QC Inspector: Quality Inspection

After a work order is completed, the QC Inspector records the inspection result.

**6.1 Find Completed Work Orders**
- Go to **Quality Inspection**
- See all completed work orders that need inspection

**6.2 Record the Inspection**
- Click **Record QC Inspection** on the work order
- Fill in:
  - **Result**: Pass / Fail / Conditional Pass
  - **Check Type**: Appearance, Weight, Moisture, Taste, Packaging, Overall
  - **Sample Quantity**: how many units were inspected
  - **Defect Quantity**: how many were found defective
  - **Disposition**: Accept / Rework / Reject
  - **Notes**: detailed findings
- Click **Submit Inspection**

**6.3 Notifications**
- If result is **Pass** → Admin gets a pass notification
- If result is **Fail** or **Conditional Pass** → Admin gets an alert notification

**6.4 View Inspection History**
- Go to **Inspection History** to see all past QC results
- Filter by Pass / Fail / Conditional Pass

---

### STEP 7 — Admin: Resolve Downtime Reports

When an Operator reports downtime, the Admin or Manager resolves it.

**7.1 View Open Downtime Reports**
- Go to **Downtime Reports** — open reports show as **Open**
- The dashboard also shows a count of open downtime reports

**7.2 Resolve the Report**
- Open the downtime report
- Click **Resolve Issue**
- Enter the resolution details and set status to **Resolved** or **Escalated**
- The Operator who reported it gets notified

---

### STEP 8 — Admin/Manager: Monitor & Report

Throughout the process, Admin and Manager can monitor everything.

**8.1 Dashboard**
- See KPIs: total work orders, active production, completed orders, low stock alerts
- View production analytics charts
- See open downtime count

**8.2 Reports & Analytics**
- View production summaries
- See recent work orders with status
- Track completion rates and trends

**8.3 Inventory Monitoring**
- Go to **Inventory Master** to check stock levels
- Items below reorder point show a **Low Stock Alert**
- Adjust stock manually if needed (Add / Remove / Set)

---

## Status Flows at a Glance

### Work Order Status
```
Draft → Released → In Progress → Completed
                ↘ Cancelled (at any point before Completed)
```

### Material Request Status
```
Pending → Approved → Fulfilled
        ↘ Rejected
```

### Production Plan Status
```
Draft → Approved → In Progress → Completed
                 ↘ Cancelled
```

### Downtime Report Status
```
Open → Resolved
     ↘ Escalated
```

### QC Result
```
Pass / Fail / Conditional Pass
Disposition: Accept / Rework / Reject
```

---

## Notifications Summary

The system sends automatic notifications when:

| Event | Who Gets Notified |
|-------|------------------|
| Material request submitted | Admin |
| Material request approved/rejected | Planner who submitted it |
| Material request fulfilled | Planner who submitted it |
| Downtime reported | Admin, Manager |
| Downtime resolved | Operator who reported it |
| QC Pass recorded | Admin |
| QC Fail/Conditional Pass recorded | Admin (alert) |

---

## Quick Reference by Role

### Admin
1. Set up users, items, BOMs
2. Approve/reject/fulfill material requests
3. Release work orders (optional — Planner can too)
4. Resolve downtime reports
5. Monitor dashboard and reports

### Planner
1. Create production plans with plan lines
2. Generate work orders from plans
3. Submit material requests when stock is low
4. Monitor work order progress

### Operator
1. View released work orders
2. Start production (In Progress)
3. Report downtime if issues occur
4. Complete work orders when done

### QC Inspector
1. View completed work orders
2. Record inspection results (Pass/Fail/Conditional)
3. View inspection history

### Manager
1. Monitor production overview
2. View downtime reports
3. Check inventory status
4. View reports and analytics
