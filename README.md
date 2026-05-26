<div align="center">

# TapHoa — Backend

**Nền tảng thương mại điện tử nông sản tươi sạch**
Clean Architecture · CQRS · O2O Delivery Model

[![.NET](https://img.shields.io/badge/.NET-10-512bd4?logo=dotnet)](https://dotnet.microsoft.com)
[![EF Core](https://img.shields.io/badge/EF_Core-9-purple?logo=dotnet)](https://learn.microsoft.com/ef/core)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169e1?logo=postgresql&logoColor=white)](https://postgresql.org)

[Frontend Repo](https://github.com/tennyhoang/TapHoa-FE) · [API Docs](http://localhost:5084/scalar/v1) · [Báo lỗi](https://github.com/tennyhoang/TapHoa-BE/issues)

</div>

---

## Giới thiệu

TapHoa BE là REST API cho hệ thống bán lẻ nông sản tươi sạch theo mô hình **O2O (Online-to-Offline)**. Khách hàng đặt hàng online, thanh toán qua cổng hoặc ví nội bộ, sau đó đến Hub gần nhà để nhận hàng. Hệ thống hỗ trợ 5 vai trò với luồng xử lý riêng biệt.

Kiến trúc theo **Clean Architecture** kết hợp **CQRS** với MediatR — Domain logic, Application use-case, Infrastructure và Presentation hoàn toàn tách biệt nhau.

---

## Tech Stack

| Hạng mục | Công nghệ |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal API |
| Architecture | Clean Architecture + CQRS (MediatR) |
| Validation | FluentValidation (pipeline behavior) |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer + BCrypt |
| Image Upload | Cloudinary |
| AI | Groq (kiểm duyệt review, tạo bài viết tự động) |
| Route Optimization | OpenRouteService + Nominatim/OSM geocoding |
| Payment | SePay webhook (xác nhận nạp ví qua QR) |
| Background Jobs | .NET Hosted Service (NightBatchJob — gom đơn ban đêm) |
| API Docs | Scalar (OpenAPI) |
| Logging | NLog |

---

## Kiến trúc

```
src/Core/
├── TapHoa.Domain          Entities · Enums · Domain Exceptions
└── TapHoa.Application     Commands · Queries · Handlers
                           Validators · Contracts (interfaces)

src/Infrastructure/
├── TapHoa.Infrastructure  JWT · BCrypt · Cloudinary · Groq
│                          OpenRouteService · SePay · ICurrentUserService
└── TapHoa.Persistence     EF Core DbContext · Repositories · Migrations
                           DataSeeder

src/Presentation/
├── TapHoa.Api             Minimal API Endpoints · Middleware
└── TapHoa.Worker          (Worker dự phòng — chưa dùng trong production)
```

**Request flow:**
```
HTTP Request
    │
    ▼
Middleware (ExceptionHandling · Auth · CORS)
    │
    ▼
Minimal API Endpoint
    │
    ▼
MediatR.Send(Command | Query)
    │
    ├─ ValidationBehavior (FluentValidation)
    │
    ▼
CommandHandler / QueryHandler
    │
    └─ IRepository<T> (EF Core)
```

---

## Domain Model

### Entities chính

| Entity | Mô tả |
|---|---|
| `User` | Người dùng — 5 Role, WalletBalance, `WarehouseId` (Driver), `ManagedWarehouseId` (WarehouseManager) |
| `Warehouse` | Kho vận — Driver lấy hàng từ đây, WarehouseManager phụ trách |
| `Hub` | Điểm trung chuyển O2O — có tọa độ GPS |
| `HubInventory` | Tồn kho theo Hub |
| `Product` | Sản phẩm — `DiscountPrice`, `Stock`, `IsActive` |
| `Category` | Danh mục dạng cây (parent-child) |
| `ProductImage` | Ảnh phụ của sản phẩm |
| `Order` | Đơn hàng — gắn với Hub, hỗ trợ thanh toán ví một phần |
| `OrderItem` | Chi tiết sản phẩm trong đơn |
| `OrderClaim` | Khiếu nại sau giao hàng |
| `OrderDamagedReport` | Báo cáo hàng lỗi tại Hub (Agent ghi nhận) |
| `CartItem` | Giỏ hàng server-side |
| `Address` | Địa chỉ của Customer |
| `UserHub` | Hub yêu thích |
| `Review` | Đánh giá sản phẩm — qua kiểm duyệt Groq AI |
| `FlashSaleSession` | Phiên flash sale có khung giờ |
| `FlashSaleItem` | Sản phẩm trong flash sale (giá/stock riêng) |
| `Article` | Bài viết cẩm nang — AI tạo nội dung + ảnh |
| `WalletTransaction` | Lịch sử giao dịch ví |
| `WalletTopupRequest` | Yêu cầu nạp ví qua QR (SePay) |
| `WithdrawRequest` | Yêu cầu rút tiền về ngân hàng |

### Vai trò hệ thống

| Role | Quyền hạn |
|---|---|
| `Customer` | Mua hàng, quản lý giỏ hàng, đặt/hủy đơn, tạo khiếu nại, nạp/rút ví |
| `Admin` | Toàn quyền — quản lý sản phẩm, hub, kho, người dùng, flash sale, bài viết, thống kê, duyệt rút tiền |
| `Agent` | Nhân viên Hub — xác nhận hàng đến, hoàn tất giao nhận, báo cáo hàng lỗi |
| `Driver` | Tài xế — lấy đơn từ kho, vận chuyển đến Hub, tối ưu lộ trình qua AI |
| `WarehouseManager` | Quản lý kho — được Admin phân công phụ trách kho cụ thể |

---

## Luồng đơn hàng (Order Lifecycle)

```
[Customer đặt hàng]
        │
        ▼
  PendingPayment  ──── Hủy đơn ──────────────────► Cancelled
        │
   Thanh toán thành công
   (bank transfer / ví / hybrid)
        │
        ▼
Paid_WaitingForBatch ─── Hủy đơn ──────────────► Cancelled
        │
   NightBatchJob gom đơn (0h hàng đêm)
   Driver xác nhận lấy hàng từ kho
        │
        ▼
  ShippingToHub
        │
   Agent xác nhận hàng đến Hub
        │
        ▼
InHub_ReadyForPickup
        │
   Khách ra Hub nhận hàng
   Agent xác nhận
        │
        ▼
    Completed
        │
   Khách tạo khiếu nại
   Admin duyệt Claim
        │
        ▼
    Refunded
```

---

## Tính năng

- **Auth** — Đăng ký / đăng nhập, JWT stateless, Role-based authorization (5 vai trò)
- **Sản phẩm** — CRUD + Cloudinary upload, lọc mới/giảm giá, phân trang
- **Danh mục** — Cây danh mục 2 cấp (parent-child)
- **Giỏ hàng** — Server-side, validate stock khi thêm và khi checkout
- **Đặt hàng** — Validate Hub, kiểm stock atomic, hỗ trợ thanh toán ví một phần (`UseWallet`)
- **Flash Sale** — Phiên theo khung giờ, giá/stock độc lập mỗi phiên, tự động deactivate khi hết giờ
- **Ví điện tử** — Nạp qua VietQR / SePay webhook, rút về ngân hàng, lịch sử giao dịch
- **Hub & Logistics** — Agent xác nhận hàng đến, báo cáo hàng lỗi; Driver lấy từ kho và vận chuyển
- **Tối ưu lộ trình** — OpenRouteService TSP + Nominatim geocoding, trả về thứ tự hub tối ưu cho Driver
- **Kho vận** — CRUD kho, gán Driver và WarehouseManager theo kho, kho tự động resolve khi Driver optimize route
- **Khiếu nại & Hoàn tiền** — Customer tạo claim kèm ảnh, Admin duyệt → chuyển `Refunded`
- **Đánh giá** — Sau khi nhận hàng, qua kiểm duyệt Groq AI (sentiment: Positive/Neutral/Negative)
- **Bài viết** — AI tạo tiêu đề + nội dung + prompt ảnh qua Groq, hỗ trợ markdown
- **Thống kê** — Revenue by date range, order count, top products cho Admin dashboard
- **API Docs** — Scalar UI với Bearer auth pre-configured tại `/scalar/v1`

---

## API Reference

### Auth
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Đăng ký tài khoản | — |
| `POST` | `/api/v1/auth/login` | Đăng nhập, nhận JWT | — |

### Products
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/products` | Danh sách (filter, sort, page) | — |
| `GET` | `/api/v1/products/{id}` | Chi tiết sản phẩm | — |
| `POST` | `/api/v1/products` | Tạo sản phẩm | Admin |
| `PUT` | `/api/v1/products/{id}` | Cập nhật sản phẩm | Admin |
| `DELETE` | `/api/v1/products/{id}` | Xóa sản phẩm | Admin |

Query params cho `GET /products`: `search`, `categoryId`, `sortBy` (newest|price_asc|price_desc|name), `isNew`, `isDiscount`, `page`, `pageSize`

### Categories
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/categories` | Cây danh mục đầy đủ | — |
| `POST` | `/api/v1/categories` | Tạo danh mục | Admin |
| `PUT` | `/api/v1/categories/{id}` | Cập nhật | Admin |
| `DELETE` | `/api/v1/categories/{id}` | Xóa | Admin |

### Cart
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/cart` | Lấy giỏ hàng | Customer |
| `POST` | `/api/v1/cart` | Thêm sản phẩm | Customer |
| `PUT` | `/api/v1/cart/{id}` | Cập nhật số lượng | Customer |
| `DELETE` | `/api/v1/cart/{id}` | Xóa item | Customer |

### Orders
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `POST` | `/api/v1/orders` | Đặt hàng (`HubId`, `PaymentMethod`, `UseWallet`) | Customer |
| `GET` | `/api/v1/orders/my` | Đơn hàng của tôi | Customer |
| `GET` | `/api/v1/orders/{id}` | Chi tiết đơn | Customer |
| `PATCH` | `/api/v1/orders/{id}/cancel` | Hủy đơn | Customer |
| `GET` | `/api/v1/orders/all` | Tất cả đơn hàng | Admin |
| `PUT` | `/api/v1/orders/{id}/status` | Cập nhật trạng thái | Admin |

### Hubs
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/hubs/active` | Hub đang hoạt động | — |
| `GET` | `/api/v1/hubs` | Tất cả Hub | Admin |
| `POST` | `/api/v1/hubs` | Tạo Hub | Admin |
| `PUT` | `/api/v1/hubs/{id}` | Cập nhật Hub | Admin |
| `PATCH` | `/api/v1/hubs/{id}/toggle` | Bật/tắt Hub | Admin |
| `DELETE` | `/api/v1/hubs/{id}` | Xóa Hub | Admin |

### Warehouses
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/warehouses` | Kho đang hoạt động | — |
| `GET` | `/api/v1/admin/warehouses` | Tất cả kho | Admin |
| `POST` | `/api/v1/admin/warehouses` | Tạo kho | Admin |
| `PUT` | `/api/v1/admin/warehouses/{id}` | Cập nhật kho | Admin |
| `PATCH` | `/api/v1/admin/warehouses/{id}/toggle` | Bật/tắt kho | Admin |
| `DELETE` | `/api/v1/admin/warehouses/{id}` | Xóa kho | Admin |
| `PATCH` | `/api/v1/admin/users/{id}/assign-warehouse` | Gán kho cho Driver hoặc WarehouseManager | Admin |

### Agent
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/agent/orders` | Đơn tại Hub của Agent | Agent |
| `PATCH` | `/api/v1/agent/orders/{id}/arrive` | Xác nhận hàng đến Hub | Agent |
| `PATCH` | `/api/v1/agent/orders/{id}/complete-pickup` | Hoàn tất giao nhận cho khách | Agent |
| `POST` | `/api/v1/agent/orders/{id}/confirm-pickup` | (alias) Hoàn tất giao nhận | Agent |
| `POST` | `/api/v1/agent/report-damaged` | Báo cáo hàng lỗi tại Hub | Agent |

### Driver
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/driver/orders` | Đơn chờ lấy (gom theo Hub) | Driver |
| `GET` | `/api/v1/driver/orders/shipping` | Đơn đang vận chuyển | Driver |
| `GET` | `/api/v1/driver/orders/delivered` | Đơn đã giao hôm nay | Driver |
| `PATCH` | `/api/v1/driver/orders/pickup-from-warehouse` | Xác nhận lấy hàng từ kho | Driver |
| `GET` | `/api/v1/driver/me/warehouse` | Kho cố định của Driver hiện tại | Driver |
| `POST` | `/api/v1/driver/optimize-route` | Tối ưu lộ trình Hub (OpenRouteService) | Driver |

### Wallet
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/wallet/me` | Số dư ví | Authenticated |
| `GET` | `/api/v1/wallet/me/transactions` | Lịch sử giao dịch | Authenticated |
| `POST` | `/api/v1/wallet/me/topup/initiate` | Tạo QR nạp ví (SePay) | Authenticated |
| `POST` | `/api/v1/wallet/me/withdraw-request` | Yêu cầu rút tiền về ngân hàng | Authenticated |
| `POST` | `/api/v1/payment/sepay-webhook` | Webhook SePay xác nhận nạp tiền | — |

### Admin — Wallet & Revenue
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/admin/wallet/withdraw-requests` | Danh sách yêu cầu rút tiền | Admin |
| `PATCH` | `/api/v1/admin/wallet/withdraw-requests/{id}/approve` | Duyệt rút tiền | Admin |
| `PATCH` | `/api/v1/admin/wallet/withdraw-requests/{id}/reject` | Từ chối rút tiền | Admin |
| `GET` | `/api/v1/admin/revenue` | Thống kê doanh thu | Admin |

### Flash Sale
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/flash-sale/active` | Phiên flash sale đang chạy | — |
| `GET` | `/api/v1/admin/flash-sale` | Tất cả phiên | Admin |
| `POST` | `/api/v1/admin/flash-sale` | Tạo phiên flash sale | Admin |
| `PUT` | `/api/v1/admin/flash-sale/{id}` | Cập nhật phiên | Admin |
| `DELETE` | `/api/v1/admin/flash-sale/{id}` | Xóa phiên | Admin |

### Articles (Cẩm nang)
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/articles` | Bài viết đã xuất bản | — |
| `GET` | `/api/v1/admin/articles` | Tất cả bài viết | Admin |
| `POST` | `/api/v1/admin/articles` | Tạo bài viết | Admin |
| `PUT` | `/api/v1/admin/articles/{id}` | Cập nhật bài viết | Admin |
| `PATCH` | `/api/v1/admin/articles/{id}/generate-content` | AI tạo nội dung (Groq) | Admin |
| `PATCH` | `/api/v1/admin/articles/{id}/generate-image-prompt` | AI tạo prompt ảnh | Admin |
| `DELETE` | `/api/v1/admin/articles/{id}` | Xóa bài viết | Admin |

### Users & Profile
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/users` | Tất cả người dùng (filter, page) | Admin |
| `PUT` | `/api/v1/users/{id}` | Cập nhật người dùng | Admin |
| `DELETE` | `/api/v1/users/{id}` | Xóa người dùng | Admin |
| `GET` | `/api/v1/users/me` | Hồ sơ cá nhân | Authenticated |
| `PUT` | `/api/v1/users/me` | Cập nhật hồ sơ | Authenticated |
| `PATCH` | `/api/v1/users/me/password` | Đổi mật khẩu | Authenticated |
| `GET` | `/api/v1/users/me/favorite-hubs` | Hub yêu thích | Authenticated |
| `POST` | `/api/v1/users/me/favorite-hubs/{hubId}` | Thêm Hub yêu thích | Authenticated |
| `DELETE` | `/api/v1/users/me/favorite-hubs/{hubId}` | Xóa Hub yêu thích | Authenticated |

### Khác
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/addresses` | Địa chỉ của tôi | Authenticated |
| `POST` | `/api/v1/addresses` | Thêm địa chỉ | Authenticated |
| `PUT` | `/api/v1/addresses/{id}` | Cập nhật địa chỉ | Authenticated |
| `DELETE` | `/api/v1/addresses/{id}` | Xóa địa chỉ | Authenticated |
| `GET` | `/api/v1/products/{id}/reviews` | Đánh giá sản phẩm | — |
| `POST` | `/api/v1/products/{id}/reviews` | Viết đánh giá | Customer |
| `POST` | `/api/v1/upload/image` | Upload ảnh lên Cloudinary | Admin |
| `POST` | `/api/v1/claims` | Tạo khiếu nại | Customer |

---

## Cài đặt & Chạy

### Yêu cầu

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://docker.com/products/docker-desktop)

### 1. Clone & khởi động PostgreSQL

```bash
git clone https://github.com/tennyhoang/TapHoa-BE.git
cd TapHoa-BE

# Khởi động PostgreSQL + pgAdmin
docker-compose up -d
```

| Service | URL | Credentials |
|---|---|---|
| PostgreSQL | `localhost:5432` | `taphoa_user` / `taphoa_pass` |
| pgAdmin | http://localhost:5050 | `admin@taphoa.com` / `admin123` |

### 2. Cấu hình

Tạo file `src/Presentation/TapHoa.Api/config/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taphoa2_db;Username=taphoa_user;Password=taphoa_pass"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "TapHoaAPI",
    "Audience": "TapHoaClient"
  },
  "Cloudinary": {
    "CloudName": "...",
    "ApiKey": "...",
    "ApiSecret": "..."
  },
  "Groq": {
    "ApiKey": "..."
  },
  "SePay": {
    "ApiKey": "...",
    "AccountPrefix": "THWL"
  },
  "OpenRouteService": {
    "ApiKey": "..."
  }
}
```

> Cloudinary, Groq, SePay, và OpenRouteService là optional — các tính năng tương ứng sẽ trả lỗi nếu thiếu key, nhưng các tính năng khác vẫn hoạt động bình thường.

### 3. Chạy API

```bash
cd src/Presentation/TapHoa.Api
dotnet run
```

API: `http://localhost:5084`
Docs: `http://localhost:5084/scalar/v1`

Khi khởi động lần đầu, API tự động:
- Apply tất cả pending EF Core migrations
- Seed dữ liệu mẫu (Hubs, Users, Categories, Products, Warehouses)

---

## Tài khoản demo

Tất cả tài khoản dùng mật khẩu: **`TapHoa@2025`**

| Role | Email |
|---|---|
| Admin | `admin@taphoa.com` |
| Agent — Hub Quận 1 | `agent.q1@taphoa.vn` |
| Agent — Hub Bình Thạnh | `agent.bt@taphoa.vn` |
| Agent — Hub Hoàn Kiếm (HN) | `agent.hn@taphoa.vn` |
| Driver | `driver.tuan@taphoa.vn` |
| Driver | `driver.nam@taphoa.vn` |
| Customer | `customer@taphoa.vn` |

---

## Error Codes

Lỗi nghiệp vụ trả về dạng `{ "error": "...", "errorCode": "..." }`.

| Code | Ý nghĩa |
|---|---|
| `USER_NOT_FOUND` | Không tìm thấy người dùng |
| `ROLE_MISMATCH` | Role không phù hợp với thao tác |
| `WAREHOUSE_NOT_FOUND` | Kho không tồn tại |
| `WAREHOUSE_INACTIVE` | Kho đang bị vô hiệu hóa |
| `DRIVER_NOT_FOUND` | Không tìm thấy tài khoản Driver |
| `DRIVER_NO_WAREHOUSE` | Driver chưa được gán kho cố định |
| `ORDER_NOT_FOUND` | Không tìm thấy đơn hàng |
| `HUB_FORBIDDEN` | Agent không phụ trách Hub này |
| `INSUFFICIENT_WALLET` | Số dư ví không đủ |
| `STOCK_INSUFFICIENT` | Sản phẩm không đủ hàng |
