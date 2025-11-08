### **Bước 1: Khởi Tạo Dự Án ASP.NET Core Web API trong Visual Studio**

1. Mở Visual Studio.
2. Chọn **"Create a new project"**.
3. Tìm và chọn template **"ASP.NET Core Web API"**. Nhấn Next.
4. Đặt tên cho dự án của bạn, ví dụ: `PronaFlow.API`. Nhấn Next.
5. Trong màn hình "Additional information":
    - **Framework**: Chọn phiên bản .NET mới nhất (.NET 6.0, 7.0, 8.0+).
    - **Authentication type**: Để là **"None"** (chúng ta sẽ tự triển khai JWT).
    - Đảm bảo tùy chọn **"Use controllers (uncheck to use minimal APIs)"** được chọn.
    - Bỏ chọn **"Enable OpenAPI support"** nếu bạn muốn tự cấu hình Swagger sau, hoặc để nguyên để có giao diện test API ngay lập tức.
6. Nhấn **"Create"**.

### **Bước 2: Tích Hợp Cơ Sở Dữ Liệu với Entity Framework Core**
Vì bạn đã có sẵn database, chúng ta sẽ sử dụng phương pháp "Database First".
1. **Cài đặt các gói NuGet cần thiết:** Mở **Package Manager Console** (`Tools > NuGet Package Manager > Package Manager Console`) và chạy các lệnh sau:
    ```bash
    # Gói driver để kết nối với SQL Server
    Install-Package Microsoft.EntityFrameworkCore.SqlServer
    
    # Công cụ để chạy các lệnh của EF Core (như scaffold)
    Install-Package Microsoft.EntityFrameworkCore.Tools
    
    # Công cụ thiết kế, cần thiết cho việc reverse engineering
    Install-Package Microsoft.EntityFrameworkCore.Design
    ```
    
2. **Tạo Models từ Database (Scaffolding):** Trong Package Manager Console, chạy lệnh sau. Hãy thay thế `Your_Connection_String` bằng chuỗi kết nối thực tế đến SQL Server của bạn.
    ```bash
    Scaffold-DbContext "Your_Connection_String" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context PronaFlowDbContext -Force
    
    Scaffold-DbContext "Data Source=DESKTOP-57DRKJJ\MSSQLENTNMT;Initial Catalog=db_PronaFlow;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context PronaFlowDbContext -Force
    ```
    - **Giải thích:** Lệnh này sẽ kết nối tới CSDL của bạn, đọc toàn bộ schema và tự động tạo ra các lớp C# (entities) tương ứng với từng bảng trong thư mục `Models`, cùng với một lớp `DbContext` (`PronaFlowDbContext.cs`) để quản lý kết nối và truy vấn.

> [!NOTE] Connect String (SQL Server)
> Connect String: `Data Source=DESKTOP-57DRKJJ\MSSQLENTNMT;Initial Catalog=db_PronaFlow;Integrated Security=True`
> Hướng dẫn lấy `connect_string`: [[Get Connect String - SQL Server]]

3. **Đăng ký DbContext:** Mở tệp `Program.cs` và thêm đoạn code sau vào trước dòng `var app = builder.Build();`:
    ```C#
    // Lấy chuỗi kết nối từ file appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Đăng ký PronaFlowDbContext với DI Container
    builder.Services.AddDbContext<PronaFlowDbContext>(options =>
        options.UseSqlServer(connectionString));
    ```
    
    Và đừng quên thêm chuỗi kết nối vào tệp `appsettings.json`:
    ```JSON
    "ConnectionStrings": {
      "DefaultConnection": "Server=your_server_name;Database=PronaFlowDB;User Id=your_user;Password=your_password;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;"
    }
    ```
