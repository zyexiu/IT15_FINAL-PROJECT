# SnackFlow MES - Simple System Explanation
## Easy-to-Understand Flow (For Presentations)

---

## 🎯 What is SnackFlow MES?

**Simple Answer**: It's a system that helps snack food factories manage their production from start to finish. Think of it like a digital assistant that tracks everything - from raw materials to finished products.

---

## 👥 The 5 User Roles (RBAC)

**Sir, the system has 5 different user types, and each one sees different things:**

1. **Admin** - The boss, can do everything
2. **Planner** - Plans what to produce and when
3. **Operator** - Actually makes the products
4. **QC** - Checks if products are good quality
5. **Manager** - Views reports and makes decisions

**Why?** Because you don't want an Operator to delete users, and you don't want a Manager to accidentally change production data. Each person only sees what they need for their job.

---

## 📋 Module-by-Module (Simple Explanation)

### **1. Users (Admin Only)**
**What it does**: Basic CRUD - Create, Read, Update, Delete users

**Simple explanation**: 
- Admin can add new users (Planner, Operator, QC, Manager)
- Can edit their info (name, email, role)
- Can deactivate them (like suspending an account)
- Can delete them ONLY if they're inactive and have no data

**Why this matters**: You need to control who can access the system. Can't just let anyone log in.

**Connected to**: Everything - because users create all the data in the system

---

### **2. Items (Materials Master)**
**What it does**: Store all your materials and products

**Simple explanation**:
- You list everything you use: potato chips (raw), plastic bags (packaging), finished chips (product)
- Each item has a code (like RM-001), name, price, and unit (kg, pcs, box)
- You mark if it's active or not

**Why this matters**: You can't make anything if you don't know what materials you have. This is like your ingredient list.

**Connected to**: 
- **BOMs** (uses items as ingredients)
- **Inventory** (tracks how much of each item you have)
- **Work Orders** (tells you what to produce)

**Example**: 
```
RM-001: Potato Chips (Raw) - ₱50/kg
PK-001: Plastic Bag 100g - ₱0.50/pc
FG-001: Cheese Potato Chips 100g - ₱10/pc
```

---

### **3. Bill of Materials (BOM)**
**What it does**: The recipe for making a product

**Simple explanation**:
- For each finished product, you list what ingredients you need
- Like a cooking recipe: "To make 1000 pcs Cheese Chips, you need 80kg potato chips, 15kg cheese powder, 1000 plastic bags"
- You also estimate how many hours it takes (labor and machine)

**Why this matters**: Without a recipe, how do you know what materials to use? This ensures consistency - every batch uses the same ingredients.

**Connected to**:
- **Items** (uses items as ingredients)
- **Work Orders** (work orders follow the BOM recipe)
- **MRP** (calculates material needs based on BOM)

**Example**:
```
Product: Cheese Potato Chips 100g
Batch Size: 1000 pcs

Ingredients:
- Potato Chips (Raw): 80kg
- Cheese Powder: 15kg
- Salt: 2kg
- Plastic Bag: 1000 pcs

Labor: 4 hours
Machine: 6 hours
```

**Important**: You CANNOT create a Work Order without a BOM. Why? Because the system needs to know what materials to use!

---

### **4. Inventory**
**What it does**: Tracks how much stock you have in real-time

**Simple explanation**:
- Shows current stock for each item (like "500kg potato chips available")
- Every time you use materials or produce goods, it updates automatically
- Keeps a complete history (ledger) of all movements

**Why this matters**: You need to know if you have enough materials before starting production. No materials = no production.

**Connected to**:
- **Items** (tracks stock for each item)
- **Work Orders** (deducts materials when used, adds finished goods when produced)
- **MRP** (checks if you have enough stock)

**How it updates**:
```
Opening: 500kg potato chips
- Work Order uses 80kg → Balance: 420kg
+ Purchase 200kg → Balance: 620kg
- Work Order uses 80kg → Balance: 540kg
```

**Two parts**:
1. **Inventory Balance** - Current stock (like your bank balance)
2. **Inventory Ledger** - Transaction history (like your bank statement)

---

### **5. Production Plans**
**What it does**: Schedule what to produce for the week/month

