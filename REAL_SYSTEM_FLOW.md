# SnackFlow MES - REAL System Flow (Based on What You Actually Have)
## The ACTUAL Steps - No BS, Just What Works

---

## 🎯 YOUR ACTUAL SYSTEM HAS:

✅ Dashboard  
✅ Work Orders  
✅ Production Planning  
✅ Bill of Materials (BOM)  
✅ Inventory  
✅ Reports  
✅ Users  
✅ Settings  

❌ NO MRP (Material Requirements Planning)  
❌ NO QC Module (Quality Control)  
❌ NO Production Logs  

---

## 📝 THE REAL FLOW (What You Can Actually Do)

```
SETUP (Once)
    ↓
CREATE ITEMS (Inventory)
    ↓
CREATE BOMs (Recipes)
    ↓
CREATE PRODUCTION PLAN
    ↓
CREATE WORK ORDERS
    ↓
EXECUTE WORK ORDERS
    ↓
VIEW REPORTS
```

---

## STEP-BY-STEP (THE REAL WAY)

### **STEP 1: Create Users** (Admin Only)
**Where**: Users menu

**What to do**:
1. Click "Users" in sidebar
2. Click "Add New User"
3. Fill in: Name, Email, Username, Password
4. Select Role: Planner, Operator, QC, Manager (NOT Admin)
5. Click "Create User"

**Why**: Need people to use the system

**Example**:
- John - Planner
- Jane - Operator
- Bob - Manager

---

### **STEP 2: Create Items** (Inventory)
**Where**: Inventory menu

**What to do**:
1. Click "Inventory" in sidebar
2. Click "Add New Item"
3. Fill in:
   - **Item Code**: RM-001
   - **Item Name**: Potato Chips (Raw)
   - **Item Type**: Select "RawMaterial" or "FinishedGood" or "Packaging"
   - **Unit of Measure**: kg, pcs, box, etc.
   - **Unit Cost**: ₱50
   - **Reorder Point**: 100 (minimum stock level)
4. Click "Create"

**Create These Types**:
1. **Raw Materials** (ingredients you buy)
   - Potato Chips (Raw) - RawMaterial
   - Cheese Powder - RawMaterial
   - Salt - RawMaterial

2. **Packaging** (bags, boxes)
   - Plastic Bag 100g - Packaging
   - Carton Box - Packaging

3. **Finished Goods** (products you sell)
   - Cheese Potato Chips 100g - FinishedGood
   - BBQ Potato Chips 100g - FinishedGood

**Why**: System needs to know what materials exist

**Important**: Create ALL items BEFORE creating BOMs!

---

### **STEP 3: Add Opening Stock** (Inventory)
**Where**: Inventory menu → Click on an item → Edit

**What to do**:
1. Go to Inventory
2. Click on an item (e.g., Potato Chips)
3. Click "Edit"
4. Set "Quantity on Hand": 500 (how much you have now)
5. Click "Save"

**Do this for ALL items**

**Why**: System needs to know current stock levels

**Example**:
```
Potato Chips (Raw): 500 kg
Cheese Powder: 100 kg
Plastic Bags: 10,000 pcs
Cheese Chips (Finished): 0 pcs (nothing produced yet)
```

---

### **STEP 4: Create BOM** (Bill of Materials = Recipe)
**Where**: Bill of Materials menu

**What to do**:
1. Click "Bill of Materials" in sidebar
2. Click "Add New BOM"
3. Fill in:
   - **Item**: Select a **FINISHED GOOD** (e.g., Cheese Chips 100g)
   - **Version**: v1
   - **Batch Output Qty**: 1000 (how many you make per batch)
   - **Batch Output UOM**: pcs
   - **Est Machine Hours**: 6
   - **Est Labor Hours**: 4
   - **Notes**: Optional
4. Click "Create"

5. **Now Add Materials** (ingredients):
   - Click "Add Material" button
   - Select Material: Potato Chips (Raw)
   - Quantity: 80
   - Unit: kg
   - Click "Add Material"

6. **Repeat for all ingredients**:
   - Cheese Powder: 15 kg
   - Salt: 2 kg
   - Plastic Bag: 1000 pcs

**Why**: This is the recipe. Tells system what materials are needed to make the product.

**Important**: 
- You MUST select a **Finished Good** item
- You CANNOT create Work Order without BOM!

**Example BOM**:
```
Product: Cheese Chips 100g
Batch: 1000 pcs

Materials Needed:
- Potato Chips (Raw): 80 kg
- Cheese Powder: 15 kg
- Salt: 2 kg
- Plastic Bag 100g: 1000 pcs

Labor: 4 hours
Machine: 6 hours
```

---

### **STEP 5: Create Production Plan** (Optional but Recommended)
**Where**: Production Planning menu