### **Bước 3: Thiết Kế Cấu Trúc Dự Án (Layered Architecture)**
Để đảm bảo tính chuyên nghiệp và dễ bảo trì, chúng ta nên phân tách dự án thành các lớp (projects) riêng biệt.
1. Trong Solution Explorer, chuột phải vào Solution và chọn `Add > New Project...`.
2. Tạo một **Class Library** tên là `PronaFlow.Core` (để chứa các entities/models, DTOs, và interfaces).
3. Tạo một **Class Library** khác tên là `PronaFlow.Services` (để chứa business logic).
**Cấu trúc cuối cùng sẽ trông như sau:**
- `PronaFlow.API` (Dự án chính, chứa Controllers)
    - Tham chiếu đến `PronaFlow.Services`.
- `PronaFlow.Services` (Chứa logic nghiệp vụ)
    - Tham chiếu đến `PronaFlow.Core`.
- `PronaFlow.Core` (Chứa các đối tượng chung)
    - Chuyển thư mục `Models` (đã tạo ở Bước 2) từ dự án API vào đây
> [!NOTE] Refer
 Chi tiết Bước 3: [[Details-Step3]]
 > Các bước tiến hành: [[Step3]]

### **Bước 4: Triển Khai API Đăng Ký Người Dùng (`POST /api/register`)**

Đây là nghiệp vụ đầu tiên và quan trọng nhất, kết hợp nhiều logic lại với nhau.

1. **Cài đặt thư viện băm mật khẩu:**
    ```bash
    Install-Package BCrypt.Net-Next
    ```
    
2. **Tạo Data Transfer Objects (DTOs):** Trong `PronaFlow.Core`, tạo một thư mục `DTOs/User`. Bên trong, tạo lớp `UserForRegisterDto.cs`:
    ```C#
    // PronaFlow.Core/DTOs/User/UserForRegisterDto.cs
    using System.ComponentModel.DataAnnotations;
    
    public class UserForRegisterDto
    {
        [Required]
        public string FullName { get; set; }
    
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    
        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }
    ```
    
3. **Tạo Service Interface và Implementation:**
    
    - Trong `PronaFlow.Core`, tạo `Interfaces/IUserService.cs`.
        
    - Trong `PronaFlow.Services`, tạo `UserService.cs` để triển khai interface này.
        
    ```C#
    // PronaFlow.Services/UserService.cs
    public class UserService : IUserService
    {
        private readonly PronaFlowDbContext _context;
    
        public UserService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        public async Task<User> Register(UserForRegisterDto userForRegisterDto)
        {
            // 1. Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(x => x.Email == userForRegisterDto.Email))
            {
                throw new Exception("Email already exists");
            }
    
            // 2. Băm mật khẩu
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userForRegisterDto.Password);
    
            var user = new User
            {
                FullName = userForRegisterDto.FullName,
                Email = userForRegisterDto.Email,
                PasswordHash = passwordHash,
                // Các trường khác sẽ lấy giá trị default từ CSDL
            };
    
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // Lưu user để lấy được user.Id
    
            // 3. Tự động tạo Workspace mặc định cho người dùng mới
            var defaultWorkspace = new Workspace
            {
                Name = $"{user.FullName}'s Workspace",
                OwnerId = user.Id
            };
    
            await _context.Workspaces.AddAsync(defaultWorkspace);
            await _context.SaveChangesAsync();
    
            return user;
        }
    }
    ```
    
4. **Tạo Controller:** Trong dự án `PronaFlow.API`, tạo một `AuthController.cs` trong thư mục `Controllers`.
    ``` C#
    // PronaFlow.API/Controllers/AuthController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
    
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }
    
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            try
            {
                var createdUser = await _userService.Register(userForRegisterDto);
                // Cân nhắc trả về thông tin user hoặc chỉ một thông báo thành công
                return StatusCode(201); // 201 Created
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    ```
>[!NOTE] Lưu ý:
>- Đăng ký `IUserService` và `UserService` trong `Program.cs`. [[Sign IUserService and UserService in Program.cs]]
        
### **Bước 5: Triển Khai API Đăng Nhập & JWT (`POST /api/login`)**

1. **Cài đặt các gói NuGet cho JWT:**
    ```bash
    Install-Package Microsoft.AspNetCore.Authentication.JwtBearer -Version 8.0.6
    ```
    
