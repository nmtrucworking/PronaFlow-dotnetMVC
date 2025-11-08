### **Mục Đích Của Kiến Trúc Phân Lớp: Tại Sao Không Để Tất Cả Code Vào Một Chỗ?**

Hãy tưởng tượng việc xây dựng một ngôi nhà. Bạn sẽ không trộn lẫn xi măng, hệ thống điện, ống nước và nội thất vào cùng một chỗ. Thay vào đó, bạn có các lớp riêng biệt: móng, khung, hệ thống điện nước, và cuối cùng là hoàn thiện nội thất. Mỗi lớp có một chức năng riêng và được xây dựng dựa trên lớp trước đó.

Trong phát triển phần mềm, kiến trúc phân lớp áp dụng cùng một triết lý. Thay vì viết tất cả code (giao tiếp CSDL, xử lý logic, xử lý request từ người dùng) vào chung một dự án, chúng ta chia nhỏ chúng thành các "lớp" riêng biệt. Mỗi lớp có một **trách nhiệm (responsibility)** duy nhất.

Mục tiêu chính là để đạt được **Nguyên tắc Tách biệt Trách nhiệm (Separation of Concerns - SoC)**, mang lại các lợi ích to lớn:

1. **Dễ Tổ Chức và Tìm Kiếm:** Bạn biết chính xác nơi để tìm code liên quan đến giao diện (API), nơi xử lý nghiệp vụ, và nơi làm việc với CSDL.
    
2. **Dễ Bảo Trì và Nâng Cấp:** Khi bạn muốn thay đổi logic tính toán, bạn chỉ cần sửa ở lớp logic mà không sợ ảnh hưởng đến lớp giao diện hay CSDL.
    
3. **Dễ Dàng Tái Sử Dụng (Reusability):** Các logic nghiệp vụ ở lớp `Services` có thể được tái sử dụng bởi nhiều loại giao diện khác nhau trong tương lai (ví dụ: một ứng dụng di động, một ứng dụng desktop) chứ không chỉ riêng cho Web API.
    
4. **Dễ Kiểm Thử (Testability):** Bạn có thể kiểm thử riêng biệt từng lớp. Ví dụ, bạn có thể test lớp logic mà không cần khởi chạy cả web server.
### **Phân Tích Cấu Trúc 3 Lớp Cho Dự Án PronaFlow**

Trong hướng dẫn trước, tôi đã đề xuất cấu trúc gồm 3 project (lớp). Dưới đây là vai trò chi tiết của từng lớp trong hệ sinh thái PronaFlow.

#### **1. `PronaFlow.Core` (Lớp Lõi / The Core Layer)**

- **Tên gọi học thuật:** Domain Layer hoặc Core Layer.
    
- **Mục đích:** Đây là **"trái tim"** của ứng dụng. Lớp này định nghĩa các đối tượng và quy tắc cốt lõi nhất của nghiệp vụ mà không phụ thuộc vào bất kỳ công nghệ cụ thể nào (như web hay CSDL).
    
- **Chứa những gì?**
    
    - **Entities (Models):** Các lớp C# ánh xạ trực tiếp tới các bảng trong CSDL của bạn (ví dụ: `User.cs`, `Workspace.cs`, `Project.cs`). Các lớp này do EF Core tự động tạo ra khi bạn chạy lệnh `Scaffold-DbContext`.
    - **DTOs (Data Transfer Objects):** Các lớp dùng để định hình dữ liệu gửi đi và nhận về qua API (ví dụ: `UserForRegisterDto.cs`, `WorkspaceForCreationDto.cs`). Việc dùng DTO giúp tách biệt mô hình CSDL khỏi mô hình API, tăng tính bảo mật và linh hoạt.
    - **Interfaces (Hợp đồng):** Các định nghĩa interface cho các service (ví dụ: `IUserService.cs`, `IWorkspaceService.cs`). Lớp này chỉ định nghĩa "cần làm gì" chứ không định nghĩa "làm như thế nào".
        
