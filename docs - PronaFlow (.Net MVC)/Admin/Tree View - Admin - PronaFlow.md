```bash
PronaFlow/
│
├── 📁 PronaFlow.API/  (Lớp Trình bày - Presentation Layer)
│   ├── 📁 Areas/
│   │   └── 📁 Admin/  (✨ Khu vực mới cho Admin)
│   │       └── 📁 Controllers/
│   │           ├── DashboardController.cs
│   │           ├── UsersController.cs
│   │           └── ... (Các controllers quản lý khác)
│   │
│   ├── 📁 Controllers/  (Controllers cho người dùng thông thường)
│   │   ├── AuthController.cs
│   │   ├── ProjectController.cs
│   │   ├── TaskController.cs
│   │   └── ... (Các controllers hiện có)
│   │
│   ├── 📁 wwwroot/  (Tài nguyên Frontend)
│   │   ├── 📁 admin/  (✨ Giao diện cho Admin Panel)
│   │   │   ├── index.html
│   │   │   ├── users.html
│   │   │   └── assets/
│   │   │       ├── js/
│   │   │       └── css/
│   │   │
│   │   ├── 📁 src/  (Mã nguồn JavaScript cho người dùng)
│   │   │   ├── api/
│   │   │   ├── auth/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   ├── route/
│   │   │   └── ...
│   │   │
│   │   ├── 📁 assets/
│   │   │   ├── css/
│   │   │   ├── images/
│   │   │   └── ...
│   │   │
│   │   └── index.html (Trang chính của ứng dụng)
│   │
│   ├── Program.cs  (Cấu hình và khởi chạy ứng dụng)
│   └── appsettings.json
│
├── 📁 PronaFlow.Core/  (Lớp Lõi - Core Layer)
│   ├── 📁 Data/
│   │   └── PronaFlowDbContext.cs  (Entity Framework DbContext)
│   │
│   ├── 📁 DTOs/
│   │   ├── 📁 Admin/  (✨ DTOs riêng cho Admin)
│   │   │   ├── AdminDashboardStatsDto.cs
│   │   │   └── AdminUserViewDto.cs
│   │   │
│   │   ├── 📁 Project/
│   │   ├── 📁 Task/
│   │   └── ... (Các DTOs hiện có)
│   │
│   ├── 📁 Interfaces/
│   │   ├── IAdminService.cs  (✨ Interface cho Admin Service)
│   │   ├── IUserService.cs
│   │   ├── IProjectService.cs
│   │   └── ... (Các interfaces dịch vụ khác)
│   │
│   └── 📁 Models/
│       ├── User.cs
│       ├── Project.cs
│       └── ... (Các Entities/Models)
│
├── 📁 PronaFLow.Services/  (Lớp Logic Nghiệp vụ - Business Logic Layer)
│   ├── AdminService.cs  (✨ Service chứa logic của Admin)
│   ├── UserService.cs
│   ├── ProjectService.cs
│   └── ... (Các services hiện có)
│
├── 📁 docs/  (Thư mục tài liệu dự án)
│   ├── Admin Pages.md
│   └── Database/
│       └── ... (Tài liệu về cơ sở dữ liệu)
│
└── PronaFlow.API.sln  (Tệp giải pháp của Visual Studio)
```
