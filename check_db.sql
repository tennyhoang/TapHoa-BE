SELECT COUNT(*) AS total,
       SUM(CASE WHEN "IsActive" = true THEN 1 ELSE 0 END) AS active
FROM "Products";
