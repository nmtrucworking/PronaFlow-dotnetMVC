Tổ chức và tái Cấu trúc Code
*Để đảm bảo tính tách biệt và quản lý hiệu quả, chúng ta sẽ áp dụng nguyên tắc **phân tách theo chức năng** (Separation of Concerns) và tạo ra một khu vực (Area) riêng cho Admin.*
[[Tree View - Admin - PronaFlow]]
- [H1](#1.-Backend-(ASP.NET-Core))
#### **1. Backend (ASP.NET Core)**

##### **a. Trong `PronaFlow.API` (Lớp Trình bày - Presentation Layer)**

Đây là nơi tiếp nhận các yêu cầu HTTP và trả về dữ liệu. Chúng ta sẽ tạo một khu vực riêng cho Admin.

- **Tạo một Area cho Admin:**
    - Tạo thư mục: `PronaFlow.API/Areas/Admin/Controllers`.
    - **Lý do:** Việc này giúp cô lập hoàn toàn các `Controller` của Admin khỏi các `Controller` của người dùng thông thường, làm cho cấu trúc dự án trở nên rõ ràng và dễ điều hướng.
        
- **Tạo Admin Controllers:**
    
    - Trong thư mục trên, tạo các controller tương ứng với từng module quản lý:
        
        - `DashboardController.cs`: Xử lý các API cho trang tổng quan (ví dụ: `GET /api/admin/dashboard/stats`).
        - `UsersController.cs`: Quản lý người dùng (ví dụ: `GET /api/admin/users`, `PUT /api/admin/users/{id}/role`).
        - `WorkspacesController.cs`: Quản lý không gian làm việc.
        - `ProjectsController.cs`: Quản lý dự án.
        - `ActivityLogController.cs`: Truy xuất nhật ký hệ thống.
            ```bash
touch DashboardController.cs UsersController.cs WorkspacesController.cs
            ```
            
    - **Bảo mật:** Tất cả các controller trong khu vực Admin phải được đánh dấu bằng thuộc tính `[Authorize(Roles = "admin")]` để đảm bảo chỉ những tài khoản có vai trò 'admin' mới có thể truy cập.
    ```C#
    // Ví dụ trong PronaFlow.API/Areas/Admin/Controllers/UsersController.cs
    [Authorize(Roles = "admin")]
    [ApiController]
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    public class UsersController : ControllerBase
    {
        //... DI a new IAdminService here
    }
    ```
    

##### **b. Trong `PronaFLow.Services` (Lớp Logic Nghiệp vụ - Business Logic Layer)**

Lớp này sẽ chứa toàn bộ logic xử lý cho các yêu cầu từ Admin.

- **Tạo Service Interface và Implementation mới:**
    
    - **Interface:** Tạo file `IAdminService.cs` trong `PronaFlow.Core/Interfaces`.
        
    - **Implementation:** Tạo file `AdminService.cs` trong `PronaFLow.Services`.
        
    - **Lý do:** Thay vì thêm logic admin vào các service hiện có (như `UserService`, `ProjectService`), việc tạo `AdminService` riêng biệt sẽ giúp:
        
        1. **Tập trung logic:** Toàn bộ nghiệp vụ phức tạp của Admin (thống kê, quản lý chéo) được đặt ở một nơi duy nhất.
            
        2. **Bảo mật:** Tách biệt rõ ràng các phương thức có quyền lực cao của Admin khỏi các phương thức thông thường.
            
        3. **Dễ bảo trì:** Khi cần thay đổi logic của Admin, bạn chỉ cần tác động đến một service duy nhất.
            
- **Đăng ký Service:** Đừng quên đăng ký `IAdminService` và `AdminService` trong file `Program.cs` của dự án `PronaFlow.API`.
    

##### **c. Trong `PronaFlow.Core` (Lớp Lõi - Core Layer)**

Lớp này chứa các đối tượng truyền tải dữ liệu (DTOs) và models.

- **Tạo các DTOs dành riêng cho Admin:**
    
    - Tạo một thư mục mới: `PronaFlow.Core/DTOs/Admin/`.
        
    - Bên trong, tạo các DTOs cần thiết:
        
        - `AdminDashboardStatsDto.cs`: Chứa các số liệu thống kê trên dashboard.
            
        - `AdminUserViewDto.cs`: Chứa thông tin người dùng hiển thị cho Admin (có thể nhiều hơn DTO thông thường).
            
        - `AdminProjectViewDto.cs`: Chứa thông tin dự án hiển thị cho Admin.
            
    - **Lý do:** Dữ liệu mà Admin cần xem thường khác với người dùng cuối (ví dụ: Admin cần xem `is_deleted`, `deleted_at`). Việc tạo DTOs riêng giúp API trả về đúng và đủ dữ liệu cho giao diện Admin mà không ảnh hưởng đến các API hiện có.
        

#### **2. Frontend (JavaScript & HTML trong `PronaFlow.API/wwwroot`)**

- **Tổ chức Thư mục:**
    
    - Tạo một thư mục gốc cho toàn bộ trang Admin: `PronaFlow.API/wwwroot/admin/`.
        
    - Bên trong, cấu trúc các trang con:
        
        - `/admin/index.html` (trang dashboard)
            
        - `/admin/users.html`
            
        - `/admin/projects.html`
            
        - `/admin/assets/js/`
            
        - `/admin/assets/css/`
            
- **Routing:**
    
    - Trong file `router.js`, bạn cần thêm một cơ chế kiểm tra quyền truy cập (route guard). Trước khi điều hướng đến một URL có tiền tố `/admin`, router phải kiểm tra thông tin người dùng (lấy từ `localStorage` hoặc state) để đảm bảo họ có vai trò là `'admin'`. Nếu không, hãy chuyển hướng họ về trang chính.
        
    - Định nghĩa các route mới cho trang Admin: `/admin/dashboard`, `/admin/users`, v.v.
        
- **Tái sử dụng Components:**
    
    - Tạo một file `admin-layout.html` hoặc `admin-sidebar.html` riêng. File này có thể kế thừa cấu trúc từ `sidebar.html` của người dùng nhưng chứa các mục điều hướng dành riêng cho Admin như đã thiết kế.
        
### **II. Tổ chức Lưu trữ** #Database

Dựa trên tài liệu `Database - PronaFlow.md` và các file SQL, thiết kế cơ sở dữ liệu hiện tại của bạn đã **rất phù hợp** và **không cần thay đổi lớn** để triển khai trang Admin. Chúng ta chỉ cần tận dụng và chuẩn hóa cách sử dụng các bảng hiện có.

#### **1. Tận dụng Bảng `users`**

- Cột `role` là yếu tố then chốt. Toàn bộ logic phân quyền của Admin Panel sẽ dựa trên việc kiểm tra `users.role = 'admin'`. Đây là nền tảng cho việc bảo mật hệ thống.
    

#### **2. Tận dụng Bảng `activities`**

- Bảng này là trái tim của module "Nhật ký Hoạt động Hệ thống".
    
- **Kiến nghị Cải tiến:** Để tăng cường khả năng kiểm toán (auditing), khi một Admin thực hiện một hành động quan trọng (ví dụ: thay đổi vai trò người dùng, xóa vĩnh viễn một dự án), bản ghi trong bảng `activities` nên được làm rõ hơn.
    
    - **Cách 1 (Đơn giản):** Trong cột `description`, thêm tiền tố, ví dụ: `"Admin action: User 'admin_user' changed role of user 'target_user' to 'admin'"`.
        
    - **Cách 2 (Mở rộng):** Thêm một cột mới vào bảng `activities` là `performed_by_role` (VARCHAR) hoặc `is_admin_action` (BOOLEAN). Cách này giúp việc lọc và truy vấn các hành động của Admin sau này trở nên cực kỳ hiệu quả.
        

#### **3. Tối ưu hóa Truy vấn (Query Optimization)**

- Các câu lệnh `SELECT` được mô tả trong `Admin Pages.md` là hợp lý. Khi triển khai trong `AdminService.cs` bằng Entity Framework Core, hãy đảm bảo:
    
    - Sử dụng các phương thức bất đồng bộ (`ToListAsync`, `FirstOrDefaultAsync`) để không block luồng xử lý của server.
        
    - **Đánh Index (Indexing):** Để đảm bảo hiệu năng khi dữ liệu lớn lên, hãy chắc chắn rằng các cột thường xuyên được dùng trong mệnh đề `WHERE` hoặc `JOIN` đã được đánh index, ví dụ:
        
        - `users.role`
        - `users.email`
        - `projects.is_deleted`, `projects.is_archived`
        - `activities.created_at`
            

Việc áp dụng cấu trúc trên không chỉ giúp bạn xây dựng tính năng Admin một cách mạch lạc mà còn đặt nền móng vững chắc cho việc bảo trì và phát triển các tính năng quản trị khác trong tương lai. Chúc bạn triển khai thành công!