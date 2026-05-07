# SnackFlow MES - Step-by-Step Flow (Super Simple)
## The EXACT Order to Use Your System

---

## 🎯 The Correct Flow (Follow This Order!)

```
SETUP (Do Once)
    ↓
PLANNING (Every Week/Month)
    ↓
EXECUTION (Daily Production)
    ↓
QUALITY CHECK (After Production)
    ↓
REPORTS (Anytime)
```

---

## 📝 PHASE 1: SETUP (Admin Does This ONCE at the Beginning)

### **Step 1: Create Users**
**Where**: Users menu → Add New User

**What to do**:
1. Click "Add New User"
2. Fill in: Name, Email, Username, Password
3. Select Role: Planner, Operator, QC, or Manager
4. Click "Create"

**Why**: You need people to use the system. Each person gets a role.

**Example**:
- John Doe - Planner
- Jane Smith - Operator
- Bob Lee - QC Inspector
- Mary Johnson - Manager

---

### **Step 2: Create Items (Materials)**
**Where**: Items menu → Add New Item

**What to do**:
1. Click "Add New Item"
2. Fill in:
   - Item Code: RM-001
   - Item Name: Potato Chips (Raw)
   - Item Type: **RawMaterial** ← Important!
   - Unit: kg
   - Unit Cost: ₱50
3. Click "Create"

**Repeat for**:
- All raw materials (potato chips, cheese powder, salt)
- All packaging (plastic bags, boxes)
- All finished goods (Cheese Chips 100g, BBQ Chips 100g)

**Why**: You need to tell the system what materials exist before you can use them.

**Important**: 
- Raw materials = ingredients you buy
- Packaging = bags, boxes
- Finished goods = final products you sell

**Example Items to Create**:
```
RM-001: Potato Chips (Raw) - RawMaterial - kg - ₱50
RM-002: Cheese Powder - RawMaterial - kg - ₱200
RM-003: Salt - RawMaterial - kg - ₱30
PK-001: Plastic Bag 100g - Packaging - pcs - ₱0.50
PK-002: Carton Box - Packaging - box - ₱20
FG-001: Cheese Potato Chips 100g - FinishedGood - pcs - ₱10
FG-002: BBQ Potato Chips 100g - FinishedGood - pcs - ₱10
```

---

### **Step 3: Create BOMs (Recipes)**
**Where**: BOM menu → Add New BOM

**What to do**:
1. Click "Add New BOM"
2. Select **Finished Good** (e.g., Cheese Chips 100g)
3. Fill in:
   - Version: v1
   - Batch Output: 1000 pcs
   - Labor Hours: 4 hours
   - Machine Hours: 6 hours
4. Click "Create"
5. Now **Add Materials** (ingredients):
   - Click "Add Material"
   - Select: Potato Chips (Raw)
   - Quantity: 80 kg
   - Click "Add"
6. Repeat for all ingredients

**Why**: This is the recipe. It tells the system: "To make 1000 Cheese Chips, you need 80kg potato chips, 15kg cheese powder, etc."

**Important**: 
- You MUST create a BOM for EACH finished good
- Without BOM, you CANNOT create Work Orders!

**Example BOM**:
```
Product: Cheese Chips 100g (FG-001)
Batch: 1000 pcs

Ingredients:
- Potato Chips (Raw): 80 kg
- Cheese Powder: 15 kg
- Salt: 2 kg
- Plastic Bag 100g: 1000 pcs
- Carton Box: 20 boxes

Labor: 4 hours
Machine: 6 hours
```

---

### **Step 4: Add Opening Inventory**
**Where**: Inventory menu → Adjust Stock

**What to do**:
1. Go to Inventory
2. For each item, click "Adjust"
3. Enter opening balance (how much you have now)
4. Type: "Opening Balance"
5. Click "Save"

**Why**: System needs to know how much stock you have before production starts.

**Example**:
```
Potato Chips (Raw): 500 kg
Cheese Powder: 100 kg
Salt: 50 kg
Plastic Bags: 10,000 pcs
Carton Boxes: 500 boxes
```

---

## ✅ SETUP COMPLETE! Now you can start production.

---

## 📅 PHASE 2: PLANNING (Planner Does This Weekly/Monthly)

### **Step 5: Create Production Plan**
**Where**: Production Plans menu → Add New Plan

**What to do**:
1. Click "Add New Plan"
2. Fill in:
   - Plan Name: "May 2026 Production"
   - Date From: May 1, 2026
   - Date To: May 31, 2026
3. Click "Create"

**Why**: You need a schedule. This is like a to-do list for the month.

---

### **Step 6: Add Plan Lines (What to Produce)**
**Where**: Inside the plan you just created

**What to do**:
1. Click "Add Plan Line"
2. Select: Cheese Chips 100g
3. Quantity: 5000 pcs
4. Date: May 5, 2026
5. Production Line: Line 1
6. Click "Add"

**Repeat for all products you want to make this month**

**Why**: This tells the system: "On May 5, make 5000 Cheese Chips on Line 1"

