SELECT
  c."Name"       AS "Category",
  COUNT(p."Id")  AS "So san pham",
  SUM(CASE WHEN p."Stock" = 0 THEN 1 ELSE 0 END) AS "Het hang"
FROM "Products" p
JOIN "Categories" c ON c."Id" = p."CategoryId"
WHERE c."Id"::text LIKE 'c1000000%'
GROUP BY c."Name"
ORDER BY c."Name";
