### **Bước 1: Tạo Các Lớp DTO (Data Transfer Objects) cho Workspace**

Chúng ta sẽ tạo các lớp DTO để định hình dữ liệu đầu vào và đầu ra cho API, thay vì sử dụng trực tiếp lớp Entity.

1. Trong project `PronaFlow.Core`, tạo một thư mục mới bên trong `DTOs` tên là `Workspace`.
    
2. Tạo 3 file C# sau bên trong thư mục `PronaFlow.Core/DTOs/Workspace`:
    
    **`WorkspaceDto.cs` (Dùng để trả về dữ liệu)**
    ```C#
    namespace PronaFlow.Core.DTOs.Workspace;
    
    public class WorkspaceDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long OwnerId { get; set; }
    }
    ```
    
    **`WorkspaceCreateDto.cs` (Dùng để nhận dữ liệu khi tạo mới)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Workspace;
    
    public class WorkspaceCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    
        public string? Description { get; set; }
    }
    ```
### **Bước 2: Định Nghĩa Interface `IWorkspaceService`**

Interface này sẽ định nghĩa các "hợp đồng" về logic nghiệp vụ mà `WorkspaceService` phải thực hiện.
1. Trong project `PronaFlow.Core`, mở thư mục `Interfaces`.
2. Tạo một file interface mới tên là `IWorkspaceService.cs`.
3. Thêm nội dung sau:
    ```C#
    using PronaFlow.Core.DTOs.Workspace;
    using PronaFlow.Core.Models;
    
    namespace PronaFlow.Core.Interfaces;
    
    public interface IWorkspaceService
    {
        Task<IEnumerable<WorkspaceDto>> GetWorkspacesForUserAsync(long userId);
        Task<WorkspaceDto?> GetWorkspaceByIdAsync(long workspaceId, long userId);
        Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceCreateDto workspaceDto, long userId);
        Task<bool> UpdateWorkspaceAsync(long workspaceId, WorkspaceCreateDto workspaceDto, long userId);
        Task<(bool Success, string? Error)> DeleteWorkspaceAsync(long workspaceId, long userId);
    }
    ```
    
    _Lưu ý: Chúng ta luôn truyền `userId` vào các phương thức này để có thể kiểm tra quyền sở hữu ở tầng service._
    

### **Bước 3: Triển Khai Logic trong `WorkspaceService`**

Đây là nơi chứa "bộ não" xử lý các nghiệp vụ liên quan đến Workspace.

1. Trong project `PronaFlow.Services`, tạo một file class mới tên là `WorkspaceService.cs`.
2. Thêm nội dung sau. Hãy đọc kỹ các comment để hiểu rõ logic xử lý, đặc biệt là phần kiểm tra quyền và các quy tắc nghiệp vụ.
    ```C#
    using Microsoft.EntityFrameworkCore;
    using PronaFlow.Core.Data;
    using PronaFlow.Core.DTOs.Workspace;
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Core.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    namespace PronaFlow.Services;
    
    public class WorkspaceService : IWorkspaceService
    {
        private readonly PronaFlowDbContext _context;
    
        public WorkspaceService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        public async Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceCreateDto workspaceDto, long userId)
        {
            var workspace = new Workspace
            {
                Name = workspaceDto.Name,
                Description = workspaceDto.Description,
                OwnerId = userId
            };
    
            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();
    
            return new WorkspaceDto
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Description = workspace.Description,
                OwnerId = workspace.OwnerId
            };
        }
    
        public async Task<(bool Success, string? Error)> DeleteWorkspaceAsync(long workspaceId, long userId)
        {
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId && w.OwnerId == userId);
    
            if (workspace == null)
            {
                return (false, "Workspace not found or you don't have permission to delete it.");
            }
    
            // Kiểm tra quy tắc nghiệp vụ: Workspace phải rỗng trước khi xóa
            //
            var hasProjects = await _context.Projects.AnyAsync(p => p.WorkspaceId == workspaceId);
            if (hasProjects)
            {
                return (false, "Workspace is not empty. Please move or delete all projects before deleting the workspace.");
            }
    
            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    
        public async Task<WorkspaceDto?> GetWorkspaceByIdAsync(long workspaceId, long userId)
        {
            var workspace = await _context.Workspaces
                .AsNoTracking() // Dùng AsNoTracking để tăng hiệu suất cho các truy vấn chỉ đọc
                .Where(w => w.Id == workspaceId && w.OwnerId == userId)
                .Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    OwnerId = w.OwnerId
                })
                .FirstOrDefaultAsync();
    
            return workspace;
        }
    
        public async Task<IEnumerable<WorkspaceDto>> GetWorkspacesForUserAsync(long userId)
        {
            return await _context.Workspaces
                .AsNoTracking()
                .Where(w => w.OwnerId == userId)
                .Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    OwnerId = w.OwnerId
                })
                .ToListAsync();
        }
    
        public async Task<bool> UpdateWorkspaceAsync(long workspaceId, WorkspaceCreateDto workspaceDto, long userId)
        {
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId && w.OwnerId == userId);
    
            if (workspace == null)
            {
                return false; // Không tìm thấy hoặc không có quyền
            }
    
            workspace.Name = workspaceDto.Name;
            workspace.Description = workspaceDto.Description;
    
            await _context.SaveChangesAsync();
            return true;
        }
    }
    ```
### **Bước 4: Tạo `WorkspacesController`**

Đây là lớp sẽ tiếp nhận các HTTP request và gọi đến `WorkspaceService`.

1. Trong project `PronaFlow.API`, chuột phải vào thư mục `Controllers` -> `Add` -> `Controller...`.
2. Chọn **"API Controller - Empty"** và đặt tên là `WorkspacesController.cs`.
3. Thêm nội dung sau. Đoạn code này sẽ được bảo vệ bởi `[Authorize]`, nghĩa là người dùng phải gửi kèm một JWT hợp lệ.
    ```C#
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PronaFlow.Core.DTOs.Workspace;
    using PronaFlow.Core.Interfaces;
    using System.Security.Claims;
    
    namespace PronaFlow.API.Controllers;
    
    [Authorize] // YÊU CẦU XÁC THỰC CHO TẤT CẢ CÁC ACTION TRONG CONTROLLER NÀY
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;
    
        public WorkspacesController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }
    
        private long GetCurrentUserId()
        {
            // Lấy User ID từ claim 'NameIdentifier' trong JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                // Tình huống này gần như không xảy ra nếu token hợp lệ
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    
        [HttpGet]
        public async Task<IActionResult> GetUserWorkspaces()
        {
            var userId = GetCurrentUserId();
            var workspaces = await _workspaceService.GetWorkspacesForUserAsync(userId);
            return Ok(workspaces);
        }
    
        [HttpPost]
        public async Task<IActionResult> CreateWorkspace(WorkspaceCreateDto workspaceDto)
        {
            var userId = GetCurrentUserId();
            var createdWorkspace = await _workspaceService.CreateWorkspaceAsync(workspaceDto, userId);
            return CreatedAtAction(nameof(GetUserWorkspaces), new { id = createdWorkspace.Id }, createdWorkspace);
        }
    
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkspace(long id, WorkspaceCreateDto workspaceDto)
        {
            var userId = GetCurrentUserId();
            var success = await _workspaceService.UpdateWorkspaceAsync(id, workspaceDto, userId);
    
            if (!success)
            {
                return NotFound("Workspace not found or permission denied.");
            }
            return NoContent(); // 204 No Content - Thành công
        }
    
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkspace(long id)
        {
            var userId = GetCurrentUserId();
            var (success, error) = await _workspaceService.DeleteWorkspaceAsync(id, userId);
    
            if (!success)
            {
                // Trả về lỗi 404 nếu không tìm thấy, 400 nếu có lỗi nghiệp vụ
                if (error != null && error.Contains("not empty"))
                {
                    return BadRequest(error); // 400 Bad Request
                }
                return NotFound(error); // 404 Not Found
            }
            return NoContent();
        }
    }
    ```
    

### **Bước 5: Đăng Ký Service trong `Program.cs`**

Cuối cùng, đừng quên đăng ký `IWorkspaceService` với DI Container.

1. Mở file `Program.cs`.
2. Thêm dòng sau vào khu vực đăng ký dịch vụ:
    ```C#
    // ...
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Services;
    // ...
    
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IWorkspaceService, WorkspaceService>(); // <-- THÊM DÒNG NÀY
    ```

[[Test CRUD API]]