#### **2. `PronaFlow.Services` (Lớp Nghiệp Vụ / The Service Layer)**

- **Tên gọi học thuật:** Business Logic Layer (BLL) hoặc Application Layer.
    
- **Mục đích:** Đây là **"bộ não"** của ứng dụng. Lớp này chứa toàn bộ logic, quy trình và nghiệp vụ phức tạp của PronaFlow. Nó thực thi các "hợp đồng" đã được định nghĩa ở lớp `Core`.
    
- **Chứa những gì?**
    - **Service Implementations:** Các lớp triển khai chi tiết các interface từ lớp `Core` (ví dụ: `UserService.cs`, `WorkspaceService.cs`).
- **Luồng hoạt động:**
    1. Nhận yêu cầu từ lớp `API` (ví dụ: "hãy đăng ký người dùng này").
    2. Sử dụng các `Entities` từ lớp `Core` để tương tác với CSDL (thông qua `DbContext`).
    3. Thực hiện tất cả các logic cần thiết (kiểm tra email tồn tại, băm mật khẩu, tự động tạo Workspace mặc định, v.v.).
    4. Trả kết quả về cho lớp `API`.
- **Ví dụ cụ thể với PronaFlow:** Phương thức `Register` trong lớp `UserService` là một ví dụ hoàn hảo. Nó nhận vào một `UserForRegisterDto`, kiểm tra logic, băm mật khẩu, tạo một đối tượng `User` và một `Workspace`, sau đó lưu cả hai vào CSDL.
    

#### **3. `PronaFlow.API` (Lớp Giao Tiếp / The Presentation Layer)**

- **Tên gọi học thuật:** Presentation Layer.
    
- **Mục đích:** Đây là **"bộ mặt"** của backend. Lớp này chịu trách nhiệm giao tiếp với thế giới bên ngoài (cụ thể là frontend hoặc các ứng dụng khác) thông qua giao thức HTTP. Nó không chứa bất kỳ logic nghiệp vụ nào.
- **Chứa những gì?**
    - **Controllers:** Các lớp chịu trách nhiệm định nghĩa các API endpoints (ví dụ: `AuthController.cs`, `WorkspacesController.cs`).
    - **Cấu hình (Configuration):** Các file `Program.cs` và `appsettings.json` để cấu hình dịch vụ, CSDL, JWT, v.v.
        
- **Luồng hoạt động:**
    1. Nhận một HTTP request (ví dụ: `POST` đến `/api/auth/register` với dữ liệu JSON trong body).
    2. Chuyển đổi (map) dữ liệu từ request thành một DTO (ví dụ: `UserForRegisterDto`).
    3. Gọi phương thức tương ứng trong lớp `Services` và truyền DTO vào (ví dụ: `_userService.Register(dto)`).
    4. Nhận kết quả trả về từ `Services`.
    5. Tạo một HTTP Response (ví dụ: `201 Created` hoặc `400 Bad Request`) và gửi về cho client.
### **Luồng Phụ Thuộc (Dependency Flow)**

Điều quan trọng nhất của kiến trúc này là quy tắc về sự phụ thuộc:

`PronaFlow.API` **-->** `PronaFlow.Services` **-->** `PronaFlow.Core`
- **API** phụ thuộc vào (và có thể gọi) **Services**.
- **Services** phụ thuộc vào (và có thể sử dụng) **Core**.
- **Ngược lại thì không!**
    - Lớp `Services` không bao giờ biết đến sự tồn tại của `Controllers` hay HTTP.
    - Lớp `Core` là độc lập nhất, nó không biết gì về `Services` hay `API`.
Quy tắc này đảm bảo rằng "trái tim" và "bộ não" của ứng dụng có thể tồn tại và hoạt động độc lập với "bộ mặt" của nó, giúp ứng dụng trở nên cực kỳ linh hoạt và bền vững theo thời gian.