DELETE FROM "Products" WHERE "CategoryId" IN (
  SELECT "Id" FROM "Categories" WHERE "Name" ILIKE '%dien tu%' OR "Name" ILIKE '%điện tử%'
);

DELETE FROM "Categories" WHERE "Name" ILIKE '%dien tu%' OR "Name" ILIKE '%điện tử%';
