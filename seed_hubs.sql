INSERT INTO "Hubs" ("Id", "Name", "Address", "Ward", "District", "City", "Latitude", "Longitude", "Status", "CreatedAt") VALUES
  (gen_random_uuid(), 'TapHoa Hub Quận 1',          '25 Nguyễn Huệ',            'Phường Bến Nghé',    'Quận 1',           'TP. Hồ Chí Minh', 10.7731,  106.7030, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Quận 3',          '112 Võ Văn Tần',           'Phường 6',           'Quận 3',           'TP. Hồ Chí Minh', 10.7769,  106.6882, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Quận 7',          '8 Nguyễn Thị Thập',        'Phường Tân Hưng',    'Quận 7',           'TP. Hồ Chí Minh', 10.7284,  106.7218, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Bình Thạnh',      '45 Đinh Bộ Lĩnh',          'Phường 26',          'Bình Thạnh',       'TP. Hồ Chí Minh', 10.8063,  106.7143, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Thủ Đức',         '200 Võ Văn Ngân',          'Phường Linh Chiểu',  'TP. Thủ Đức',      'TP. Hồ Chí Minh', 10.8490,  106.7721, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Gò Vấp',          '33 Quang Trung',           'Phường 10',          'Gò Vấp',           'TP. Hồ Chí Minh', 10.8380,  106.6660, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Tân Bình',        '16 Cộng Hòa',              'Phường 4',           'Tân Bình',         'TP. Hồ Chí Minh', 10.8011,  106.6526, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Hà Nội - Hoàn Kiếm', '5 Hàng Bài',           'Phường Hàng Bài',    'Hoàn Kiếm',        'Hà Nội',          21.0245,  105.8412, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Hà Nội - Cầu Giấy', '144 Xuân Thủy',         'Phường Dịch Vọng',   'Cầu Giấy',         'Hà Nội',          21.0370,  105.7834, 'Active', NOW()),
  (gen_random_uuid(), 'TapHoa Hub Đà Nẵng',         '88 Nguyễn Văn Linh',       'Phường Thạc Gián',   'Thanh Khê',        'Đà Nẵng',         16.0676,  108.2098, 'Active', NOW());

SELECT "Name", "District", "City", "Status" FROM "Hubs" ORDER BY "City", "District";
