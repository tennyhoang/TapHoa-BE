-- ══════════════════════════════════════════════
-- CATEGORIES
-- ══════════════════════════════════════════════
INSERT INTO "Categories" ("Id", "Name", "Description", "ImageUrl", "ParentId", "CreatedAt") VALUES
  ('c1000000-0000-0000-0000-000000000001', 'Rau cu huu co',     'Rau cu sach, trong huu co tu Da Lat va cac vung nong nghiep sach', NULL, NULL, NOW()),
  ('c1000000-0000-0000-0000-000000000002', 'Hai san tuoi song', 'Hai san danh bat va nuoi trong sach, giao nhanh trong ngay',       NULL, NULL, NOW()),
  ('c1000000-0000-0000-0000-000000000003', 'Trai cay mien Tay', 'Trai cay dac san mien Tay Nam Bo, thu hoach theo mua vu',          NULL, NULL, NOW()),
  ('c1000000-0000-0000-0000-000000000004', 'Thit & Trung',      'Thit heo, bo, ga va trung tu trang trai sach, kiem dinh ATTP',    NULL, NULL, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ══════════════════════════════════════════════
-- PRODUCTS — RAU CU HUU CO
-- ══════════════════════════════════════════════
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPrice","Stock","ThumbnailUrl","IsActive","CategoryId","CreatedAt") VALUES
  (gen_random_uuid(), 'Cai bap huu co Da Lat (1 kg)',    'Cai bap trong theo tieu chuan VietGAP, khong thuoc tru sau, la xanh muot chac tay.',       25000,  NULL,   30, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Ca rot huu co Da Lat (500 g)',    'Ca rot Da Lat dat do, ngot tu nhien, giau beta-carotene, thu hoach sang som.',              18000,  15000,  45, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Dua leo baby huu co (500 g)',     'Dua leo baby nhap giong Nhat, gion ngon, trong nha kinh Da Lat khong du luong hoa chat.',   22000,  NULL,   20, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Bi do Da Lat (1 kg)',             'Bi do ruot vang, bui deo, thich hop nau canh va lam banh. Hang theo mua vu.',               15000,  NULL,    0, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Cai thao huu co (1 kg)',          'Cai thao sach Da Lat, la trang xanh, ngot mat - ly tuong cho lau va kim chi.',              20000,  18000,  35, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Rau muong sach (500 g)',          'Rau muong trong thuy canh, khong dat ban, rua sach dung ngay.',                              8000,   NULL,   50, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW()),
  (gen_random_uuid(), 'Cai xanh huu co (500 g)',         'Cai xanh non hai buoi sang, dat hang truoc 9h giao trong ngay.',                           12000,  NULL,   40, NULL, true, 'c1000000-0000-0000-0000-000000000001', NOW());

-- ══════════════════════════════════════════════
-- PRODUCTS — HAI SAN TUOI SONG
-- ══════════════════════════════════════════════
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPrice","Stock","ThumbnailUrl","IsActive","CategoryId","CreatedAt") VALUES
  (gen_random_uuid(), 'Tom su song co 20 (500 g)',       'Tom su Ca Mau nuoi sach, con song khi giao, size 20 con/kg. Bao quan da lanh.',            120000, 105000, 15, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Ca basa phi le tuoi (500 g)',     'Ca basa An Giang phi le san, khong xuong, dong lanh IQF giu nguyen vi tuoi.',               65000,  NULL,   25, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Muc ong tuoi Phan Thiet (500 g)', 'Muc ong danh bat ven bo Phan Thiet, thit day trang, lam muc nuong hay xao sa ot deu ngon.',  85000,  NULL,   18, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Ghe xanh tuoi song (1 kg)',       'Ghe bien Can Gio con song buoc cang, giao trong 2h khu vuc noi thanh.',                    200000, NULL,    0, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Ca thu tuoi nguyen con (1 kg)',   'Ca thu Phu Quoc danh bat xa bo, thit chac, it xuong dam - cat khuc theo yeu cau.',           95000,  85000,  10, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Tom the chan trang (500 g)',       'Tom the nuoi ao sach Ben Tre, co 35-40 con/kg, rang muoi hay hap bia deu ngon.',             75000,  NULL,   20, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW()),
  (gen_random_uuid(), 'Ngheu Ben Tre tuoi (1 kg)',        'Ngheu luoc dai sach san, hap sa hay nau bun deu dam vi. Dat sang giao chieu.',              55000,  NULL,   30, NULL, true, 'c1000000-0000-0000-0000-000000000002', NOW());

-- ══════════════════════════════════════════════
-- PRODUCTS — TRAI CAY MIEN TAY
-- ══════════════════════════════════════════════
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPrice","Stock","ThumbnailUrl","IsActive","CategoryId","CreatedAt") VALUES
  (gen_random_uuid(), 'Xoai cat Hoa Loc (1 kg)',         'Xoai Hoa Loc Tien Giang chinh goc, com vang min, ngot thom dac trung - dac san so 1 mien Tay.', 55000, 45000, 30, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Sau rieng Ri6 Cai Mon (1 kg)',    'Sau rieng Ri6 Ben Tre, mui vang day, com kho rao, hat lep - thu hoach dung do chin cay.',       95000, NULL,  12, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Chom chom nhan Binh Duong (1 kg)','Chom chom nhan vo do tuoi, com trang gion, tach hat de. Mua vu thang 5-7.',                   30000, NULL,   0, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Thanh long ruot do Long An (1 kg)','Thanh long ruot do H14 Long An, ngot thanh, nhieu lycopene - xuat khau chau Au tieu chuan.',   38000, 32000, 25, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Buoi da xanh Ben Tre (1 qua ~1kg)','Buoi da xanh mui hong dao, it hat, ngot thanh khong dang - thuong hieu OCOP 4 sao.',           55000, NULL,  20, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Vu sua Lo Ren Vinh Kim (1 kg)',   'Vu sua Lo Ren Tien Giang da tim ong, nuoc sua ngot dam - dac san chi co mua thang 11 den tet.', 75000, NULL,   8, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW()),
  (gen_random_uuid(), 'Dua xiem xanh Ben Tre (1 qua)',   'Dua xiem tuoi chat san, nuoc ngot mat, com mong mem. Giao kem ong hut.',                        15000, NULL,  40, NULL, true, 'c1000000-0000-0000-0000-000000000003', NOW());

-- ══════════════════════════════════════════════
-- PRODUCTS — THIT & TRUNG
-- ══════════════════════════════════════════════
INSERT INTO "Products" ("Id","Name","Description","Price","DiscountPrice","Stock","ThumbnailUrl","IsActive","CategoryId","CreatedAt") VALUES
  (gen_random_uuid(), 'Thit heo huu co nac vai (500 g)', 'Heo nuoi thao duoc Vinh Long, khong hormone tang trong, thit do hong san chac.',              95000, 85000, 15, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW()),
  (gen_random_uuid(), 'Trung ga ta tha vuon (10 qua)',   'Trung ga ta Binh Phuoc tha vuon, long do dam mau cam, giau omega-3 va vitamin D.',             38000, NULL,  50, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW()),
  (gen_random_uuid(), 'Thit bo Wagyu nhap khau (300 g)', 'Bo Wagyu MB4 pha nac vai, van mo dep - ap chao 3 phut ra tai hong, mem tan chay.',            185000, NULL,   5, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW()),
  (gen_random_uuid(), 'Ga ta nguyen con lam san (1.5kg)','Ga Dong Tao lai tha doi, mo sach dong tui hut chan khong, giao lanh cung ngay.',              135000, NULL,   8, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW()),
  (gen_random_uuid(), 'Trung vit lon tuoi (10 qua)',     'Trung vit lon ap 14-15 ngay tu trai Vinh Long, an kem rau ram gung - giao dung do khong bi gia.', 48000, 42000, 30, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW()),
  (gen_random_uuid(), 'Thit heo ba chi huu co (500 g)',  'Ba chi heo huu co ti le nac-mo 7:3, nuoi bang cam ngo khong khang sinh.',                     115000, NULL,   0, NULL, true, 'c1000000-0000-0000-0000-000000000004', NOW());

-- ══════════════════════════════════════════════
-- VERIFY
-- ══════════════════════════════════════════════
SELECT
  c."Name"       AS "Category",
  COUNT(p."Id")  AS "So san pham",
  SUM(CASE WHEN p."Stock" = 0 THEN 1 ELSE 0 END) AS "Het hang"
FROM "Products" p
JOIN "Categories" c ON c."Id" = p."CategoryId"
WHERE c."Id" LIKE 'c1000000%'
GROUP BY c."Name"
ORDER BY c."Name";
