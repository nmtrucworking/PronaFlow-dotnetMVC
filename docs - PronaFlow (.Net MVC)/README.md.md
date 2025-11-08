# PronaFlow - Nền Tảng Quản Lý Công Việc Trực Quan

**PronaFlow** là một ứng dụng web quản lý dự án và công việc hiện đại, được xây dựng với mục tiêu biến những ý tưởng phức tạp thành một quy trình làm việc rõ ràng, liền mạch và hiệu quả. Nền tảng này được thiết kế để trở thành một không gian làm việc tập trung, nơi các nhóm có thể cộng tác, tổ chức nhiệm vụ và đạt được mục tiêu chung một cách tối ưu.

## **Mục Lục**

## **Giới Thiệu**

Trong môi trường làm việc hiện đại, sự phân mảnh thông tin giữa nhiều ứng dụng (chat, ghi chú, lịch, email) thường gây ra sự hỗn loạn và làm giảm hiệu suất. PronaFlow ra đời từ chính vấn đề này với một sứ mệnh duy nhất: **mang lại sự rõ ràng và trật tự cho công việc của bạn**.

Chúng tôi tin rằng công nghệ tốt nhất là công nghệ "vô hình" - giúp bạn tập trung 100% vào công việc. Do đó, PronaFlow được xây dựng dựa trên ba giá trị cốt lõi:

- **TRỰC QUAN (Intuitive):** Giao diện sạch sẽ, tối giản giúp giảm thiểu sự phức tạp.
    
- **NHẤT QUÁN (Consistent):** Mọi thành phần, từ nút bấm đến luồng công việc, đều tuân theo một ngôn ngữ thiết kế thống nhất.
    
- **LINH HOẠT (Flexible):** Các công cụ mạnh mẽ cho phép bạn tùy chỉnh quy trình làm việc, vì chúng tôi hiểu rằng không có hai đội nhóm nào giống nhau.
    

## **Tính Năng Nổi Bật**

- **Quản lý Workspace & Project:** Tạo các không gian làm việc riêng biệt và quản lý nhiều dự án trong đó.
    
- **Quản lý Tác vụ Toàn diện:**
    
    - Tạo, gán và theo dõi công việc một cách chi tiết.
        
    - Phân rã công việc lớn thành các **Subtask** (công việc con) dễ quản lý.
        
    - Tổ chức công việc theo các danh sách (Task Lists) trong từng dự án.
        
- **Giao diện Trực quan:**
    
    - **Kanban Board:** Kéo-thả các thẻ công việc qua các cột trạng thái để theo dõi tiến độ một cách trực quan.
        
    - **My Tasks:** Trang tổng hợp tất cả các công việc được giao cho bạn, giúp tập trung và sắp xếp ưu tiên.
        
    - **Calendar View:** Xem các công việc và sự kiện trên lịch để quản lý thời gian hiệu quả.
        
- **Cộng tác Nâng cao:**
    
    - Mời thành viên vào dự án và phân quyền (Admin, Member).
        
    - Thảo luận trực tiếp trên các công việc và dự án thông qua hệ thống **bình luận**.
        
    - Đính kèm tệp tin vào công việc và dự án.
        
- **Hệ thống Thông báo & Hoạt động:**
    
    - Nhận thông báo về các cập nhật quan trọng (được nhắc tên, được giao việc).
        
    - Theo dõi toàn bộ lịch sử hoạt động của một dự án hoặc công việc.
        
- **Tổ chức Thông minh:**
    
    - Gán **Tags** (nhãn) với mã màu tùy chỉnh để phân loại và lọc dự án.
        
- **Quản lý Dữ liệu:**
    
    - **Thùng rác (Trash):** Khôi phục các dự án hoặc công việc đã xóa trong vòng 30 ngày.
        
    - **Lưu trữ (Archive):** Lưu trữ các dự án đã hoàn thành để giữ cho không gian làm việc luôn gọn gàng.
        

## **Kiến Trúc & Công Nghệ**

PronaFlow được xây dựng theo kiến trúc 3 lớp (3-Tier Architecture) kết hợp với mô hình Single Page Application (SPA) ở phía client.

### **Backend (.NET 8)**

- **Framework:** ASP.NET Core 8
- **Ngôn ngữ:** C#
- **Kiến trúc:** Clean Architecture với 3 project chính:
    - `PronaFlow.Core`: Chứa các Models (Entities), Interfaces, và DTOs.
    - `PronaFlow.Services`: Chứa business logic, tương tác trực tiếp với cơ sở dữ liệu.
    - `PronaFlow.API`: Chứa các Controllers, chịu trách nhiệm xử lý request/response và authentication.
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)

### **Frontend (Vanilla JavaScript)**