**Example Plan Lines**:
```
May 5: Cheese Chips - 5000 pcs - Line 1
May 7: BBQ Chips - 3000 pcs - Line 1
May 10: Cheese Chips - 5000 pcs - Line 2
```

---

### **Step 7: Run MRP (Check Materials)**
**Where**: Inside the plan → Click "Run MRP"

**What to do**:
1. Click "Run MRP" button
2. System calculates material needs
3. Review the results

**What it shows**:
- ✅ Green = You have enough
- ❌ Red = Shortage! Need to buy more

**Why**: Prevents starting production and realizing you're missing ingredients.

**Example MRP Result**:
```
For 5000 Cheese Chips:
- Potato Chips: Need 400kg, Have 500kg ✅ OK
- Cheese Powder: Need 75kg, Have 50kg ❌ SHORT 25kg!
- Plastic Bags: Need 5000pcs, Have 10000pcs ✅ OK

Action: Buy 25kg Cheese Powder before production!
```

---

### **Step 8: Create Work Orders**
**Where**: Work Orders menu → Add New Work Order

**What to do**:
1. Click "Add New Work Order"
2. Select:
   - Item: Cheese Chips 100g ← Must be a Finished Good!
   - BOM: v1 ← Must have a BOM!
   - Quantity: 5000 pcs
   - Scheduled Date: May 5, 2026
   - Production Line: Line 1
3. Click "Create"

**Why**: This is the actual production job. Operator will execute this.

**Important**:
- You CANNOT create Work Order without:
  1. A Finished Good (what to make)
  2. A BOM for that finished good (how to make it)
- If you try, system will show error!

---

### **Step 9: Release Work Order**
**Where**: Work Orders menu → Click on the work order → Click "Release"

**What to do**:
1. Open the work order
2. Click "Release" button
3. Status changes: Draft → Released

**What happens**:
- System reserves materials from inventory
- Work order is now ready for production
- Operator can now see it

**Why**: This locks the materials so they're not used for other jobs.

---

## ✅ PLANNING COMPLETE! Now Operator can start production.

---

## 🏭 PHASE 3: EXECUTION (Operator Does This Daily)

### **Step 10: Start Work Order**
**Where**: Work Orders menu → Select work order → Click "Start"

**What to do**:
1. Operator logs in
2. Goes to Work Orders
3. Finds their assigned work order
4. Clicks "Start Production"
5. Status changes: Released → InProgress

**What happens**:
- Materials are issued from inventory (stock decreases)
- Production timer starts
- Operator can now produce

**Why**: Tracks when production actually starts.

---

### **Step 11: Produce Goods**
**What to do**:
- Operator follows the BOM recipe
- Makes the products
- Records any issues

**This is the actual physical work** - making the chips!

---

### **Step 12: Log Production**
**Where**: Inside the work order → Click "Add Production Log"

**What to do**:
1. Click "Add Production Log"
2. Fill in:
   - Date: May 5, 2026
   - Shift: Morning
   - Produced Qty: 4850 pcs (good units)
   - Scrap Qty: 150 pcs (waste)
   - Labor Hours: 4.5 hours
   - Machine Hours: 6.2 hours
3. Click "Save"

**Why**: Records actual output. Maybe you planned 5000 but only made 4850 - this tracks it.

**Important**: 
- Good units = products that are OK
- Scrap = waste, damaged, rejected

---

### **Step 13: Complete Work Order**
**Where**: Inside the work order → Click "Complete"

**What to do**:
1. Click "Complete" button
2. Status changes: InProgress → Completed

**What happens**:
- Finished goods are added to inventory (stock increases)
- Work order is done
- Ready for QC inspection
- System calculates costs automatically

**Why**: Marks the job as finished.

---

## ✅ PRODUCTION COMPLETE! Now QC can inspect.

---

## ✔️ PHASE 4: QUALITY CHECK (QC Does This After Production)

### **Step 14: Inspect Work Order**
**Where**: QC menu → Select completed work order → Click "Inspect"

**What to do**:
1. QC Inspector logs in
2. Goes to QC menu
3. Finds completed work order
4. Clicks "Inspect"
5. Performs quality checks:
   - Appearance: OK?
   - Weight: Correct?
   - Taste: Good?
   - Packaging: Sealed properly?
6. Records result:
   - **Pass** = Good quality, ship it
   - **Fail** = Bad quality, reject it
   - **Conditional Pass** = Minor issues, rework
7. Sets disposition:
   - **Accept** = Ship to customers
   - **Rework** = Fix and repackage
   - **Reject** = Throw away
8. Click "Submit"

**Why**: Ensures products meet quality standards before shipping to customers.

---

## ✅ QUALITY CHECK COMPLETE! Products ready to ship.

---

## 📊 PHASE 5: REPORTS (Manager Does This Anytime)

### **Step 15: View Reports**
**Where**: Reports menu

**What to do**:
1. Manager logs in
2. Goes to Reports
3. Selects report type:
   - Production Summary
   - Cost Analysis
   - Inventory Status
   - QC Report
