using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;

namespace TapHoa.Persistence.Data;

/// <summary>
/// Chèn dữ liệu mẫu khi DB còn trống (lần deploy đầu tiên).
/// Idempotent: thoát ngay nếu bảng Users đã có dữ liệu.
/// </summary>
public static class DataSeeder
{
    // ── Fixed GUIDs — stable across reseeds ──────────────────────────────────
    // Hubs
    static readonly Guid HubQ1Id = Guid.Parse("2685894a-0176-4cc2-9030-225b23f12f1e");
    static readonly Guid HubBtId = Guid.Parse("f01c8625-b9a1-4c61-ab85-7dc54721cf9d");
    static readonly Guid HubHnId = Guid.Parse("7a383c5e-4488-4c2f-b470-e5e8c51bcecf");

    // Parent categories
    static readonly Guid CatRauId  = Guid.Parse("a1000001-0000-0000-0000-000000000001");
    static readonly Guid CatTraiId = Guid.Parse("a1000002-0000-0000-0000-000000000002");
    static readonly Guid CatKhoId  = Guid.Parse("a1000003-0000-0000-0000-000000000003");
    static readonly Guid CatTuoiId = Guid.Parse("a1000004-0000-0000-0000-000000000004");

    // Child categories
    static readonly Guid CatRauLaId    = Guid.Parse("b2000001-0000-0000-0000-000000000001");
    static readonly Guid CatCuQuaId    = Guid.Parse("b2000002-0000-0000-0000-000000000002");
    static readonly Guid CatNhietDoiId = Guid.Parse("b2000003-0000-0000-0000-000000000003");
    static readonly Guid CatMuaId      = Guid.Parse("b2000004-0000-0000-0000-000000000004");
    static readonly Guid CatThitCaId   = Guid.Parse("b2000005-0000-0000-0000-000000000005");
    static readonly Guid CatTrungSuaId = Guid.Parse("b2000006-0000-0000-0000-000000000006");