- **Ngôn ngữ:** JavaScript (ES6 Modules), HTML5, CSS3
- **Kiến trúc:** Single Page Application (SPA) với hệ thống routing dựa trên hash (`#/`).
    
- **Thư viện:**
    
    - **Lucide Icons:** Cho hệ thống icon nhất quán.
        
    - **FullCalendar:** Cho tính năng lịch biểu.
        
- **Điểm đặc biệt:** Giao diện được xây dựng hoàn toàn bằng Vanilla JS mà không phụ thuộc vào các framework lớn như React, Angular, hay Vue, cho thấy khả năng làm chủ các công nghệ web nền tảng.
    

## **Bắt Đầu Nhanh**

### **Yêu Cầu Hệ Thống**

- .NET 8 SDK
    
- SQL Server (phiên bản 2017 trở lên)
    
- Visual Studio 2022 hoặc trình soạn thảo mã nguồn bất kỳ (VS Code, Rider).
    

### **Cài Đặt Backend**

1. **Clone a repository:**
    ```Bash
    git clone https://your-repository-url/PronaFlow.git
    cd PronaFlow
    ```
    
2. **Mở giải pháp:** Mở tệp `PronaFlow.API.sln` bằng Visual Studio.
    
3. **Cấu hình chuỗi kết nối:**
    
    - Mở tệp `PronaFlow.API/appsettings.json`.
        
    - Chỉnh sửa chuỗi `DefaultConnection` để trỏ đến instance SQL Server của bạn.
    ```JSON
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PronaFlowDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;"
    }
    ```
    
4. **Tạo cơ sở dữ liệu:**
    
    - Dự án này sử dụng EF Core. Bạn cần tạo database và các bảng từ `PronaFlowDbContext`.
        
    - Mở `Package Manager Console` trong Visual Studio và chạy các lệnh sau:
    ```PowerShell
    Update-Database
    ```
    
    (Lưu ý: Nếu bạn chưa có thư mục `Migrations`, hãy chạy `Add-Migration InitialCreate` trước).
    

### Khởi Chạy Dự Án

1. Trong Visual Studio, đặt `PronaFlow.API` làm dự án khởi động (Set as Startup Project).
    
2. Chọn profile `PronaFlow.Web (Giao diện)` từ danh sách debug.
    
3. Nhấn **F5** hoặc nút **Start** để build và chạy dự án.
    
4. Trình duyệt sẽ tự động mở trang web tại `http://localhost:5226`.
    

## Cấu Trúc Dự Án

Dự án được tổ chức thành 3 phần chính, tuân thủ theo nguyên tắc của Clean Architecture.

```bash
/PronaFlow
├── PronaFlow.API/         # Lớp Web API, phục vụ cả giao diện người dùng (wwwroot)
│   ├── Controllers/       # Chứa các API endpoints
│   ├── wwwroot/           # Chứa toàn bộ mã nguồn Frontend (HTML, CSS, JS)
│   └── Program.cs         # Cấu hình và khởi chạy ứng dụng
│
├── PronaFlow.Core/        # Lõi của ứng dụng, không phụ thuộc vào các lớp khác
│   ├── Data/              # DbContext cho Entity Framework
│   ├── DTOs/              # Data Transfer Objects
│   ├── Interfaces/        # Định nghĩa các contracts cho services
│   └── Models/            # Các thực thể (entities) của cơ sở dữ liệu
│
└── PronaFlow.Services/    # Lớp Business Logic
    ├── ActivityService.cs
    ├── ProjectService.cs
    └── ... (các services khác)
```

## **Tổng Quan Về API**

Tất cả các API đều yêu cầu xác thực bằng JWT Bearer Token, trừ các endpoint đăng ký và đăng nhập.

- `POST /api/auth/register`: Đăng ký người dùng mới.
- `POST /api/auth/login`: Đăng nhập và nhận JWT token.
- `GET /api/workspaces`: Lấy danh sách các không gian làm việc của người dùng.
- `GET /api/workspaces/{id}/projects`: Lấy danh sách dự án trong một không gian làm việc.
- `POST /api/projects/{id}/tasks`: Tạo một công việc mới.
- ... và nhiều endpoints khác được định nghĩa trong thư mục `PronaFlow.API/Controllers`.
## **Đóng Góp**

Chúng tôi luôn chào đón các đóng góp để cải thiện PronaFlow. Vui lòng tuân thủ các quy tắc sau:

1. **Fork** a repository.
2. Tạo một nhánh mới (`git checkout -b feature/AmazingFeature`).
3. Commit các thay đổi của bạn (`git commit -m 'Add some AmazingFeature'`).
4. Đẩy lên nhánh (`git push origin feature/AmazingFeature`).
5. Mở một **Pull Request**.
## **Giấy Phép**

Dự án này được cấp phép theo Giấy phép MIT. Xem tệp `LICENSE` để biết thêm chi tiết.