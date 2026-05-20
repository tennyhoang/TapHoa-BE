SELECT c."Id"::text, c."Name", COUNT(p."Id") AS products
FROM "Categories" c
LEFT JOIN "Products" p ON p."CategoryId" = c."Id"
GROUP BY c."Id", c."Name"
ORDER BY c."Name";