    public static async Task SeedAsync(AppDbContext db)
    {
        // Guard: chỉ seed khi DB hoàn toàn trống.
        if (await db.Users.AnyAsync()) return;

        // ── 1. Hubs ──────────────────────────────────────────────────────────
        var hubQ1 = new Hub
        {
            Id        = HubQ1Id,
            Name      = "Hub Quận 1",
            Address   = "12 Lý Tự Trọng",
            Ward      = "Phường Bến Nghé",
            District  = "Quận 1",
            City      = "TP. Hồ Chí Minh",
            Latitude  = 10.7769,
            Longitude = 106.7009,
            Status    = HubStatus.Active
        };
        var hubBt = new Hub
        {
            Id        = HubBtId,
            Name      = "Hub Bình Thạnh",
            Address   = "45 Đinh Bộ Lĩnh",
            Ward      = "Phường 24",
            District  = "Quận Bình Thạnh",
            City      = "TP. Hồ Chí Minh",
            Latitude  = 10.8142,
            Longitude = 106.7125,
            Status    = HubStatus.Active
        };
        var hubHn = new Hub
        {
            Id        = HubHnId,
            Name      = "Hub Hoàn Kiếm",
            Address   = "8 Hàng Bài",
            Ward      = "Phường Hàng Bài",
            District  = "Quận Hoàn Kiếm",
            City      = "Hà Nội",
            Latitude  = 21.0285,
            Longitude = 105.8542,
            Status    = HubStatus.Active
        };
        db.Hubs.AddRange(hubQ1, hubBt, hubHn);

        // ── 2. Users ─────────────────────────────────────────────────────────
        // Hash một lần, dùng chung — tất cả tài khoản demo đều dùng "TapHoa@2025"
        var pw = BCrypt.Net.BCrypt.HashPassword("TapHoa@2025", workFactor: 11);

        db.Users.AddRange(
            new User
            {
                FullName     = "Admin TapHoa",
                Email        = "admin@taphoa.com",
                PasswordHash = pw,
                Role         = UserRole.Admin,
                PhoneNumber  = "0901000001"
            },
            new User
            {
                FullName     = "Tuấn",
                Email        = "ttuan0147@gmail.com",
                PasswordHash = pw,
                Role         = UserRole.Admin,
                PhoneNumber  = "0901000008"
            },
            new User
            {
                FullName     = "Agent Quận 1",
                Email        = "agent.q1@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Agent,
                AgentHubId   = HubQ1Id,
                PhoneNumber  = "0901000002"
            },
            new User
            {
                FullName     = "Agent Bình Thạnh",
                Email        = "agent.bt@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Agent,
                AgentHubId   = HubBtId,
                PhoneNumber  = "0901000003"
            },
            new User
            {
                FullName     = "Agent Hà Nội",
                Email        = "agent.hn@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Agent,
                AgentHubId   = HubHnId,
                PhoneNumber  = "0901000004"
            },
            new User
            {
                FullName     = "Driver Minh Tuấn",
                Email        = "driver.tuan@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Driver,
                PhoneNumber  = "0901000005"
            },
            new User
            {
                FullName     = "Driver Văn Nam",
                Email        = "driver.nam@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Driver,
                PhoneNumber  = "0901000006"
            },
            new User
            {
                FullName     = "Khách Demo",
                Email        = "customer@taphoa.vn",
                PasswordHash = pw,
                Role         = UserRole.Customer,
                PhoneNumber  = "0901000007"
            }
        );

        // ── 3. Categories ────────────────────────────────────────────────────
        // Parent categories — IDs cố định để dễ debug và test
        var catRau  = new Category { Id = CatRauId,  Name = "Rau củ quả",           Description = "Rau sạch VietGAP, thu hoạch hàng ngày" };
        var catTrai = new Category { Id = CatTraiId, Name = "Trái cây",             Description = "Trái cây tươi, nhập hàng ngày" };
        var catKho  = new Category { Id = CatKhoId,  Name = "Hàng khô & gia vị",   Description = "Ngũ cốc, gia vị, thực phẩm khô" };
        var catTuoi = new Category { Id = CatTuoiId, Name = "Thực phẩm tươi sống", Description = "Thịt, cá, trứng, sữa tươi" };
        db.Categories.AddRange(catRau, catTrai, catKho, catTuoi);

        // Child categories — ParentId được set qua navigation property Parent
        var catRauLa    = new Category { Id = CatRauLaId,    Name = "Rau lá xanh",        Parent = catRau };
        var catCuQua    = new Category { Id = CatCuQuaId,    Name = "Củ & Quả",           Parent = catRau };
        var catNhietDoi = new Category { Id = CatNhietDoiId, Name = "Trái cây nhiệt đới", Parent = catTrai };
        var catMua      = new Category { Id = CatMuaId,      Name = "Trái cây theo mùa",  Parent = catTrai };
        var catThitCa   = new Category { Id = CatThitCaId,   Name = "Thịt & Cá",          Parent = catTuoi };
        var catTrungSua = new Category { Id = CatTrungSuaId, Name = "Trứng & Sữa",        Parent = catTuoi };
        db.Categories.AddRange(catRauLa, catCuQua, catNhietDoi, catMua, catThitCa, catTrungSua);

        // ── 4. Products ──────────────────────────────────────────────────────
        var products = new List<Product>
        {
            // Rau lá xanh
            new() { Name = "Rau muống",           Category = catRauLa,    Price = 8_000,  Stock = 200, Description = "Rau muống sạch VietGAP, bó 300g" },
            new() { Name = "Cải xanh",            Category = catRauLa,    Price = 12_000, Stock = 150, Description = "Cải xanh non tươi, bó 500g" },
            new() { Name = "Xà lách",             Category = catRauLa,    Price = 15_000, DiscountPrice = 12_000, Stock = 100, Description = "Xà lách xoăn Đà Lạt, bó 300g" },
            new() { Name = "Cải bó xôi",          Category = catRauLa,    Price = 18_000, Stock = 80,  Description = "Spinach organic, túi 200g" },

            // Củ & Quả
            new() { Name = "Cà rốt Đà Lạt",      Category = catCuQua,    Price = 18_000, Stock = 120, Description = "Cà rốt Đà Lạt loại 1, 500g" },
            new() { Name = "Khoai tây",           Category = catCuQua,    Price = 22_000, Stock = 200, Description = "Khoai tây Đà Lạt, 1kg" },
            new() { Name = "Bí đỏ",               Category = catCuQua,    Price = 25_000, Stock = 80,  Description = "Bí đỏ hokaido tươi, 1kg" },
            new() { Name = "Củ cải trắng",        Category = catCuQua,    Price = 14_000, Stock = 100, Description = "Củ cải trắng Đà Lạt, 500g" },
            new() { Name = "Cà chua bi",          Category = catCuQua,    Price = 28_000, DiscountPrice = 24_000, Stock = 90, Description = "Cà chua bi ngọt, hộp 500g" },

            // Trái cây nhiệt đới
            new() { Name = "Chuối sứ",            Category = catNhietDoi, Price = 30_000, Stock = 150, Description = "Chuối sứ Cần Thơ chín vàng, 1kg" },
            new() { Name = "Xoài cát Hòa Lộc",   Category = catNhietDoi, Price = 65_000, DiscountPrice = 55_000, Stock = 60, Description = "Xoài chín thơm ngọt, 1kg" },
            new() { Name = "Dứa mật",             Category = catNhietDoi, Price = 35_000, Stock = 90,  Description = "Dứa mật Tiền Giang, 1 quả ~1kg" },
            new() { Name = "Ổi lê Đài Loan",      Category = catNhietDoi, Price = 45_000, Stock = 70,  Description = "Ổi giòn ngọt, 1kg" },

            // Trái cây theo mùa
            new() { Name = "Bưởi da xanh",        Category = catMua,      Price = 45_000, Stock = 70,  Description = "Bưởi da xanh Bến Tre ngọt, 1 quả" },
            new() { Name = "Cam sành",            Category = catMua,      Price = 38_000, DiscountPrice = 32_000, Stock = 110, Description = "Cam sành Vĩnh Long, 1kg" },
            new() { Name = "Nho xanh không hạt",  Category = catMua,      Price = 75_000, Stock = 50,  Description = "Nho Mỹ không hạt, hộp 500g" },

            // Hàng khô & gia vị
            new() { Name = "Gạo ST25",            Category = catKho,      Price = 35_000, Stock = 300, Description = "Gạo ST25 Sóc Trăng, túi 2kg — Gạo ngon nhất thế giới" },
            new() { Name = "Đậu xanh cà",        Category = catKho,      Price = 28_000, Stock = 200, Description = "Đậu xanh sạch, túi 500g" },
            new() { Name = "Nước mắm Phú Quốc",  Category = catKho,      Price = 55_000, DiscountPrice = 48_000, Stock = 150, Description = "Nước mắm 40 độ đạm truyền thống, chai 500ml" },

            // Thịt & Cá
            new() { Name = "Thịt heo ba chỉ",    Category = catThitCa,   Price = 95_000, Stock = 50,  Description = "Ba chỉ heo tươi, miếng 500g" },
            new() { Name = "Cá basa fillet",      Category = catThitCa,   Price = 80_000, DiscountPrice = 72_000, Stock = 60, Description = "Cá basa phi lê sạch, túi 500g" },
            new() { Name = "Tôm thẻ chân trắng", Category = catThitCa,   Price = 120_000, Stock = 40, Description = "Tôm thẻ size 40con/kg, 500g" },

            // Trứng & Sữa
            new() { Name = "Trứng gà ta",         Category = catTrungSua, Price = 42_000, Stock = 300, Description = "Trứng gà ta thả vườn, vỉ 10 quả" },
            new() { Name = "Sữa tươi Vinamilk",  Category = catTrungSua, Price = 35_000, Stock = 200, Description = "Sữa tươi tiệt trùng không đường, hộp 1L" },
            new() { Name = "Phô mai Con Bò Cười", Category = catTrungSua, Price = 55_000, DiscountPrice = 49_000, Stock = 80, Description = "Phô mai tam giác, hộp 8 miếng" },
        };
        db.Products.AddRange(products);

        // Lưu lần 1 — cần IDs của Products trước khi tạo HubInventory.
        await db.SaveChangesAsync();

        // ── 5. HubInventory ──────────────────────────────────────────────────
        // Phân bổ kho đều cho 3 Hub (mỗi hub = 1/3 tổng stock của Product).
        var allHubs = new[] { hubQ1, hubBt, hubHn };
        var inventories = allHubs.SelectMany(hub =>
            products.Select(p => new HubInventory
            {
                HubId     = hub.Id,
                ProductId = p.Id,
                Stock     = Math.Max(1, p.Stock / 3)
            })
        ).ToList();

        db.HubInventories.AddRange(inventories);
        await db.SaveChangesAsync();
    }
}
