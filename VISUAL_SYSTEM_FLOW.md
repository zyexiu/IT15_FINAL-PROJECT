# SnackFlow MES - Visual System Flow
## Quick Reference Guide for Presentations

---

## 🎯 System Overview (One-Page Summary)

```
┌─────────────────────────────────────────────────────────────────┐
│                      SNACKFLOW MES                              │
│         Manufacturing Execution System for Snacks               │
└─────────────────────────────────────────────────────────────────┘

┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│    ADMIN     │  │   PLANNER    │  │   OPERATOR   │  │      QC      │  │   MANAGER    │
│              │  │              │  │              │  │              │  │              │
│ • Manage     │  │ • Plan       │  │ • Execute    │  │ • Inspect    │  │ • Monitor    │
│   Users      │  │   Production │  │   Work       │  │   Quality    │  │   Reports    │
│ • Configure  │  │ • Schedule   │  │   Orders     │  │ • Pass/Fail  │  │ • Analyze    │
│   System     │  │ • Run MRP    │  │ • Log Output │  │ • Document   │  │   KPIs       │
│ • View All   │  │ • Create WO  │  │ • Issue      │  │   Defects    │  │ • Make       │
│   Modules    │  │              │  │   Materials  │  │              │  │   Decisions  │
└──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
       │                  │                  │                  │                  │
       └──────────────────┴──────────────────┴──────────────────┴──────────────────┘
                                          │
                        ┌─────────────────┴─────────────────┐
                        │    CENTRALIZED DATABASE           │
                        │  • Items & BOMs                   │
                        │  • Inventory (Real-time)          │
                        │  • Work Orders                    │
                        │  • Production Logs                │
                        │  • QC Results                     │
                        │  • Cost Data                      │
                        │  • Audit Trail                    │
                        └───────────────────────────────────┘
```

---

## 📊 Complete Production Flow (End-to-End)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         PHASE 1: SETUP                                      │
└─────────────────────────────────────────────────────────────────────────────┘

    [Admin Creates Users] → [Admin Sets Up Items] → [Admin Creates BOMs]
            │                        │                        │
            ↓                        ↓                        ↓
    Planner, Operator,      Raw Materials,           Recipe for each
    QC, Manager             Packaging, FG            finished good


┌─────────────────────────────────────────────────────────────────────────────┐
│                      PHASE 2: PLANNING                                      │
└─────────────────────────────────────────────────────────────────────────────┘

    [Planner Creates Production Plan]
            │
            ↓
    [Adds Plan Lines: What to produce, when, how much]
            │
            ↓
    [Runs MRP: Material Requirements Planning]
            │
            ├─→ [Sufficient Stock] → Continue
            │
            └─→ [Shortage Detected] → [Purchase Materials] → Continue
            │
            ↓
    [Creates Work Orders from Plan]
            │
            ↓
    [Releases Work Orders] → Ready for Production


┌─────────────────────────────────────────────────────────────────────────────┐
│                      PHASE 3: EXECUTION                                     │
└─────────────────────────────────────────────────────────────────────────────┘

    [Operator Views Work Orders]
            │
            ↓
    [Starts Work Order] → Status: InProgress
            │
            ↓
    [Issues Materials] → Inventory Deducted
            │
            ↓
    [Produces Goods] → Following BOM recipe
            │
            ↓
    [Records Production Log]
    • Good units: 4850 pcs
    • Scrap: 150 pcs
    • Labor hours: 4.5 hrs
    • Machine hours: 6.2 hrs
            │
            ↓
    [Completes Work Order] → Status: Completed
            │
            ↓
    [Finished Goods Posted to Inventory] → Stock Increased


┌─────────────────────────────────────────────────────────────────────────────┐
│                   PHASE 4: QUALITY CONTROL                                  │
└─────────────────────────────────────────────────────────────────────────────┘

    [QC Inspector Views Completed Work Orders]
            │
            ↓
    [Performs Quality Checks]
    • Appearance
    • Weight
    • Moisture
    • Taste
    • Packaging
            │
            ├─→ [PASS] → [Disposition: Accept] → Ship to Customers
            │
            ├─→ [CONDITIONAL PASS] → [Disposition: Rework] → Reprocess
            │
            └─→ [FAIL] → [Disposition: Reject] → Discard/Downgrade


