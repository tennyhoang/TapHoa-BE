using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapHoa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductThumbnailUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var updates = new[]
            {
                ("Rau muống",           "/products/rau-muong.jpg"),
                ("Cải xanh",            "/products/cai-xanh.jpg"),
                ("Xà lách",             "/products/xa-lach.jpg"),
                ("Cải bó xôi",          "/products/cai-bo-xoi.jpg"),
                ("Cà rốt Đà Lạt",      "/products/ca-rot.jpg"),
                ("Khoai tây",           "/products/khoai-tay.jpg"),
                ("Bí đỏ",               "/products/bi-do.jpg"),
                ("Củ cải trắng",        "/products/cu-cai-trang.jpg"),
                ("Cà chua bi",          "/products/ca-chua-bi.jpg"),
                ("Chuối sứ",            "/products/chuoi-su.jpg"),
                ("Xoài cát Hòa Lộc",   "/products/xoai.jpg"),
                ("Dứa mật",             "/products/dua-mat.jpg"),
                ("Ổi lê Đài Loan",      "/products/oi-le.jpg"),
                ("Bưởi da xanh",        "/products/buoi-da-xanh.jpg"),
                ("Cam sành",            "/products/cam-sanh.jpg"),
                ("Nho xanh không hạt",  "/products/nho-xanh.jpg"),
                ("Gạo ST25",            "/products/gao-st25.jpg"),
                ("Đậu xanh cà",         "/products/dau-xanh.jpg"),
                ("Nước mắm Phú Quốc",   "/products/nuoc-mam.jpg"),
                ("Thịt heo ba chỉ",    "/products/thit-heo-ba-chi.jpg"),
                ("Cá basa fillet",      "/products/ca-basa.jpg"),
                ("Tôm thẻ chân trắng", "/products/tom-the.jpg"),
                ("Trứng gà ta",         "/products/trung-ga.jpg"),
                ("Sữa tươi Vinamilk",   "/products/sua-tuoi.jpg"),
                ("Phô mai Con Bò Cười", "/products/pho-mai.jpg"),
            };

            foreach (var (name, url) in updates)
                migrationBuilder.Sql(
                    $"UPDATE \"Products\" SET \"ThumbnailUrl\" = '{url}' WHERE \"Name\" = '{name.Replace("'", "''")}' AND (\"ThumbnailUrl\" IS NULL OR \"ThumbnailUrl\" = '');");

            // Categories
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"ImageUrl\" = '/categories/cat-rau-cu.jpg'   WHERE \"Name\" = 'Rau củ quả'           AND (\"ImageUrl\" IS NULL OR \"ImageUrl\" = '');");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"ImageUrl\" = '/categories/cat-trai-cay.jpg' WHERE \"Name\" = 'Trái cây'             AND (\"ImageUrl\" IS NULL OR \"ImageUrl\" = '');");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"ImageUrl\" = '/categories/cat-hang-kho.jpg' WHERE \"Name\" = 'Hàng khô & gia vị'   AND (\"ImageUrl\" IS NULL OR \"ImageUrl\" = '');");
            migrationBuilder.Sql("UPDATE \"Categories\" SET \"ImageUrl\" = '/categories/cat-tuoi-song.jpg' WHERE \"Name\" = 'Thực phẩm tươi sống' AND (\"ImageUrl\" IS NULL OR \"ImageUrl\" = '');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