2. **Cấu hình JWT:**
    
    - Trong `appsettings.json`, thêm cấu hình cho JWT:
        ```JSON
        "AppSettings": {
          "Token": "your-super-secret-key-that-is-long-enough"
        }
        ```
    - Trong `Program.cs`, thêm các dịch vụ xác thực và cấu hình JWT Bearer.
**Bước 1: Lưu Trữ Khóa Bí Mật (Secret Key) trong `appsettings.json`**
	Khóa bí mật là một chuỗi ký tự dùng để ký (sign) và xác thực token. **Không bao giờ được viết trực tiếp (hardcode) khóa này vào code**. Nơi an toàn và tiêu chuẩn để lưu nó là file `appsettings.json`.
	1. Mở file `appsettings.json` trong project `PronaFlow.API`.
	2. Thêm một section mới tên là `AppSettings` như sau:
    
```JSON
    {
      "AppSettings": {
        "Token": "your-super-secret-key-that-is-long-enough-for-hs256"
      },
      "ConnectionStrings": {
        "DefaultConnection": "Server=your_server_name;Database=PronaFlowDB;..."
      },
      "Logging": {
        // ...
      }
    }
    ```
    
**Quan trọng:** Hãy thay thế chuỗi `"your-super-secret-key..."` bằng một chuỗi ký tự ngẫu nhiên, phức tạp và dài của riêng bạn. Đây chính là "mật khẩu" của server dùng để tạo token.

**Bước 2: Cấu Hình Dịch Vụ Xác Thực trong `Program.cs`**

Bây giờ chúng ta sẽ viết code để đọc khóa bí mật ở trên và cấu hình dịch vụ JWT Bearer.

1. Mở file `Program.cs`.
2. Thêm các `using` statement cần thiết ở đầu file:
    ```C#
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;
    ```
    
3. Tìm đến khu vực đăng ký dịch vụ (sau `builder.Services.AddControllers();` và trước `builder.Services.AddEndpointsApiExplorer();`).
4. Thêm đoạn code cấu hình sau vào:
    ```C#
    // ================== Cấu hình JWT Authentication ==================
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Tự cấp token
            ValidateIssuer = false,
            ValidateAudience = false,
    
            // Ký vào token
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value!)),
    
            ClockSkew = TimeSpan.Zero
        };
    });
    // ================================================================
    ```

**Giải thích chi tiết đoạn code trên:**
- **`builder.Services.AddAuthentication(...)`**: Báo cho hệ thống biết rằng ứng dụng này sẽ sử dụng dịch vụ xác thực, và phương thức mặc định sẽ là `JwtBearer`.
- **`.AddJwtBearer(...)`**: Cấu hình chi tiết cho phương thức xác thực bằng JWT.
- **`TokenValidationParameters`**: Đây là đối tượng chứa các quy tắc để kiểm tra một token có hợp lệ hay không.
    - `ValidateIssuer = false`, `ValidateAudience = false`: Trong trường hợp đơn giản này, chúng ta tự tạo và tự kiểm tra token nên không cần xác thực người phát hành (Issuer) và đối tượng nhận (Audience). Trong các hệ thống lớn hơn, bạn nên đặt là `true` và cung cấp các giá trị hợp lệ.
    - `ValidateIssuerSigningKey = true`: **Bắt buộc**. Yêu cầu hệ thống phải kiểm tra chữ ký của token để đảm bảo nó không bị giả mạo.
    - `IssuerSigningKey = new SymmetricSecurityKey(...)`: Cung cấp khóa bí mật đã được mã hóa. Đoạn code này thực hiện 3 việc:
        1. `builder.Configuration.GetSection("AppSettings:Token").Value!`: Đọc khóa bí mật từ file `appsettings.json`.
        2. `Encoding.UTF8.GetBytes(...)`: Chuyển chuỗi khóa bí mật thành một mảng bytes.
        3. `new SymmetricSecurityKey(...)`: Tạo đối tượng khóa bảo mật từ mảng bytes đó.