**What to do**:
1. Click "Production Planning" in sidebar
2. Click "Create New Plan"
3. Fill in:
   - **Plan Name**: May 2026 Production
   - **Plan Date From**: May 1, 2026
   - **Plan Date To**: May 31, 2026
   - **Status**: Draft
   - **Notes**: Optional
4. Click "Create"

5. **Add Plan Lines** (what to produce):
   - Click "Add Plan Line"
   - **Item**: Cheese Chips 100g
   - **Planned Qty**: 5000 pcs
   - **Scheduled Date**: May 5, 2026
   - **Production Line**: Line 1
   - Click "Add"

**Why**: This is your production schedule. Like a to-do list for the month.

**Note**: This is optional. You can create Work Orders directly without a plan.

---

### **STEP 6: Create Work Order**
**Where**: Work Orders menu

**What to do**:
1. Click "Work Orders" in sidebar
2. Click "Create New Work Order"
3. Fill in:
   - **WO Number**: Auto-generated (WO-2026-0001)
   - **Item**: Select **FINISHED GOOD** (Cheese Chips 100g)
   - **BOM**: Select the BOM you created (v1)
   - **Planned Qty**: 5000 pcs
   - **Scheduled Start**: May 5, 2026 08:00
   - **Scheduled End**: May 5, 2026 16:00
   - **Production Line**: Line 1
   - **Status**: Draft
4. Click "Create"

**Why**: This is the actual production job. Operator will execute this.

**Important**:
- You MUST select a **Finished Good**
- You MUST select a **BOM** for that finished good
- If no BOM exists, you CANNOT create Work Order!

**What Happens**:
- System creates work order
- Status = Draft (not started yet)
- Materials NOT yet reserved

---

### **STEP 7: Release Work Order** (Change Status)
**Where**: Work Orders menu → Click on work order → Edit

**What to do**:
1. Open the work order you created
2. Click "Edit"
3. Change **Status** from "Draft" to "Released"
4. Click "Save"

**Why**: This marks the work order as ready for production.

**What Happens**:
- Status changes: Draft → Released
- Work order is now visible to Operators
- Ready to start production

---

### **STEP 8: Start Production** (Operator)
**Where**: Work Orders menu → Click on work order → Edit

**What to do**:
1. Operator logs in
2. Goes to "My Work Orders" or "Work Orders"
3. Opens the released work order
4. Click "Edit"
5. Change **Status** from "Released" to "InProgress"
6. Set **Actual Start** date/time
7. Click "Save"

**Why**: Marks that production has started.

**What Happens**:
- Status changes: Released → InProgress
- Production timer starts
- Operator can now produce

---

### **STEP 9: Record Production** (Operator)
**Where**: Work Orders menu → Click on work order → Edit

**What to do**:
1. After producing, open the work order
2. Click "Edit"
3. Fill in:
   - **Actual Qty**: 4850 pcs (how many you actually made)
   - **Actual End**: May 5, 2026 16:00
4. Click "Save"

**Why**: Records actual output vs. planned.

**Note**: In your system, you record this in the work order itself, not in a separate production log.

---

### **STEP 10: Complete Work Order**
**Where**: Work Orders menu → Click on work order → Edit

**What to do**:
1. Open the work order
2. Click "Edit"
3. Change **Status** from "InProgress" to "Completed"
4. Make sure **Actual Qty** is filled in
5. Click "Save"

**Why**: Marks the job as finished.

**What Happens**:
- Status changes: InProgress → Completed
- Work order is done
- Finished goods should be added to inventory (manually update inventory)

---

### **STEP 11: Update Inventory** (After Completion)
**Where**: Inventory menu

**What to do**:
1. Go to Inventory
2. **Deduct Raw Materials** (what you used):
   - Find "Potato Chips (Raw)"
   - Click "Edit"
   - Reduce Qty: 500kg - 80kg = 420kg
   - Click "Save"
   - Repeat for all materials used

3. **Add Finished Goods** (what you produced):
   - Find "Cheese Chips 100g"
   - Click "Edit"
   - Increase Qty: 0pcs + 4850pcs = 4850pcs
   - Click "Save"

**Why**: Keep inventory accurate.

**Note**: In your current system, this is MANUAL. You need to update inventory yourself after completing work orders.

---

### **STEP 12: View Reports**
**Where**: Reports menu

**What to do**:
1. Click "Reports" in sidebar
2. Select report type
3. Select date range
4. Click "Generate"
5. View results
6. Export if needed

**Why**: See production summary, costs, inventory status.

---

## ✅ THAT'S THE COMPLETE FLOW!

---

## 🔄 THE CYCLE