**Simple explanation**:
- Planner creates a plan: "This week, we'll make 5000 Cheese Chips on Monday, 3000 BBQ Chips on Wednesday"
- It's like a to-do list for production
- Each item in the plan is called a "Plan Line"

**Why this matters**: You can't just randomly produce things. You need a schedule so everyone knows what to make and when.

**Connected to**:
- **Items** (specifies which products to make)
- **Work Orders** (plan lines become work orders)
- **MRP** (checks if you have enough materials for the plan)

**Example**:
```
Plan: May 2026 Production
- May 5: Cheese Chips - 5000 pcs
- May 7: BBQ Chips - 3000 pcs
- May 10: Sour Cream Chips - 4000 pcs
```

---

### **6. MRP (Material Requirements Planning)**
**What it does**: Calculates if you have enough materials

**Simple explanation**:
- You tell it "I want to make 5000 Cheese Chips"
- It looks at the BOM and calculates: "You need 400kg potato chips, 75kg cheese powder"
- Then it checks your inventory: "You have 540kg potato chips (OK), but only 50kg cheese powder (NOT ENOUGH!)"
- It tells you: "Buy 25kg more cheese powder"

**Why this matters**: Prevents production delays. Imagine starting production and realizing halfway you're out of cheese powder!

**Connected to**:
- **Production Plans** (calculates materials for the plan)
- **BOMs** (uses BOM to know what materials are needed)
- **Inventory** (checks current stock)

**Example**:
```
Plan: Make 5000 Cheese Chips

BOM says you need (per 1000 pcs):
- Potato Chips: 80kg
- Cheese Powder: 15kg

For 5000 pcs, you need:
- Potato Chips: 400kg
- Cheese Powder: 75kg

Current stock:
- Potato Chips: 540kg ✓ (enough)
- Cheese Powder: 50kg ✗ (short by 25kg)

MRP Result: ⚠️ Buy 25kg Cheese Powder!
```

---

### **7. Work Orders**
**What it does**: The actual production job

**Simple explanation**:
- It's like a work ticket: "Make 5000 Cheese Chips using BOM v1, scheduled for May 5"
- Has 4 statuses:
  - **Draft**: Just created, not ready yet
  - **Released**: Ready to start, materials reserved
  - **InProgress**: Currently being made
  - **Completed**: Done, finished goods added to inventory

**Why this matters**: This is where production actually happens. Without work orders, you have no record of what was produced.

**Connected to**:
- **Items** (specifies what to produce)
- **BOMs** (follows the recipe)
- **Production Plans** (created from plan lines)
- **Inventory** (deducts materials, adds finished goods)
- **Production Logs** (operators record output here)
- **QC Results** (QC inspects completed work orders)

**Important Dependencies**:
- You CANNOT create a Work Order without:
  1. A **Finished Good** (what are you making?)
  2. A **BOM** (how do you make it?)
- Why? Because the system needs to know what to produce and what materials to use!

**Example**:
```
WO-2026-0001
Product: Cheese Chips 100g
Quantity: 5000 pcs
BOM: v1
Status: InProgress
Scheduled: May 5, 08:00-16:00

Materials (from BOM):
- Potato Chips: 400kg
- Cheese Powder: 75kg
- Plastic Bags: 5000 pcs
```

---

### **8. Production Logs**
**What it does**: Operators record what they actually produced

**Simple explanation**:
- Operator says: "I made 4850 good pieces, 150 were scrap (waste)"
- Records how many hours worked (labor and machine)
- Can have multiple logs per work order (one per shift)

**Why this matters**: You need to know actual output vs. planned. Maybe you planned 5000 but only made 4850 - why? This data helps improve efficiency.

**Connected to**:
- **Work Orders** (logs belong to a work order)
- **Cost Calculation** (uses labor/machine hours to calculate cost)

**Example**:
```
Work Order: WO-2026-0001
Date: May 5, 2026
Shift: Morning
Produced: 4850 pcs (good)
Scrap: 150 pcs (waste)
Labor Hours: 4.5 hrs
Machine Hours: 6.2 hrs
Operator: John Doe
```

---

### **9. QC (Quality Control)**
**What it does**: Inspectors check if products are good quality

**Simple explanation**:
- QC Inspector checks completed work orders
- Tests: appearance, weight, taste, packaging
- Result: Pass / Fail / Conditional Pass
- Decision: Accept (ship it) / Rework (fix it) / Reject (throw it away)

