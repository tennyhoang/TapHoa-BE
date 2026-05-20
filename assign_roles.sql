-- UserRole enum: Customer=0, Admin=1, Agent=2, Driver=3

-- Agent Quận 1  → Hub Quận 1
UPDATE "Users" SET
  "Role"       = 2,
  "FullName"   = 'Agent Quận 1',
  "AgentHubId" = '2685894a-0176-4cc2-9030-225b23f12f1e'
WHERE "Email" = 'agent.q1@taphoa.vn';

-- Agent Bình Thạnh → Hub Bình Thạnh
UPDATE "Users" SET
  "Role"       = 2,
  "FullName"   = 'Agent Bình Thạnh',
  "AgentHubId" = 'f01c8625-b9a1-4c61-ab85-7dc54721cf9d'
WHERE "Email" = 'agent.bt@taphoa.vn';

-- Agent Hà Nội → Hub Hoàn Kiếm
UPDATE "Users" SET
  "Role"       = 2,
  "FullName"   = 'Agent Hà Nội',
  "AgentHubId" = '7a383c5e-4488-4c2f-b470-e5e8c51bcecf'
WHERE "Email" = 'agent.hn@taphoa.vn';

-- Driver Minh Tuấn
UPDATE "Users" SET
  "Role"     = 3,
  "FullName" = 'Driver Minh Tuấn'
WHERE "Email" = 'driver.tuan@taphoa.vn';

-- Driver Văn Nam
UPDATE "Users" SET
  "Role"     = 3,
  "FullName" = 'Driver Văn Nam'
WHERE "Email" = 'driver.nam@taphoa.vn';

-- Verify
SELECT "FullName", "Email", "Role", "AgentHubId"::text
FROM "Users"
WHERE "Email" LIKE '%taphoa.vn'
  AND "Email" != 'admin@taphoa.com'
ORDER BY "Role", "Email";