┌─────────────────────────────────────────────────────────────────────────────┐
│                   PHASE 5: COST & REPORTING                                 │
└─────────────────────────────────────────────────────────────────────────────┘

    [System Calculates Costs Automatically]
            │
            ├─→ Material Cost (actual usage × unit cost)
            ├─→ Labor Cost (hours × labor rate)
            ├─→ Machine Cost (hours × machine rate)
            └─→ Total Cost & Cost per Unit
            │
            ↓
    [Manager Views Reports]
    • Production Summary
    • Cost Analysis
    • Quality Metrics
    • Inventory Status
            │
            ↓
    [Makes Strategic Decisions]
    • Adjust production targets
    • Optimize costs
    • Improve quality
```

---

## 🔄 Inventory Flow (Real-Time Tracking)

```
┌─────────────────────────────────────────────────────────────────┐
│                    INVENTORY MOVEMENTS                          │
└─────────────────────────────────────────────────────────────────┘

    OPENING BALANCE
         │
         ↓
    ┌─────────────────┐
    │  Potato Chips   │
    │   500 kg        │
    └─────────────────┘
         │
         ├─→ [Goods Receipt] +200kg → Balance: 700kg
         │
         ├─→ [WO Issue] -80kg → Balance: 620kg (Material to production)
         │
         ├─→ [Production Output] +4850 pcs FG → Finished goods increased
         │
         ├─→ [Scrap] -150 pcs → Balance decreased (waste)
         │
         └─→ [Adjustment] ±X → Manual correction
         │
         ↓
    CURRENT BALANCE
    (Real-time, always accurate)


┌─────────────────────────────────────────────────────────────────┐
│              INVENTORY LEDGER (Audit Trail)                     │
└─────────────────────────────────────────────────────────────────┘

Date       | Type            | Qty    | Balance | Reference
-----------|-----------------|--------|---------|------------------
May 1      | Opening         | +500kg | 500kg   | Initial stock
May 2      | WO Issue        | -80kg  | 420kg   | WO-2026-0001
May 3      | Goods Receipt   | +200kg | 620kg   | PO-12345
May 4      | WO Issue        | -80kg  | 540kg   | WO-2026-0002
May 5      | Production Out  | +4850  | 5390    | WO-2026-0001 (FG)
May 5      | Scrap           | -150   | 5240    | WO-2026-0001

Every transaction is logged → Complete traceability
```

---

## 🏭 Work Order Lifecycle (Detailed)

```
┌─────────────────────────────────────────────────────────────────┐
│                    WORK ORDER STATES                            │
└─────────────────────────────────────────────────────────────────┘

    ┌──────────┐
    │  DRAFT   │ ← Planner creates WO
    └──────────┘
         │
         │ [Planner Reviews & Releases]
         ↓
    ┌──────────┐
    │ RELEASED │ ← Materials reserved, ready for production
    └──────────┘
         │
         │ [Operator Starts Production]
         ↓
    ┌────────────┐
    │ INPROGRESS │ ← Materials issued, production ongoing
    └────────────┘
         │
         │ [Operator Completes]
         ↓
    ┌───────────┐
    │ COMPLETED │ ← Output recorded, FG posted to inventory
    └───────────┘
         │
         │ [QC Inspects]
         ↓
    ┌──────────────┐
    │ QC APPROVED  │ ← Ready to ship
    └──────────────┘


Alternative Paths:
    DRAFT → [Cancel] → CANCELLED
    RELEASED → [Cancel] → CANCELLED (materials unreserved)
    INPROGRESS → [Cancel] → CANCELLED (partial output recorded)
```

---

## 📈 MRP Process (Material Requirements Planning)

```
┌─────────────────────────────────────────────────────────────────┐
│              MRP CALCULATION FLOW                               │
└─────────────────────────────────────────────────────────────────┘

    [Production Plan Created]
    Plan: Produce 5000 pcs Cheese Chips
         │
         ↓
    [System Explodes BOM]
    BOM for Cheese Chips (per 1000 pcs):
    • Potato Chips: 80kg
    • Cheese Powder: 15kg
    • Plastic Bags: 1000 pcs
         │
         ↓
    [Calculate Gross Requirement]
    For 5000 pcs:
    • Potato Chips: 400kg
    • Cheese Powder: 75kg
    • Plastic Bags: 5000 pcs
         │
         ↓
    [Check Current Inventory]
    Stock on Hand:
    • Potato Chips: 540kg ✓
    • Cheese Powder: 50kg ✗
    • Plastic Bags: 6000 pcs ✓
         │
         ↓
    [Calculate Net Requirement]
    Shortages:
    • Potato Chips: 0kg (sufficient)
    • Cheese Powder: 25kg (shortage!)
    • Plastic Bags: 0 pcs (sufficient)
         │
         ↓
    [Generate MRP Report]
    Action Required:
    ⚠️ Purchase 25kg Cheese Powder (or more for safety stock)
         │
         ↓
    [Planner Reviews & Takes Action]
    • Coordinate with purchasing
    • Adjust production schedule if needed
    • Create work orders when materials available
