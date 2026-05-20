<div align="center">

# 🌿 TapHoa — Backend

**Nền tảng thương mại điện tử nông sản tươi sạch**
Clean Architecture · CQRS · O2O Delivery Model

[![.NET](https://img.shields.io/badge/.NET-10-512bd4?logo=dotnet)](https://dotnet.microsoft.com)
[![EF Core](https://img.shields.io/badge/EF_Core-9-purple?logo=dotnet)](https://learn.microsoft.com/ef/core)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169e1?logo=postgresql&logoColor=white)](https://postgresql.org)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-ff6600?logo=rabbitmq&logoColor=white)](https://rabbitmq.com)

[Frontend Repo](https://github.com/tennyhoang/TapHoa-FE) · [API Docs](http://localhost:5084/scalar/v1) · [Báo lỗi](https://github.com/tennyhoang/TapHoa-BE/issues)

</div>

---

## Giới thiệu

TapHoa BE là REST API cho hệ thống bán lẻ nông sản tươi sạch theo mô hình **O2O (Online-to-Offline)**. Đơn hàng được xử lý qua chuỗi trạng thái chặt chẽ từ khi đặt đến khi khách nhận tại Hub. Hệ thống hỗ trợ 4 vai trò với luồng xử lý riêng biệt: Customer, Admin, Agent (nhân viên Hub) và Driver (tài xế).

Kiến trúc theo **Clean Architecture** kết hợp **CQRS** với MediatR, đảm bảo tách biệt rõ ràng giữa Domain logic, Application use-case, Infrastructure và Presentation layer.

---

## Tech Stack

| Hạng mục | Công nghệ | Phiên bản |
|---|---|---|
| Runtime | .NET | 10 |
| API | ASP.NET Core Minimal API | 10 |
| Architecture | Clean Architecture + CQRS | — |
| Mediator | MediatR | latest |
| Validation | FluentValidation | latest |
| ORM | Entity Framework Core | 9 |
| Database | PostgreSQL | 16 |
| Auth | JWT Bearer + BCrypt | — |
| Message Broker | RabbitMQ | 3 |
| Background Jobs | .NET Worker Service | — |
| API Docs | Scalar (OpenAPI) | latest |
| Logging | NLog | latest |
| Containerization | Docker + Docker Compose | — |

---

## Kiến trúc

```
┌──────────────────────────────────────────────────────────────────┐
│                        Solution Structure                        │
│                                                                  │
│  src/Core/                                                       │
│  ├── TapHoa.Domain          Entities · Enums · Domain Exceptions │
│  └── TapHoa.Application     Commands · Queries · Handlers        │
│                             Validators · Contracts               │
│                                                                  │
│  src/Infrastructure/                                             │
│  ├── TapHoa.Infrastructure  JWT · BCrypt · Email Services        │
│  └── TapHoa.Persistence     EF Core DbContext · Repositories     │
│                             Migrations                           │
│                                                                  │
│  src/Presentation/                                               │
│  ├── TapHoa.Api             Minimal API Endpoints · Middleware    │
│  └── TapHoa.Worker          RabbitMQ Consumer · Email Sender     │
└──────────────────────────────────────────────────────────────────┘

Request flow:
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
      ├─ ValidationBehavior (FluentValidation pipeline)
      │
      ▼
  CommandHandler / QueryHandler
      │
      ├─ IRepository<T> (EF Core)
      │
      └─ IPublisher → RabbitMQ Event → Worker
```

---

## Domain Model

### Entities

| Entity | Mô tả |
|---|---|
| `User` | Người dùng, có `Role` và `AgentHubId` (nếu là Agent) |
| `Product` | Sản phẩm với `DiscountPrice`, `Stock`, `IsActive` |
| `Category` | Danh mục sản phẩm |
| `Hub` | Điểm trung chuyển O2O, có tọa độ địa lý |
| `HubInventory` | Tồn kho theo Hub |
| `Order` | Đơn hàng gắn với Hub và User |
| `OrderItem` | Chi tiết sản phẩm trong đơn |
| `CartItem` | Giỏ hàng server-side |
| `OrderClaim` | Khiếu nại sau giao hàng |
| `Address` | Địa chỉ giao hàng của Customer |
| `Review` | Đánh giá sản phẩm |

### Vai trò hệ thống

| Role | Quyền hạn |
|---|---|
| `Customer` | Mua hàng, quản lý giỏ hàng, đặt/hủy đơn, tạo khiếu nại |
| `Admin` | CRUD sản phẩm/danh mục/hub, duyệt đơn hàng, xem thống kê |
| `Agent` | Xác nhận hàng đến Hub, hoàn tất giao nhận tại Hub |
| `Driver` | Xác nhận lấy hàng từ kho, vận chuyển đến Hub |

---

## Luồng đơn hàng (Order Lifecycle)

```
  [Customer đặt hàng]
          │
          ▼
       Pending  ──── Hủy đơn ────► Cancelled
          │
     Admin xác nhận
          │
          ▼
      Confirmed ──── Hủy đơn ────► Cancelled
          │
   Admin bắt đầu giao
          │
          ▼
       Shipping
          │
  Driver lấy hàng từ kho
  Agent xác nhận đến Hub
          │
          ▼
    ArrivedAtHub ──► RabbitMQ Event
          │                │
          │         Worker tiêu thụ
          │         Gửi email khách
          │
   Khách ra lấy hàng
   Agent xác nhận
          │
          ▼
       Delivered
          │
   Khách khiếu nại
          │
          ▼
       Refunded  (Admin duyệt Claim)
```

---

## Tính năng

- **Xác thực** — Đăng ký/đăng nhập, mật khẩu hash BCrypt, JWT stateless với Role-based authorization
- **Sản phẩm** — CRUD với upload ảnh, lọc `isNew` (CreatedAt DESC) / `isDiscount` (DiscountPrice > 0), phân trang
- **Giỏ hàng** — Server-side per user, validation stock khi thêm và khi checkout
- **Đặt hàng** — Validate Hub còn hoạt động, kiểm stock từng sản phẩm, trừ stock atomic, xóa cart
- **Hệ thống Hub** — CRUD hub với tọa độ GPS, quản lý tồn kho theo Hub
- **Logistics** — Agent xác nhận hàng đến Hub, Driver xác nhận lấy từ kho
- **Claim & Refund** — Customer tạo claim kèm ảnh, Admin duyệt → chuyển Refunded
- **Thống kê** — Revenue stats theo khoảng thời gian cho Admin dashboard
- **Thông báo** — RabbitMQ Worker gửi email khi hàng đến Hub
- **API Docs** — Scalar UI với Bearer auth pre-configured

---

## Cài đặt & Chạy

### Yêu cầu

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://docker.com/products/docker-desktop)

### 1. Clone & khởi động infrastructure

```bash
git clone https://github.com/tennyhoang/TapHoa-BE.git
cd TapHoa-BE

# Khởi động PostgreSQL, pgAdmin, RabbitMQ
docker-compose up -d
```

| Service | URL | Credentials |
|---|---|---|
| PostgreSQL | `localhost:5432` | `taphoa_user` / `taphoa_pass` |
| pgAdmin | http://localhost:5050 | `admin@taphoa.com` / `admin123` |
| RabbitMQ | http://localhost:15672 | `taphoa` / `taphoa_pass` |

### 2. Cấu hình

Sửa file `src/Presentation/TapHoa.Api/config/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taphoa2_db;Username=taphoa_user;Password=taphoa_pass"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "TapHoaAPI",
    "Audience": "TapHoaClient"
  }
}
```

### 3. Chạy migration & khởi động API

```bash
# Chạy API (tự apply migration khi khởi động)
cd src/Presentation/TapHoa.Api
dotnet run
```

API: `http://localhost:5084`  
Docs: `http://localhost:5084/scalar/v1`

### 4. Chạy Worker (tùy chọn — xử lý email notification)

```bash
cd src/Presentation/TapHoa.Worker
dotnet run
```

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
| `GET` | `/api/v1/products` | Danh sách sản phẩm (filter, sort, page) | — |
| `GET` | `/api/v1/products/{id}` | Chi tiết sản phẩm | — |
| `POST` | `/api/v1/products` | Tạo sản phẩm | Admin |
| `PUT` | `/api/v1/products/{id}` | Cập nhật sản phẩm | Admin |
| `DELETE` | `/api/v1/products/{id}` | Xóa sản phẩm | Admin |

**Query params cho GET /products:**
```
search        string    Tìm theo tên
categoryId    uuid      Lọc theo danh mục
sortBy        string    newest | price_asc | price_desc | name
isNew         bool      Hàng mới nhất (CreatedAt DESC)
isDiscount    bool      Đang giảm giá (DiscountPrice > 0)
page          int       Trang hiện tại (default: 1)
pageSize      int       Số item/trang (default: 20)
```

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
| `POST` | `/api/v1/orders` | Đặt hàng | Customer |
| `GET` | `/api/v1/orders/my` | Đơn hàng của tôi | Customer |
| `GET` | `/api/v1/orders/{id}` | Chi tiết đơn | Customer |
| `DELETE` | `/api/v1/orders/{id}` | Hủy đơn | Customer |
| `GET` | `/api/v1/orders` | Tất cả đơn hàng | Admin |
| `PUT` | `/api/v1/orders/{id}/status` | Cập nhật trạng thái | Admin |

### Hubs
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `GET` | `/api/v1/hubs/active` | Danh sách Hub đang mở | — |
| `GET` | `/api/v1/hubs` | Tất cả Hub | Admin |
| `POST` | `/api/v1/hubs` | Tạo Hub | Admin |
| `PUT` | `/api/v1/hubs/{id}` | Cập nhật Hub | Admin |
| `DELETE` | `/api/v1/hubs/{id}` | Xóa Hub | Admin |

### Logistics
| Method | Endpoint | Mô tả | Auth |
|---|---|---|---|
| `POST` | `/api/v1/agent/arrive` | Xác nhận hàng đến Hub | Agent |
| `POST` | `/api/v1/agent/complete-pickup` | Hoàn tất giao nhận | Agent |
| `POST` | `/api/v1/driver/pickup` | Lấy hàng từ kho | Driver |

---

## Tài khoản demo

| Role | Email | Password |
|---|---|---|
| Agent (Quận 1) | `agent.q1@taphoa.vn` | `TapHoa@2025` |
| Agent (Bình Thạnh) | `agent.bt@taphoa.vn` | `TapHoa@2025` |
| Agent (Hà Nội) | `agent.hn@taphoa.vn` | `TapHoa@2025` |
| Driver | `driver.tuan@taphoa.vn` | `TapHoa@2025` |
| Driver | `driver.nam@taphoa.vn` | `TapHoa@2025` |
