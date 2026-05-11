# SnackFlow MES - Role Workflow Guide

## Overview
This document describes how all 5 roles in SnackFlow MES work together to manage the complete production lifecycle, from planning to quality inspection.

---

## 🎭 Role Summary

| Role | Focus | Primary Responsibility |
|------|-------|----------------------|
| **Admin** | System Administration | User management, system settings, inventory master data |
| **Planner** | Production Planning | Create production plans, schedule work orders, manage BOMs |
| **Operator** | Floor Execution | Execute work orders, record production data, update status |
| **QC Inspector** | Quality Control | Inspect completed batches, record quality results, approve/reject |
| **Manager** | Business Analytics | View reports, monitor KPIs, analyze performance |

---

## 🔄 Complete Production Workflow

### Phase 1: Planning (Planner)
```
1. Planner creates/updates Bill of Materials (BOM)
   └─> Defines what materials are needed to produce finished goods
   
2. Planner creates Production Plan
   └─> Sets production targets and schedules
   
3. Planner creates Work Order
   ├─> Selects finished good item
   ├─> Selects BOM (defines materials needed)
   ├─> Sets planned quantity
   ├─> Assigns production line
   ├─> Sets scheduled start/end dates
   └─> Status: Draft
   
4. Planner releases Work Order
   └─> Status: Draft → Released
   └─> System automatically RESERVES materials in inventory
```

**Planner Access:**
- ✅ View inventory (read-only, to check material availability)
- ✅ Create/Edit work orders
- ✅ Create/Edit BOMs
- ✅ Create production plans
- ❌ Cannot execute production
- ❌ Cannot modify inventory quantities
- ❌ Cannot manage users

---

### Phase 2: Execution (Operator)

```
5. Operator views assigned Work Orders
   └─> Sees only Released and In Progress orders
   
6. Operator starts production
   ├─> Updates status: Released → In Progress
   ├─> System automatically ISSUES materials (deducts from inventory)
   └─> System records actual start time
   
7. Operator records production data
   ├─> Produced quantity
   ├─> Scrap quantity
   ├─> Labor hours
   ├─> Machine hours
   └─> Shift notes
   
8. Operator completes Work Order
   ├─> Updates status: In Progress → Completed
   ├─> Records final actual quantity
   ├─> System automatically ADDS finished goods to inventory
   └─> System records actual end time
```

**Operator Access:**
- ✅ View assigned work orders
- ✅ Update work order status (Released → In Progress → Completed)
- ✅ Record production logs
- ❌ Cannot create/edit/delete work orders
- ❌ Cannot access planning functions
- ❌ Cannot access inventory management
- ❌ Cannot access system settings

---

### Phase 3: Quality Inspection (QC Inspector)

```
9. QC Inspector reviews completed Work Orders
   └─> Dashboard shows all completed orders pending review
   
10. QC Inspector inspects batch
    ├─> Reviews production data
    ├─> Checks variance (actual vs planned quantity)
    ├─> Performs quality checks:
    │   ├─> Appearance
    │   ├─> Weight
    │   ├─> Moisture
    │   ├─> Taste
    │   ├─> Packaging
    │   └─> Overall quality
    └─> Records inspection results
    
11. QC Inspector records QC Result
    ├─> Result: Pass | Fail | Conditional Pass
    ├─> Sample quantity
    ├─> Defect quantity
    ├─> Notes
    └─> Disposition: Accept | Rework | Reject
    
12. QC Inspector approves/rejects batch
    └─> If rejected, may require rework or scrap
```

**QC Inspector Access:**
- ✅ View all work orders (especially completed ones)
- ✅ View production logs
- ✅ Record QC inspection results
- ✅ Monitor quality metrics
- ❌ Cannot create/edit work orders
- ❌ Cannot execute production
- ❌ Cannot modify inventory
- ❌ Cannot access planning functions

---

### Phase 4: Oversight (Admin & Manager)

#### Admin
```
13. Admin manages system
    ├─> Creates/manages users
    ├─> Assigns roles
    ├─> Manages inventory master data (items, categories)
    ├─> Configures system settings
    └─> Views audit logs
```

**Admin Access:**
- ✅ Full user management
- ✅ Full inventory master data management
- ✅ System settings
- ✅ All reports
- ✅ Can access all pages via direct URL (but menu guides to admin functions)
- ❌ Menu does NOT show production-focused items (not their primary focus)

#### Manager
```
14. Manager monitors business performance
    ├─> Views production reports
    ├─> Analyzes KPIs
    ├─> Monitors efficiency
    └─> Reviews cost analysis
```

**Manager Access:**
- ✅ View all reports
- ✅ View dashboards
- ✅ View production overview (read-only)
- ❌ Cannot create/edit anything
- ❌ Cannot manage users
- ❌ Cannot execute production

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     PRODUCTION LIFECYCLE                         │
└─────────────────────────────────────────────────────────────────┘

ADMIN                    PLANNER                 OPERATOR
  │                         │                        │
  │ Creates Items          │                        │
  │ & BOMs                 │                        │
  ├────────────────────────>│                        │
  │                         │                        │
  │                         │ Creates Work Order     │
  │                         │ (Draft)                │
  │                         │                        │
  │                         │ Releases WO            │
  │                         │ [Reserves Materials]   │
  │                         ├───────────────────────>│
  │                         │                        │
  │                         │                        │ Starts WO
  │                         │                        │ [Issues Materials]
  │                         │                        │
  │                         │                        │ Records Production
  │                         │                        │
  │                         │                        │ Completes WO
  │                         │                        │ [Adds Finished Goods]
  │                         │<───────────────────────┤
  │                         │                        │
  │                         │                        ▼
  │                         │                    QC INSPECTOR
  │                         │                        │
  │                         │                        │ Inspects Batch
  │                         │                        │
  │                         │                        │ Records QC Result
  │                         │                        │ (Pass/Fail)
  │                         │                        │
  │                         │                        ▼
  │                         │                    MANAGER
  │                         │                        │
  │                         │                        │ Views Reports
  │                         │                        │ Analyzes KPIs
  │                         │                        │
  └─────────────────────────┴────────────────────────┘
