# TapHoa BE

Backend của nền tảng thương mại điện tử nông sản tươi sạch **TapHoa**, theo mô hình **O2O (Online-to-Offline)**.

> **Frontend repo:** [TapHoa-FE](https://github.com/tennyhoang/TapHoa-FE)

---

## Tech Stack

| | |
|---|---|
| Framework | .NET 10 Minimal API |
| Architecture | Clean Architecture + CQRS |
| Mediator | MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer |
| Message broker | RabbitMQ |
| API docs | Scalar (OpenAPI) |
| Logging | NLog |

---

## Kiến trúc

```
src/
├── Core/
│   ├── TapHoa.Domain/          # Entities, Enums, Domain Exceptions
│   └── TapHoa.Application/     # Commands, Queries, Handlers, Validators
├── Infrastructure/
│   ├── TapHoa.Infrastructure/  # JWT, BCrypt, email services
│   └── TapHoa.Persistence/     # EF Core DbContext, Repositories, Migrations
└── Presentation/
    ├── TapHoa.Api/             # Minimal API Endpoints, Middleware
    └── TapHoa.Worker/          # Background service (RabbitMQ consumer)
```

---

## Tính năng

- **Xác thực** — Đăng ký / Đăng nhập, mật khẩu hash BCrypt, JWT stateless
- **Mô hình O2O** — Khách chọn Hub gần nhất; đơn hàng giao đến Hub, khách ra lấy
- **Vòng đời đơn hàng** — `Pending → Confirmed → Shipping → ArrivedAtHub → Delivered → Refunded`
- **Hệ thống vai trò** — Customer, Admin, Agent (nhân viên Hub), Driver (tài xế)
- **Sản phẩm** — CRUD, lọc `isNew` / `isDiscount`, phân trang
- **Giỏ hàng** — Server-side, trừ stock khi đặt hàng
- **Khiếu nại & hoàn tiền** — Customer tạo claim, Admin duyệt
- **Thông báo** — Worker lắng nghe RabbitMQ event `OrderArrivedAtHub`, gửi email cho khách
- **API docs** — Scalar UI tại `/scalar/v1`

---

## Luồng đơn hàng

```
[Customer đặt hàng]
        ↓
   Pending
        ↓ Admin xác nhận
   Confirmed
        ↓ Admin giao hàng
   Shipping
        ↓ Driver lấy từ kho → Agent xác nhận đến Hub
   ArrivedAtHub ──→ RabbitMQ ──→ Worker gửi email khách
        ↓ Khách ra lấy / Agent xác nhận
   Delivered
        ↓ (nếu có khiếu nại)
   Refunded
```

---

## Cài đặt & Chạy

### Yêu cầu
- .NET 10 SDK
- Docker Desktop

### 1. Khởi động infrastructure

```bash
docker-compose up -d
```

Dịch vụ:
| Service | URL |
|---|---|
| PostgreSQL | `localhost:5432` |
| pgAdmin | http://localhost:5050 |
| RabbitMQ | http://localhost:15672 |

### 2. Cấu hình

Sửa `src/Presentation/TapHoa.Api/config/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taphoa2_db;Username=taphoa_user;Password=taphoa_pass"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "TapHoaAPI",
    "Audience": "TapHoaClient"
  }
}
```

### 3. Chạy API

```bash
cd src/Presentation/TapHoa.Api
dotnet run
```

API chạy tại `http://localhost:5084`.  
Docs: `http://localhost:5084/scalar/v1`

---

## API Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| POST | `/api/v1/auth/register` | Đăng ký |
| POST | `/api/v1/auth/login` | Đăng nhập |
| GET | `/api/v1/products` | Danh sách sản phẩm |
| GET | `/api/v1/products/{id}` | Chi tiết sản phẩm |
| GET/POST | `/api/v1/cart` | Quản lý giỏ hàng |
| POST | `/api/v1/orders` | Đặt hàng |
| GET | `/api/v1/orders/my` | Đơn hàng của tôi |
| GET | `/api/v1/hubs/active` | Danh sách Hub đang mở |
| PUT | `/api/v1/orders/{id}/status` | Cập nhật trạng thái (Admin) |
| POST | `/api/v1/agent/arrive` | Agent xác nhận hàng đến Hub |
