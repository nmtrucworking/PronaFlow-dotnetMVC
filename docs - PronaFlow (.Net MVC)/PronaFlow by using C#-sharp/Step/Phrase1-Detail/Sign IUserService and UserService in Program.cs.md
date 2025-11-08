## Mục đích việc Đăng ký `IUserService` và `UserService`
Việc đăng ký này giống như bạn "chỉ dẫn" cho hệ thống ASP.NET Core biết rằng: _"Mỗi khi có một lớp nào đó (ví dụ như `AuthController`) yêu cầu một đối tượng kiểu `IUserService`, hãy tự động tạo ra một đối tượng từ lớp `UserService` và cung cấp cho nó."_

Điều này giúp các lớp không bị phụ thuộc cứng vào nhau (loose coupling) và làm cho code của bạn linh hoạt, dễ bảo trì và dễ kiểm thử hơn. `AuthController` chỉ cần biết đến "hợp đồng" `IUserService` mà không cần quan tâm đến việc `UserService` được tạo ra như thế nào.
## **Chi tiết các bước thực hiện**:
#### **Bước 1: Mở File `Program.cs`**
- Trong **Solution Explorer**, tìm đến project `PronaFlow.API`. 
- Mở file `Program.cs`. Đây là nơi cấu hình tất cả các dịch vụ của ứng dụng.
#### **Bước 2: Thêm `using` Statements Cần Thiết**
Để `Program.cs` có thể "nhìn thấy" được `IUserService` (trong `PronaFlow.Core`) và `UserService` (trong `PronaFlow.Services`), bạn cần thêm các `using` statement ở đầu file:
```C#
using PronaFlow.Core.Interfaces; // Giả sử bạn đặt IUserService trong thư mục Interfaces của Core
using PronaFlow.Services;
```

_Lưu ý: Nếu bạn chưa tạo thư mục `Interfaces` trong `PronaFlow.Core`, hãy tạo nó và đặt file `IUserService.cs` vào đó, đồng thời cập nhật namespace cho chính xác._
#### **Bước 3: Thêm Dòng Code Đăng Ký Dịch Vụ**

Tìm đến khu vực đăng ký các dịch vụ (bên dưới `builder.Services.AddControllers();` và `builder.Services.AddDbContext<...>();`).

Thêm dòng code sau vào:
```C#
builder.Services.AddScoped<IUserService, UserService>();
```

**Giải thích chi tiết về dòng code này:**
- **`builder.Services`**: Đây là DI Container của ASP.NET Core, nơi bạn đăng ký tất cả các dịch vụ của mình.
- **`.AddScoped<IUserService, UserService>()`**: Đây là phương thức đăng ký.
    - **`AddScoped`**: Đây là việc chỉ định "vòng đời" (lifetime) của dịch vụ. **Scoped** có nghĩa là một đối tượng `UserService` mới sẽ được tạo ra cho **mỗi một HTTP request**. Đây là lựa chọn phổ biến và phù hợp nhất cho các service có sử dụng `DbContext` (vì `DbContext` cũng được đăng ký với vòng đời Scoped theo mặc định).
    - **`<IUserService>`**: Đây là interface (hợp đồng).
    - **`, UserService`**: Đây là lớp triển khai (implementation) cụ thể.
#### **File `Program.cs` Sẽ Trông Như Thế Nào**
Sau khi thêm, đoạn code liên quan trong file `Program.cs` của bạn sẽ trông tương tự như sau:
```C#
// Thêm các using statement ở đầu file
using PronaFlow.Core.Data;
using PronaFlow.Core.Interfaces; // using cho interface
using PronaFlow.Services;       // using cho class triển khai
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Lấy chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký PronaFlowDbContext với DI Container
builder.Services.AddDbContext<PronaFlowDbContext>(options =>
    options.UseSqlServer(connectionString));

// ================================================================
// DÒNG CODE BẠN THÊM VÀO SẼ NẰM Ở ĐÂY
builder.Services.AddScoped<IUserService, UserService>();
// ================================================================


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ... phần còn lại của file
```

Sau khi hoàn thành bước này, hệ thống DI đã sẵn sàng. Khi một request được gửi đến `AuthController`, ASP.NET Core sẽ thấy rằng constructor của `AuthController` cần một `IUserService`, nó sẽ tự động tạo một instance của `UserService` (cùng với `PronaFlowDbContext` cần thiết) và "tiêm" (inject) vào đó.