```

---

## 🔐 Permission Matrix

| Function | Admin | Planner | Operator | QC | Manager |
|----------|-------|---------|----------|----|---------| 
| **Users** |
| Create/Edit/Delete Users | ✅ | ❌ | ❌ | ❌ | ❌ |
| View Users | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Inventory** |
| Create/Edit Items | ✅ | ❌ | ❌ | ❌ | ❌ |
| Archive/Delete Items | ✅ | ❌ | ❌ | ❌ | ❌ |
| Adjust Stock Quantities | ✅ | ❌ | ❌ | ❌ | ❌ |
| View Inventory | ✅ | ✅ | ❌ | ❌ | ✅ |
| **BOM** |
| Create/Edit BOM | ✅ | ✅ | ❌ | ❌ | ❌ |
| View BOM | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Production Planning** |
| Create Production Plans | ✅ | ✅ | ❌ | ❌ | ❌ |
| View Production Plans | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Work Orders** |
| Create/Edit/Delete WO | ✅ | ✅ | ❌ | ❌ | ❌ |
| Update WO Status | ✅ | ✅ | ✅ | ❌ | ❌ |
| Record Production Data | ✅ | ✅ | ✅ | ❌ | ❌ |
| View Work Orders | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Quality Control** |
| Record QC Results | ✅ | ❌ | ❌ | ✅ | ❌ |
| View QC Results | ✅ | ✅ | ❌ | ✅ | ✅ |
| **Reports** |
| View All Reports | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Settings** |
| System Settings | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 🎯 Menu Structure by Role

### Admin Menu (5 items)
```
OVERVIEW
├─ Dashboard

ADMINISTRATION
├─ User Management
├─ Inventory Master
├─ Reports & Analytics
└─ System Settings
```

### Planner Menu (6 items)
```
OVERVIEW
├─ Dashboard

PLANNING
├─ Production Planning
├─ Work Orders
└─ Bill of Materials

RESOURCES
├─ Inventory (view-only)
└─ Reports
```

### Operator Menu (2 items)
```
OVERVIEW
├─ Dashboard

PRODUCTION
└─ My Work Orders
```

### QC Inspector Menu (2 items)
```
OVERVIEW
├─ Dashboard

QUALITY CONTROL
└─ Quality Inspection
```

### Manager Menu (5 items)
```
OVERVIEW
├─ Dashboard

ANALYTICS
└─ Reports & Analytics

MONITORING
├─ Production Overview
├─ Production Planning
└─ Inventory Status
```

---

## 🔄 Inventory Automation

The system automatically manages inventory at each stage:

### 1. Work Order Released (Planner)
```
Status: Draft → Released
Action: RESERVE materials
Effect: QtyReserved increases, materials locked for this WO
```

### 2. Production Started (Operator)
```
Status: Released → In Progress
Action: ISSUE materials
Effect: 
  - QtyOnHand decreases (materials consumed)
  - QtyReserved decreases (reservation fulfilled)
  - Inventory ledger entry created
```

### 3. Production Completed (Operator)
```
Status: In Progress → Completed
Action: ADD finished goods
Effect:
  - Finished good QtyOnHand increases
  - Inventory ledger entry created
```

---

## 📝 Best Practices

### For Planners
1. Always check inventory availability before creating work orders
2. Ensure BOM is complete and accurate before releasing work orders
3. Release work orders only when materials are available
4. Monitor work order progress but don't interfere with floor execution

### For Operators
1. Start work orders only when ready to begin production
2. Record production data accurately and promptly
3. Complete work orders only when production is truly finished
4. Report any issues or variances immediately

### For QC Inspectors
1. Inspect completed batches promptly
2. Record detailed inspection notes
3. Use appropriate disposition (Accept/Rework/Reject)
4. Track quality trends and report recurring issues

### For Admins
1. Keep inventory master data clean and accurate
2. Manage user roles appropriately
3. Monitor system health and audit logs
4. Don't interfere with production workflow unless necessary

### For Managers
1. Use reports to identify trends and opportunities
2. Monitor KPIs regularly
3. Provide feedback to improve processes
4. Don't bypass role boundaries

---

## 🚨 Common Issues & Solutions

### Issue: Materials not available when starting production
**Cause:** Work order released without checking inventory  
**Solution:** Planner should verify inventory before releasing WO

### Issue: Finished goods not appearing in inventory
**Cause:** Work order completed without recording actual quantity  
**Solution:** Operator must enter actual quantity before completing WO

### Issue: Cannot delete work order
**Cause:** Only Draft or Cancelled work orders can be deleted  
**Solution:** Cancel the work order first, then delete

### Issue: QC cannot record inspection results
**Cause:** QC result recording feature may need implementation  
**Solution:** Contact admin to verify QC module is configured

---

## 📞 Support

For questions about:
- **Role access issues** → Contact Admin
- **Production planning** → Contact Planner
- **Floor execution** → Contact Operator
- **Quality concerns** → Contact QC Inspector
- **Reports & analytics** → Contact Manager

---

**Document Version:** 1.0  
**Last Updated:** May 11, 2026  
**System:** SnackFlow MES  
**URL:** https://snackflowmes.onrender.com/