**Why this matters**: You can't ship bad products to customers. QC ensures quality standards are met.

**Connected to**:
- **Work Orders** (inspects completed work orders)
- **Reports** (tracks pass/fail rates)

**Example**:
```
Work Order: WO-2026-0001
Inspector: Jane Smith
Result: Pass
Sample: 50 pcs
Defects: 2 pcs (minor)
Disposition: Accept
Notes: Overall quality excellent
```

---

### **10. Cost Summary**
**What it does**: Calculates how much it cost to make the product

**Simple explanation**:
- System automatically calculates:
  - **Material Cost**: How much materials cost (actual usage × price)
  - **Labor Cost**: How much labor cost (hours × labor rate)
  - **Machine Cost**: How much machine time cost (hours × machine rate)
- Then divides by quantity to get cost per unit

**Why this matters**: You need to know if you're making profit. If it costs ₱8 to make but you sell for ₱10, you profit ₱2 per piece.

**Connected to**:
- **Work Orders** (calculates cost per work order)
- **Production Logs** (uses labor/machine hours)
- **Inventory** (uses material costs)

**Example**:
```
Work Order: WO-2026-0001
Output: 4850 pcs

Material Cost: ₱36,850
Labor Cost: ₱675
Machine Cost: ₱620
Total: ₱38,145

Cost per Unit: ₱38,145 ÷ 4850 = ₱7.87/pc

If you sell for ₱10/pc, profit = ₱2.13/pc
```

---

### **11. Reports**
**What it does**: Shows data in useful formats

**Simple explanation**:
- Production reports (how much produced)
- Inventory reports (current stock)
- Cost reports (profitability)
- QC reports (quality metrics)
- Can export to Excel/PDF

**Why this matters**: Managers need to see the big picture. Reports turn raw data into insights.

**Connected to**: Everything - pulls data from all modules

---

### **12. Dashboard**
**What it does**: Personalized home screen for each role

**Simple explanation**:
- Admin sees: user count, system health, all modules
- Planner sees: production calendar, material shortages, MRP
- Operator sees: today's work orders, production stats
- QC sees: inspection queue, pass/fail rates
- Manager sees: charts, KPIs, analytics

**Why this matters**: Each person sees what's relevant to their job. No clutter, just what they need.

**Connected to**: Everything - shows summary of all data

---

### **13. Settings** (Admin Only)
**What it does**: Configure system settings

**Simple explanation**:
- Email settings (SMTP, welcome emails)
- reCAPTCHA settings (security)
- System info (version, database status)

**Why this matters**: Admin needs to configure the system for their company.

---

### **14. Audit Log**
**What it does**: Records every action in the system

**Simple explanation**:
- Who did what, when
- Example: "John Doe changed WO-2026-0001 status from Released to InProgress at 08:30"
- Cannot be edited or deleted (immutable)

**Why this matters**: Security, compliance, troubleshooting. If something goes wrong, you can see who did it.

**Connected to**: Everything - logs all actions

---

## 🔗 How Everything Connects (The Big Picture)

### **Setup Phase** (Admin does this first):
```
1. Admin creates Users (Planner, Operator, QC, Manager)
   ↓
2. Admin creates Items (raw materials, packaging, finished goods)
   ↓
3. Admin creates BOMs (recipes for each finished good)
   ↓
System is ready!
```

### **Planning Phase** (Planner does this):
```
1. Planner creates Production Plan
   ↓
2. Planner runs MRP (checks if enough materials)
   ↓
3. If shortage → Buy materials
   ↓
4. Planner creates Work Orders from plan
   ↓
5. Planner releases Work Orders
   ↓
Ready for production!
```

### **Execution Phase** (Operator does this):
```
1. Operator starts Work Order
   ↓
2. System deducts materials from Inventory
   ↓
3. Operator produces goods
   ↓
4. Operator logs production (good units, scrap, hours)
   ↓
5. Operator completes Work Order
   ↓
6. System adds finished goods to Inventory
   ↓
Ready for QC!
```

### **Quality Phase** (QC does this):
```
1. QC inspects completed Work Order
   ↓
2. QC records result (Pass/Fail)
   ↓
3. QC sets disposition (Accept/Rework/Reject)
   ↓
Done!
```

