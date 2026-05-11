-- DEBUG: Check Notifications in Database
-- Run this in your MySQL database to see what's actually stored

-- 1. Check all material request notifications
SELECT 
    NotificationId,
    RecipientRole,
    RecipientUserId,
    TenantId,
    Type,
    Title,
    Message,
    IsRead,
    CreatedAt
FROM Notifications
WHERE Type = 'MaterialRequest'
ORDER BY CreatedAt DESC
LIMIT 10;

-- 2. Check Admin user details
SELECT 
    u.Id AS AdminUserId,
    u.FullName,
    u.TenantId AS AdminTenantId,
    r.Name AS Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Admin';

-- 3. Check Planner user details
SELECT 
    u.Id AS PlannerUserId,
    u.FullName,
    u.TenantId AS PlannerTenantId,
    r.Name AS Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Planner';

-- 4. Check material requests
SELECT 
    RequestId,
    TenantId,
    Status,
    RequestedByUserId,
    RequestedAt
FROM MaterialRequests
ORDER BY RequestedAt DESC
LIMIT 5;

-- 5. EXPECTED RESULTS:
-- Notifications should have:
--   - RecipientRole = 'Admin'
--   - TenantId = (Admin's User ID)
--   - IsRead = 0 (false)
--
-- Admin user should have:
--   - TenantId = NULL or empty
--
-- Planner user should have:
--   - TenantId = (Admin's User ID)
--
-- Material requests should have:
--   - TenantId = (Admin's User ID)

-- 6. Test query that SHOULD work:
-- Replace 'YOUR_ADMIN_USER_ID' with actual Admin user ID from query #2
SELECT COUNT(*) AS NotificationCount
FROM Notifications
WHERE RecipientRole = 'Admin'
  AND IsRead = 0
  AND (TenantId IS NULL OR TenantId = '' OR TenantId = 'YOUR_ADMIN_USER_ID');