**Bước 3: Thêm Middleware vào Pipeline Xử Lý Request**

Cấu hình dịch vụ là chưa đủ. Bạn cần phải "bảo" pipeline xử lý request của ASP.NET Core sử dụng các dịch vụ đó.

1. Trong file `Program.cs`, tìm đến khu vực cấu hình pipeline (sau dòng `var app = builder.Build();`).
2. Thêm 2 dòng code sau vào. **Thứ tự của chúng rất quan trọng.**
    ```C#
    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    // ================== Thêm Middleware ==================
    // Dòng này phải đứng trước app.UseAuthorization();
    app.UseAuthentication();
    
    app.UseAuthorization();
    // =====================================================
    
    app.MapControllers();
    
    app.Run();
    ```
    

- **`app.UseAuthentication()`**: Middleware này sẽ kiểm tra header của mỗi request đến, tìm JWT, giải mã và xác định xem người dùng là ai (`HttpContext.User`).
    
- **`app.UseAuthorization()`**: Middleware này, sau khi đã biết người dùng là ai, sẽ kiểm tra xem họ có quyền truy cập vào endpoint mà họ đang yêu cầu hay không.
    

Sau khi hoàn thành 3 bước trên, ứng dụng của bạn đã được cấu hình hoàn chỉnh để sử dụng JWT. Bước tiếp theo sẽ là áp dụng attribute `[Authorize]` lên các controller/action mà bạn muốn bảo vệ.

3. **Triển khai Logic Login:**
    - Tạo `UserForLoginDto.cs`.
    - Trong `UserService`, thêm phương thức `Login`. Logic chính của phương thức này là:
        - Tìm người dùng bằng `email`.
        - Nếu không tìm thấy, trả về `null`.
        - Xác thực mật khẩu bằng `BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)`.
        - Nếu mật khẩu không khớp, trả về `null`.
        - Nếu thành công, tạo JWT. Hệ thống sẽ cấp cho người dùng một `access_token` và một `refresh_token`.
            
4. **Tạo Endpoint Login trong `AuthController`:** Endpoint này sẽ gọi `_userService.Login`, và nếu thành công, trả về các token trong response.
    ```C#
//PronaFlow.API/Controllers/AuthControllers.cs

[HttpPost("login")]
public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
{
    // Gọi đến service để xử lý logic đăng nhập
    var token = await _userService.Login(userForLoginDto);

    if (token == null)
    {
        // Nếu service trả về null, tức là thông tin đăng nhập không hợp lệ
        return Unauthorized("Invalid username or password.");
    }

    // Nếu đăng nhập thành công, trả về token cho client
    return Ok(new { token });
}
```

### **Bước 6: Triển Khai API Quản Lý Workspace (CRUD)**

1. **Tạo Controller:** Tạo một `WorkspacesController.cs` mới.
2. **Bảo vệ Endpoints:** Đặt `[Authorize]` attribute lên trên class `WorkspacesController`. Điều này yêu cầu mọi request đến controller này phải có một JWT hợp lệ trong header.
3. **Triển khai các Actions:**
    - `POST /api/workspaces`: Tạo workspace mới. Logic sẽ lấy `owner_id` từ `userId` trong token của người dùng đang đăng nhập.
    - `PUT /api/workspaces/{workspaceId}`: Cập nhật workspace. **Quan trọng:** Trước khi cập nhật, phải kiểm tra xem `owner_id` của workspace có trùng với `userId` từ token không để đảm bảo đúng chủ sở hữu.
    - `DELETE /api/workspaces/{workspaceId}`: Xóa workspace.
        - Kiểm tra quyền sở hữu tương tự như `PUT`.
        - **Triển khai Business Rule:** Trước khi xóa, backend phải truy vấn để kiểm tra xem có bất kỳ dự án nào còn tồn tại trong workspace đó không. Nếu có, trả về lỗi `400 Bad Request` với thông báo cụ thể.
[[Chi tiết Bước 6 - API Manager Workspace (CRUD)]]