### **Analysis Phase** (Manager does this):
```
1. System calculates costs automatically
   ↓
2. Manager views reports
   ↓
3. Manager analyzes data
   ↓
4. Manager makes decisions for next cycle
```

---

## 🚫 Important Dependencies (What You CAN'T Do)

### **You CANNOT create a Work Order without:**
1. **A Finished Good** (Item with type = FinishedGood)
   - Why? What are you making?
2. **A BOM** (Bill of Materials for that finished good)
   - Why? How do you make it? What materials do you need?

### **You CANNOT run MRP without:**
1. **A Production Plan**
   - Why? What are you planning to produce?
2. **BOMs for the items in the plan**
   - Why? How do you calculate material needs without recipes?

### **You CANNOT complete a Work Order without:**
1. **Recording production output**
   - Why? How much did you actually make?

### **You CANNOT delete a user if:**
1. **They're still active**
   - Why? Deactivate first for safety
2. **They have associated records** (created work orders, production logs, etc.)
   - Why? Data integrity - you can't delete someone who created important records

---

## 💡 Key Talking Points for Your Professor

### **1. Why RBAC?**
"Sir, we use Role-Based Access Control because different people need different access. An Operator shouldn't be able to delete users, and a Manager shouldn't accidentally change production data. Each role has a customized dashboard showing only what they need."

### **2. Why BOMs are required for Work Orders?**
"Sir, you can't create a Work Order without a BOM because the system needs to know what materials to use. It's like trying to cook without a recipe - you don't know what ingredients you need. The BOM tells the system: 'To make 1000 Cheese Chips, you need 80kg potato chips, 15kg cheese powder, etc.'"

### **3. Why Inventory updates automatically?**
"Sir, manual inventory tracking causes errors. In our system, when an Operator issues materials for production, inventory automatically decreases. When production is done, finished goods automatically increase. It's always accurate and real-time."

### **4. Why MRP is important?**
"Sir, MRP prevents production delays. Before you start production, it checks: 'Do you have enough materials?' If not, it tells you exactly what to buy. This way, you never start a job and realize halfway you're missing ingredients."

### **5. Why multi-tenant?**
"Sir, each Admin gets their own isolated workspace. When a new company signs up, they start with zero data - completely fresh. They can't see other companies' data. It's like each company has their own private system."

### **6. Why can't delete users with data?**
"Sir, it's for data integrity. If a user created 100 work orders and you delete them, those work orders become orphaned - no creator. So we only allow deleting inactive users with no associated records. Otherwise, just deactivate them."

---

## 🎯 Simple Demo Script

**When showing to your professor:**

1. **"Sir, first I'll show you the Admin dashboard"**
   - Create a user (show role selection)
   - "See sir, I can only create Planner, Operator, QC, Manager - not Admin. Admin can only be created through signup."

2. **"Now let me show you Items"**
   - Create a raw material: Potato Chips
   - Create a finished good: Cheese Chips
   - "Sir, these are like our ingredient list and product catalog"

3. **"Next is BOM - the recipe"**
   - Create BOM for Cheese Chips
   - Add ingredients: potato chips, cheese powder, plastic bags
   - "Sir, this tells the system: to make 1000 Cheese Chips, you need these materials"

4. **"Now the Planner creates a Production Plan"**
   - Add plan line: 5000 Cheese Chips
   - Run MRP
   - "Sir, see? MRP checks if we have enough materials. If not, it shows shortage."

5. **"Then create Work Order"**
   - Try to create without BOM → Show error
   - Create with BOM → Success
   - "Sir, see? Can't create Work Order without BOM. System needs to know the recipe."

6. **"Operator executes the Work Order"**
   - Start work order
   - Log production: 4850 good, 150 scrap
   - Complete work order
   - "Sir, inventory automatically updates - materials decrease, finished goods increase"

7. **"QC inspects"**
   - Inspect completed work order
   - Record Pass result
   - "Sir, quality control ensures products meet standards"

8. **"Manager views reports"**
   - Show cost summary
   - Show production reports
   - "Sir, system automatically calculates costs. Manager can see profitability."

9. **"Finally, Audit Log"**
   - Show all actions recorded
   - "Sir, complete traceability - who did what, when"

---

**Use this document when explaining to your professor - it's conversational and easy to understand!**