4. Selects date range
5. Clicks "Generate"
6. Reviews data
7. Exports to Excel if needed

**Why**: See the big picture. How much produced? How much profit? Any quality issues?

---

## 🔄 THE CYCLE REPEATS

After completing one production cycle, you go back to **Step 5** (Create Production Plan) and repeat for the next week/month.

---

## ⚠️ COMMON MISTAKES & HOW TO AVOID THEM

### **Mistake 1: Trying to create Work Order without BOM**
**Error**: "Cannot create work order without BOM"

**Fix**: 
1. Go to BOM menu
2. Create BOM for the finished good first
3. Then create work order

---

### **Mistake 2: Trying to create BOM without Finished Good**
**Error**: "No finished goods available"

**Fix**:
1. Go to Items menu
2. Create item with type = **FinishedGood**
3. Then create BOM

---

### **Mistake 3: Starting production without releasing work order**
**Error**: "Work order not released"

**Fix**:
1. Open work order
2. Click "Release" button
3. Then start production

---

### **Mistake 4: Running MRP without plan lines**
**Error**: "No plan lines to calculate"

**Fix**:
1. Create production plan first
2. Add plan lines (what to produce)
3. Then run MRP

---

### **Mistake 5: Trying to delete user with data**
**Error**: "Cannot delete user with associated records"

**Fix**:
1. Deactivate user instead of deleting
2. Or delete their associated records first (not recommended)

---

## 📋 QUICK CHECKLIST (Use This!)

### **Before Creating Work Order**:
- [ ] Finished good exists in Items?
- [ ] BOM exists for that finished good?
- [ ] Materials exist in Items?
- [ ] Inventory has stock?

### **Before Starting Production**:
- [ ] Work order created?
- [ ] Work order released?
- [ ] Materials available?
- [ ] Operator assigned?

### **Before Completing Work Order**:
- [ ] Production logged?
- [ ] Good units recorded?
- [ ] Scrap recorded?
- [ ] Hours recorded?

### **Before QC Inspection**:
- [ ] Work order completed?
- [ ] Finished goods posted to inventory?
- [ ] Production logs submitted?

---

## 🎯 SIMPLIFIED FLOW (One-Page Version)

```
1. SETUP (Once)
   ├─ Create Users
   ├─ Create Items (Raw Materials, Packaging, Finished Goods)
   ├─ Create BOMs (Recipes)
   └─ Add Opening Inventory

2. PLANNING (Weekly/Monthly)
   ├─ Create Production Plan
   ├─ Add Plan Lines (What to produce)
   ├─ Run MRP (Check materials)
   ├─ Create Work Orders
   └─ Release Work Orders

3. EXECUTION (Daily)
   ├─ Start Work Order
   ├─ Produce Goods
   ├─ Log Production
   └─ Complete Work Order

4. QUALITY (After Production)
   ├─ Inspect Work Order
   ├─ Record Result (Pass/Fail)
   └─ Set Disposition (Accept/Rework/Reject)

5. REPORTS (Anytime)
   └─ View Reports & Analytics
```

---

## 💡 REMEMBER THESE RULES

1. **Items FIRST** - Create all items before anything else
2. **BOM SECOND** - Create BOMs after items exist
3. **Work Orders THIRD** - Create work orders after BOMs exist
4. **Release BEFORE Start** - Always release before starting production
5. **Log BEFORE Complete** - Always log production before completing
6. **Complete BEFORE QC** - QC can only inspect completed work orders

---

## 🚀 DEMO SCRIPT (For Your Professor)

**"Sir, let me show you the flow:"**

1. **"First, I create items"**
   - Show creating raw material: Potato Chips
   - Show creating finished good: Cheese Chips

2. **"Second, I create BOM - the recipe"**
   - Show creating BOM for Cheese Chips
   - Add ingredients: potato chips, cheese powder
   - "Sir, this tells the system what materials to use"

3. **"Third, I create production plan"**
   - Show creating plan for May 2026
   - Add plan line: 5000 Cheese Chips

4. **"Fourth, I run MRP"**
   - Click "Run MRP"
   - "Sir, see? It checks if we have enough materials"

5. **"Fifth, I create work order"**
   - Show creating work order
   - "Sir, I need to select finished good and BOM"
   - Try without BOM → Show error
   - Add BOM → Success

6. **"Sixth, I release work order"**
   - Click "Release"
   - "Sir, now materials are reserved"

7. **"Seventh, Operator starts production"**
   - Click "Start"
   - "Sir, inventory automatically decreases"

8. **"Eighth, Operator logs production"**
   - Add production log: 4850 good, 150 scrap
   - "Sir, this records actual output"

9. **"Ninth, Operator completes work order"**
   - Click "Complete"
   - "Sir, finished goods automatically added to inventory"

10. **"Tenth, QC inspects"**
    - Record Pass result
    - "Sir, quality control ensures standards"

11. **"Finally, Manager views reports"**
    - Show cost summary
    - "Sir, system automatically calculates costs"

---

**Follow this document and you'll never be confused about the flow again!**