```
1. Create Items (Inventory) ← Do once
2. Create BOMs ← Do once per product
3. Create Production Plan ← Do weekly/monthly
4. Create Work Order ← Do for each production job
5. Release Work Order ← Mark as ready
6. Start Production ← Operator begins
7. Record Production ← Operator logs output
8. Complete Work Order ← Mark as done
9. Update Inventory ← Manually adjust stock
10. View Reports ← Check results

Then repeat from Step 3 for next production cycle
```

---

## ⚠️ IMPORTANT RULES

### **You CANNOT create Work Order without:**
1. ✅ A **Finished Good** item in Inventory
2. ✅ A **BOM** for that finished good

### **You CANNOT create BOM without:**
1. ✅ A **Finished Good** item in Inventory
2. ✅ **Raw Material** items in Inventory (for ingredients)

### **Work Order Status Flow:**
```
Draft → Released → InProgress → Completed
```
You CANNOT skip statuses!

---

## 🚫 WHAT YOUR SYSTEM DOES NOT HAVE

1. **NO Automatic MRP** - You need to manually check if you have enough materials
2. **NO Automatic Inventory Updates** - You need to manually update inventory after completing work orders
3. **NO QC Module** - No quality control inspection feature
4. **NO Production Logs** - Production data is recorded in the work order itself
5. **NO Material Reservation** - Materials are not automatically reserved when releasing work orders

---

## 💡 SIMPLIFIED CHECKLIST

### **Before Creating Work Order:**
- [ ] Finished good exists in Inventory?
- [ ] BOM exists for that finished good?
- [ ] Raw materials exist in Inventory?
- [ ] Enough stock in Inventory? (check manually)

### **Before Starting Production:**
- [ ] Work order created?
- [ ] Work order status = Released?
- [ ] Materials available? (check manually)

### **After Completing Work Order:**
- [ ] Actual Qty recorded?
- [ ] Status = Completed?
- [ ] Inventory updated manually? (deduct materials, add finished goods)

---

## 🎯 DEMO SCRIPT FOR PROFESSOR

**"Sir, let me show you the flow of my system:"**

1. **"First, I create items in Inventory"**
   - Show creating: Potato Chips (Raw) - RawMaterial
   - Show creating: Cheese Chips 100g - FinishedGood
   - "Sir, I need to create all materials first"

2. **"Second, I add opening stock"**
   - Edit Potato Chips: Set Qty = 500kg
   - "Sir, this tells the system how much stock I have"

3. **"Third, I create BOM - the recipe"**
   - Create BOM for Cheese Chips
   - Select Finished Good: Cheese Chips
   - Add materials: Potato Chips 80kg, Cheese Powder 15kg
   - "Sir, this is the recipe. It tells what materials are needed"

4. **"Fourth, I create Production Plan (optional)"**
   - Create plan: May 2026
   - Add plan line: 5000 Cheese Chips on May 5
   - "Sir, this is my production schedule"

5. **"Fifth, I create Work Order"**
   - Create WO: Cheese Chips, 5000 pcs
   - Select BOM: v1
   - "Sir, I MUST select a finished good and BOM"
   - Try without BOM → Show error (if validation exists)
   - With BOM → Success

6. **"Sixth, I release Work Order"**
   - Edit WO: Change status to Released
   - "Sir, now it's ready for production"

7. **"Seventh, Operator starts production"**
   - Edit WO: Change status to InProgress
   - Set Actual Start time
   - "Sir, production has started"

8. **"Eighth, Operator records output"**
   - Edit WO: Set Actual Qty = 4850 pcs
   - Set Actual End time
   - "Sir, this records how much was actually produced"

9. **"Ninth, Operator completes Work Order"**
   - Edit WO: Change status to Completed
   - "Sir, the job is done"

10. **"Tenth, I update Inventory manually"**
    - Edit Potato Chips: Reduce from 500kg to 420kg
    - Edit Cheese Chips: Increase from 0 to 4850 pcs
    - "Sir, I manually update stock after production"

11. **"Finally, Manager views Reports"**
    - Go to Reports
    - Generate production report
    - "Sir, this shows all production data"

---

## 🔧 WHAT NEEDS TO BE FIXED/ADDED (Future Improvements)

1. **Automatic Inventory Updates** - When completing work order, auto-deduct materials and add finished goods
2. **Material Reservation** - When releasing work order, auto-reserve materials
3. **MRP Feature** - Auto-calculate material requirements from production plan
4. **QC Module** - Add quality control inspection
5. **Production Logs** - Separate module for logging production details
6. **Validation** - Prevent creating work order without BOM
7. **Stock Checking** - Warn if not enough materials before creating work order

---

**THIS IS YOUR REAL SYSTEM FLOW - NO FEATURES THAT DON'T EXIST!**