```

---

## 💰 Cost Calculation (Automatic)

```
┌─────────────────────────────────────────────────────────────────┐
│              COST BREAKDOWN PER WORK ORDER                      │
└─────────────────────────────────────────────────────────────────┘

Work Order: WO-2026-0001
Item: Cheese Potato Chips 100g
Actual Output: 4850 pcs

┌─────────────────────────────────────────────────────────────┐
│ MATERIAL COST                                               │
├─────────────────────────────────────────────────────────────┤
│ Potato Chips:    395kg × ₱50/kg    = ₱19,750              │
│ Cheese Powder:   73kg × ₱200/kg    = ₱14,600              │
│ Plastic Bags:    5000 pcs × ₱0.50  = ₱2,500               │
│                                                             │
│ TOTAL MATERIAL COST:                  ₱36,850              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ LABOR COST                                                  │
├─────────────────────────────────────────────────────────────┤
│ Labor Hours:     4.5 hrs × ₱150/hr = ₱675                 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ MACHINE COST                                                │
├─────────────────────────────────────────────────────────────┤
│ Machine Hours:   6.2 hrs × ₱100/hr = ₱620                 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ TOTAL COST SUMMARY                                          │
├─────────────────────────────────────────────────────────────┤
│ Material Cost:                        ₱36,850              │
│ Labor Cost:                           ₱675                 │
│ Machine Cost:                         ₱620                 │
│ ─────────────────────────────────────────────              │
│ TOTAL COST:                           ₱38,145              │
│                                                             │
│ Cost per Unit:  ₱38,145 ÷ 4850 pcs = ₱7.87/pc            │
└─────────────────────────────────────────────────────────────┘

This data helps:
✓ Determine selling price
✓ Calculate profit margins
✓ Identify cost-saving opportunities
✓ Compare actual vs. standard costs
```

---

## 🎨 Dashboard Views by Role

### **Admin Dashboard**
```
┌─────────────────────────────────────────────────────────────┐
│  ADMIN DASHBOARD                                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  Users   │  │  Active  │  │ Inventory│  │  System  │  │
│  │    12    │  │   WOs    │  │  Alerts  │  │  Health  │  │
│  │          │  │    8     │  │    3     │  │   ✓ OK   │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                             │
│  Quick Actions:                                             │
│  • Manage Users                                             │
│  • View All Work Orders                                     │
│  • System Settings                                          │
│  • Generate Reports                                         │
│                                                             │
│  Recent Activities:                                         │
│  • User "john.operator" logged in                          │
│  • WO-2026-0001 completed                                  │
│  • New user "jane.qc" created                              │
└─────────────────────────────────────────────────────────────┘
```

### **Planner Dashboard**
```
┌─────────────────────────────────────────────────────────────┐
│  PLANNER DASHBOARD                                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  Active  │  │  Pending │  │ Material │  │  Plans   │  │
│  │   Plans  │  │   WOs    │  │ Shortage │  │This Week │  │
│  │    3     │  │    5     │  │    2     │  │    4     │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                             │
│  Production Calendar:                                       │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ Mon  │ Tue  │ Wed  │ Thu  │ Fri  │ Sat  │ Sun      │  │
│  │ WO-1 │ WO-2 │ WO-3 │ WO-4 │ WO-5 │ OFF  │ OFF      │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Quick Actions:                                             │
│  • Create Production Plan                                   │
│  • Run MRP                                                  │
│  • Create Work Order                                        │
│  • View Inventory                                           │
└─────────────────────────────────────────────────────────────┘
```

### **Operator Dashboard**
```
┌─────────────────────────────────────────────────────────────┐
│  OPERATOR DASHBOARD                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  My Work Orders Today:                                      │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ WO-2026-0001 | Cheese Chips | 5000 pcs | InProgress │  │
│  │ [Continue] [Complete]                                │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ WO-2026-0002 | BBQ Chips | 3000 pcs | Released      │  │
│  │ [Start Production]                                   │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Production Stats Today:                                    │
│  • Units Produced: 4850 pcs                                │
│  • Scrap: 150 pcs (3.1%)                                   │
│  • Hours Worked: 4.5 hrs                                   │
│                                                             │
│  Quick Actions:                                             │
│  • Log Production                                           │
│  • Issue Materials                                          │
│  • Report Issue                                             │
└─────────────────────────────────────────────────────────────┘
```

### **QC Dashboard**
```
┌─────────────────────────────────────────────────────────────┐
│  QC INSPECTOR DASHBOARD                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Pending  │  │  Pass    │  │   Fail   │  │  Pass    │  │
│  │Inspection│  │  Rate    │  │   Rate   │  │  Today   │  │
│  │    3     │  │  96.5%   │  │   3.5%   │  │    8     │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                             │
│  Inspection Queue:                                          │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ WO-2026-0001 | Cheese Chips | 4850 pcs | Completed  │  │
│  │ [Inspect Now]                                        │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ WO-2026-0003 | Sour Cream | 2000 pcs | Completed    │  │
│  │ [Inspect Now]                                        │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Quick Actions:                                             │
│  • View QC History                                          │
│  • Generate QC Report                                       │
│  • Document Defect                                          │
└─────────────────────────────────────────────────────────────┘
```

### **Manager Dashboard**
```
┌─────────────────────────────────────────────────────────────┐
│  MANAGER DASHBOARD                                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Production Overview (This Month):                          │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  Total   │  │   Avg    │  │   QC     │  │   Cost   │  │
│  │  Output  │  │Efficiency│  │Pass Rate │  │ per Unit │  │
│  │ 125,000  │  │  94.2%   │  │  96.5%   │  │  ₱7.85   │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                             │
│  Production Trend:                                          │
│  ┌─────────────────────────────────────────────────────┐  │
│  │     ▁▂▃▅▆█▇▆▅▄▃▂▁                                   │  │
│  │  Week 1  Week 2  Week 3  Week 4                     │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Cost Breakdown:                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ Material: 92% ████████████████████                  │  │
│  │ Labor:     5% ██                                     │  │
│  │ Machine:   3% █                                      │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Quick Actions:                                             │
│  • View Detailed Reports                                    │
│  • Export to Excel                                          │
│  • Production Analysis                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Security & Multi-Tenancy

```
┌─────────────────────────────────────────────────────────────┐
│              MULTI-TENANT ARCHITECTURE                      │
└─────────────────────────────────────────────────────────────┘

    ┌──────────────────┐         ┌──────────────────┐
    │   TENANT 1       │         │   TENANT 2       │
    │   (Admin A)      │         │   (Admin B)      │
    ├──────────────────┤         ├──────────────────┤
    │ • Users (5)      │         │ • Users (8)      │
    │ • Items (50)     │         │ • Items (75)     │
    │ • Work Orders    │         │ • Work Orders    │
    │ • Inventory      │         │ • Inventory      │
    │ • Reports        │         │ • Reports        │
    └──────────────────┘         └──────────────────┘
            │                            │
            └────────────┬───────────────┘
                         │
                ┌────────▼────────┐
                │   DATABASE      │
                │  (Shared, but   │
                │   isolated by   │
                │   TenantId)     │
                └─────────────────┘

Each Admin has:
✓ Isolated workspace
✓ Own users and data
✓ Fresh start (zero data on signup)
✓ Complete data privacy
✗ Cannot see other tenants' data
```

---

## 📱 Key Features Summary

```
┌─────────────────────────────────────────────────────────────┐
│                    SYSTEM FEATURES                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ✓ Role-Based Access Control (5 roles)                     │
│  ✓ Multi-Tenant Architecture                               │
│  ✓ Real-Time Inventory Tracking                            │
│  ✓ Automatic Cost Calculation                              │
│  ✓ Material Requirements Planning (MRP)                    │
│  ✓ Quality Control Integration                             │
│  ✓ Complete Audit Trail                                    │
│  ✓ Comprehensive Reporting                                 │
│  ✓ Email Notifications                                     │
│  ✓ reCAPTCHA Security                                      │
│  ✓ Password Encryption                                     │
│  ✓ Mobile-Friendly Interface                               │
│  ✓ Export to PDF/Excel                                     │
│  ✓ Dark Mode Support                                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

**Use this document for presentations, demonstrations, and explaining the system flow to your professor!**
