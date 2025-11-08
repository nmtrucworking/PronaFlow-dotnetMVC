# 1. **Triển khai API Quản lý Dự án (Projects):** 
Người dùng cần có một dự án trước khi có thể tạo ra các công việc con.
### **Bước 1: Tạo Các Lớp DTO (Data Transfer Objects) cho Project**
Các lớp này sẽ định hình dữ liệu cho việc tạo, cập nhật và hiển thị Project.

1. Trong project `PronaFlow.Core`, tạo một thư mục mới bên trong `DTOs` tên là `Project`.
2. Tạo 3 file C# sau bên trong thư mục `PronaFlow.Core/DTOs/Project`:
    **`ProjectDto.cs` (Dùng để trả về dữ liệu)**
    ```C#
    namespace PronaFlow.Core.DTOs.Project;
    
    public class ProjectDto
    {
        public long Id { get; set; }
        public long WorkspaceId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; }
        public string ProjectType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
    ```
    
    **`ProjectCreateDto.cs` (Dùng để nhận dữ liệu khi tạo mới)**
    
    
    
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Project;
    
    public class ProjectCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    
        // Status sẽ được truyền vào từ context của Kanban board
        [Required]
        public string Status { get; set; }
    }
    ```
    
    **`ProjectUpdateDto.cs` (Dùng để nhận dữ liệu khi cập nhật)**
    
    
    
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Project;
    
    public class ProjectUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        [Required]
        public string Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
    ```
    

### **Bước 2: Định Nghĩa Interface `IProjectService`**

1. Trong `PronaFlow.Core/Interfaces`, tạo file `IProjectService.cs`.
    
2. Thêm nội dung sau:
    
    
    
    ```C#
    using PronaFlow.Core.DTOs.Project;
    
    namespace PronaFlow.Core.Interfaces;
    
    public interface IProjectService
    {
        Task<ProjectDto> CreateProjectAsync(long workspaceId, ProjectCreateDto projectDto, long creatorId);
        Task<IEnumerable<ProjectDto>> GetProjectsByWorkspaceAsync(long workspaceId, long userId);
        Task<ProjectDto?> GetProjectByIdAsync(long projectId, long userId);
        Task<bool> UpdateProjectAsync(long projectId, ProjectUpdateDto projectDto, long userId);
        Task<bool> SoftDeleteProjectAsync(long projectId, long userId);
    }
    ```
### **Bước 3: Triển Khai Logic trong `ProjectService`**
Đây là nơi chúng ta sẽ cài đặt các quy tắc nghiệp vụ từ tài liệu của bạn.

1. Trong project `PronaFlow.Services`, tạo file class `ProjectService.cs`.
2. Thêm nội dung sau. Logic này bao gồm việc kiểm tra quyền truy cập, tự động thêm người tạo làm admin dự án, và xóa mềm.
    ```C#
    // File: PronaFlow.Services/ProjectService.cs

using Microsoft.EntityFrameworkCore;
using PronaFlow.Core.Data;
using PronaFlow.Core.DTOs.Project;
using PronaFlow.Core.Interfaces;
using PronaFlow.Core.Models;
using System.Security;

namespace PronaFlow.Services;

public class ProjectService : IProjectService
{
    private readonly PronaFlowDbContext _context;

    public ProjectService(PronaFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> CreateProjectAsync(long workspaceId, ProjectCreateDto projectDto, long creatorId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.OwnerId == creatorId);

        if (workspace == null)
        {
            throw new SecurityException("Workspace not found or permission denied.");
        }

        var project = new Project
        {
            WorkspaceId = workspaceId,
            Name = projectDto.Name,
            Status = projectDto.Status,
            ProjectType = "personal", // Mặc định khi tạo là 'personal'
            IsArchived = false,
            IsDeleted = false
        };

        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync(); 

        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = creatorId,
            Role = "admin" // Người tạo dự án mặc định là admin
        };

        await _context.ProjectMembers.AddAsync(projectMember);
        await _context.SaveChangesAsync();

        return new ProjectDto 
        {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Status = project.Status,
            ProjectType = project.ProjectType
        };
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByWorkspaceAsync(long workspaceId, long userId)
    {
        return await _context.Projects
            .Where(p => p.WorkspaceId == workspaceId && p.IsDeleted == false && p.ProjectMembers.Any(pm => pm.UserId == userId))
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                Name = p.Name,
                Description = p.Description,
                CoverImageUrl = p.CoverImageUrl,
                Status = p.Status,
                ProjectType = p.ProjectType,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
            .ToListAsync();
    }

    // ================== TRIỂN KHAI PHƯƠNG THỨC ==================
    public async Task<ProjectDto?> GetProjectByIdAsync(long projectId, long userId)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId && p.IsDeleted == false && p.ProjectMembers.Any(pm => pm.UserId == userId))
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                Name = p.Name,
                Description = p.Description,
                CoverImageUrl = p.CoverImageUrl,
                Status = p.Status,
                ProjectType = p.ProjectType,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
            .FirstOrDefaultAsync();
    }

    // ================== TRIỂN KHAI PHƯƠNG THỨC ==================
    public async Task<bool> UpdateProjectAsync(long projectId, ProjectUpdateDto projectDto, long userId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers) // Include ProjectMembers để kiểm tra quyền
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return false; // Không tìm thấy project
        }

        // Kiểm tra quyền: người dùng phải là admin của dự án mới được cập nhật
        var isUserAdmin = project.ProjectMembers.Any(pm => pm.UserId == userId && pm.Role == "admin");
        if (!isUserAdmin)
        {
            throw new SecurityException("Permission denied to update this project.");
        }

        // Cập nhật các thuộc tính
        project.Name = projectDto.Name;
        project.Description = projectDto.Description;
        project.CoverImageUrl = projectDto.CoverImageUrl;
        project.Status = projectDto.Status;
        project.StartDate = projectDto.StartDate;
        project.EndDate = projectDto.EndDate;
        
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteProjectAsync(long projectId, long userId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return false;

        var member = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId);
        if (member == null || member.Role != "admin")
        {
            throw new SecurityException("Permission denied to delete this project.");
        }

        project.IsDeleted = true; 
        project.DeletedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }
}
    ```
### **Bước 4: Tạo `ProjectsController`**

1. Trong `PronaFlow.API/Controllers`, tạo một API Controller rỗng tên là `ProjectsController.cs`.
2. Thêm nội dung sau. Chú ý cách chúng ta định nghĩa route lồng nhau (`workspaces/{workspaceId}/projects`) để thể hiện rõ mối quan hệ cha-con.
    ```C#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PronaFlow.Core.DTOs.Project;
using PronaFlow.Core.Interfaces;
using System.Security.Claims;

namespace PronaFlow.API.Controllers;

[Authorize]
[Route("api/workspaces/{workspaceId}/projects")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("User ID not found in token.");
        }
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects(long workspaceId)
    {
        var userId = GetCurrentUserId();
        var projects = await _projectService.GetProjectsByWorkspaceAsync(workspaceId, userId);
        return Ok(projects);
    }

    // ================== THÊM ENDPOINT MỚI ==================
    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectById(long workspaceId, long projectId)
    {
        // Tham số workspaceId vẫn có ở đây để giữ cấu trúc route,
        // nhưng logic kiểm tra quyền đã nằm trong service dựa trên projectId và userId.
        var userId = GetCurrentUserId();
        var project = await _projectService.GetProjectByIdAsync(projectId, userId);

        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(long workspaceId, ProjectCreateDto projectDto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var createdProject = await _projectService.CreateProjectAsync(workspaceId, projectDto, userId);
            // Trả về action GetProjectById để client có thể lấy thông tin chi tiết project vừa tạo
            return CreatedAtAction(nameof(GetProjectById), new { workspaceId = workspaceId, projectId = createdProject.Id }, createdProject);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ================== THÊM ENDPOINT MỚI ==================
    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(long workspaceId, long projectId, ProjectUpdateDto projectDto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var success = await _projectService.UpdateProjectAsync(projectId, projectDto, userId);
            if (!success)
            {
                return NotFound("Project not found.");
            }
            return NoContent(); // Thành công
        }
        catch (SecurityException ex)
        {
            return Forbid(ex.Message); // Trả về lỗi 403 Forbidden nếu không có quyền
        }
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(long workspaceId, long projectId)
    {
        var userId = GetCurrentUserId();
        try
        {
            var success = await _projectService.SoftDeleteProjectAsync(projectId, userId);
            if (!success)
            {
                return NotFound("Project not found.");
            }
            return NoContent();
        }
        catch (SecurityException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
    ```
[[Test Endpoint  - Project]]
### **Bước 5: Đăng Ký Service trong `Program.cs`**

1. Mở file `Program.cs`.
2. Thêm dòng đăng ký cho `IProjectService`:
    ```C#
    // ...
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
    builder.Services.AddScoped<IProjectService, ProjectService>(); // <-- THÊM DÒNG NÀY
    ```
# 2. **Triển khai API Quản lý Giai đoạn (Task Lists / Phases):** 
Mỗi dự án sẽ cần có các cột trạng thái (ví dụ: To Do, In Progress, Done) để chứa các công việc.
### **Bước 1: Tạo Các Lớp DTO (Data Transfer Objects) cho TaskList**

1. Trong project `PronaFlow.Core`, tạo một thư mục mới bên trong `DTOs` tên là `TaskList`.
2. Tạo 2 file C# sau bên trong thư mục `PronaFlow.Core/DTOs/TaskList`:
    **`TaskListDto.cs` (Dùng để trả về dữ liệu)**
    ```C#
    namespace PronaFlow.Core.DTOs.TaskList;
    
    public class TaskListDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
    }
    ```
    
    **`TaskListCreateDto.cs` (Dùng để nhận dữ liệu khi tạo mới)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.TaskList;
    
    public class TaskListCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
    ```
### **Bước 2**: Định Nghĩa Interface `ITaskListService`

1. Trong `PronaFlow.Core/Interfaces`, tạo file `ITaskListService.cs`.
2. Thêm nội dung sau:
    ```C#
    using PronaFlow.Core.DTOs.TaskList;
    
    namespace PronaFlow.Core.Interfaces;
    
    public interface ITaskListService
    {
        Task<IEnumerable<TaskListDto>> GetTaskListsForProjectAsync(long projectId, long userId);
        Task<TaskListDto> CreateTaskListAsync(long projectId, TaskListCreateDto dto, long userId);
        Task<bool> DeleteTaskListAsync(long taskListId, long userId);
        // Các phương thức Update sẽ được thêm sau
    }
    ```
### **Bước 3: Triển Khai Logic trong `TaskListService`**
Service này sẽ chứa các quy tắc nghiệp vụ quan trọng, chẳng hạn như tự động tính toán vị trí (`position`) và kiểm tra quyền hạn.

1. Trong project `PronaFlow.Services`, tạo file class `TaskListService.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.EntityFrameworkCore;
    using PronaFlow.Core.Data;
    using PronaFlow.Core.DTOs.TaskList;
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Core.Models;
    using System.Security;
    
    namespace PronaFlow.Services;
    
    public class TaskListService : ITaskListService
    {
        private readonly PronaFlowDbContext _context;
    
        public TaskListService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        // Helper private để kiểm tra quyền thành viên của user trong một project
        private async Task CheckProjectMembershipAsync(long projectId, long userId)
        {
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    
            if (!isMember)
            {
                throw new SecurityException("User is not a member of this project.");
            }
        }
    
        public async Task<TaskListDto> CreateTaskListAsync(long projectId, TaskListCreateDto dto, long userId)
        {
            await CheckProjectMembershipAsync(projectId, userId);
    
            // Kiểm tra tên giai đoạn không được trùng trong cùng một dự án
            var nameExists = await _context.TaskLists
                .AnyAsync(tl => tl.ProjectId == projectId && tl.Name == dto.Name);
            if (nameExists)
            {
                throw new InvalidOperationException("A task list with this name already exists in the project.");
            }
    
            // Tự động tính toán vị trí cho giai đoạn mới
            var maxPosition = await _context.TaskLists
                .Where(tl => tl.ProjectId == projectId)
                .Select(tl => (int?)tl.Position) // Ép kiểu sang int? để có thể trả về null nếu không có record nào
                .MaxAsync();
    
            var newPosition = (maxPosition ?? -1) + 1;
    
            var taskList = new TaskList
            {
                ProjectId = projectId,
                Name = dto.Name,
                Position = newPosition
            };
    
            await _context.TaskLists.AddAsync(taskList);
            await _context.SaveChangesAsync();
    
            return new TaskListDto
            {
                Id = taskList.Id,
                ProjectId = taskList.ProjectId,
                Name = taskList.Name,
                Position = taskList.Position
            };
        }
    
        public async Task<bool> DeleteTaskListAsync(long taskListId, long userId)
        {
            var taskList = await _context.TaskLists
                .Include(tl => tl.Tasks) // Include Tasks để kiểm tra
                .FirstOrDefaultAsync(tl => tl.Id == taskListId);
    
            if (taskList == null) return false;
    
            await CheckProjectMembershipAsync(taskList.ProjectId, userId);
    
            // Quy tắc nghiệp vụ: Không cho xóa nếu giai đoạn vẫn còn công việc
            if (taskList.Tasks.Any())
            {
                throw new InvalidOperationException("Cannot delete a task list that contains tasks. Please move or delete all tasks first.");
            }
    
            _context.TaskLists.Remove(taskList);
            return await _context.SaveChangesAsync() > 0;
        }
    
        public async Task<IEnumerable<TaskListDto>> GetTaskListsForProjectAsync(long projectId, long userId)
        {
            await CheckProjectMembershipAsync(projectId, userId);
    
            return await _context.TaskLists
                .Where(tl => tl.ProjectId == projectId)
                .OrderBy(tl => tl.Position) // Sắp xếp theo vị trí
                .Select(tl => new TaskListDto
                {
                    Id = tl.Id,
                    ProjectId = tl.ProjectId,
                    Name = tl.Name,
                    Position = tl.Position
                })
                .ToListAsync();
        }
    }
    ```
### **Bước 4: Tạo `TaskListsController`**
Controller này sẽ được đặt lồng bên trong route của `Project`.

1. Trong `PronaFlow.API/Controllers`, tạo một API Controller rỗng tên là `TaskListsController.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PronaFlow.Core.DTOs.TaskList;
    using PronaFlow.Core.Interfaces;
    using System.Security.Claims;
    
    namespace PronaFlow.API.Controllers;
    
    [Authorize]
    [Route("api/projects/{projectId}/tasklists")]
    [ApiController]
    public class TaskListsController : ControllerBase
    {
        private readonly ITaskListService _taskListService;
    
        public TaskListsController(ITaskListService taskListService)
        {
            _taskListService = taskListService;
        }
    
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    
        [HttpGet]
        public async Task<IActionResult> GetTaskLists(long projectId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var taskLists = await _taskListService.GetTaskListsForProjectAsync(projectId, userId);
                return Ok(taskLists);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> CreateTaskList(long projectId, TaskListCreateDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var createdTaskList = await _taskListService.CreateTaskListAsync(projectId, dto, userId);
                return Ok(createdTaskList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpDelete("{taskListId}")]
        public async Task<IActionResult> DeleteTaskList(long projectId, long taskListId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskListService.DeleteTaskListAsync(taskListId, userId);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    ```
### **Bước 5: Đăng Ký Service trong `Program.cs`**

1. Mở file `Program.cs`.
2. Thêm dòng đăng ký cho `ITaskListService`:
    ```C#
    // ...
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ITaskListService, TaskListService>(); // <-- THÊM DÒNG NÀY
    ```
# 3. **Triển khai API Quản lý Công việc (Tasks):** 
Cuối cùng, chúng ta sẽ xây dựng các API cho các công việc chi tiết.
### **Bước 1: Tạo Các Lớp DTO (Data Transfer Objects) cho Task**

1. Trong project `PronaFlow.Core`, tạo một thư mục mới bên trong `DTOs` tên là `Task`.
2. Tạo 3 file C# sau bên trong thư mục `PronaFlow.Core/DTOs/Task`:
    
    **`TaskDto.cs` (Dùng để trả về dữ liệu chi tiết)**
    ```C#
    namespace PronaFlow.Core.DTOs.Task;
    
    public class TaskDto
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long? TaskListId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? CreatorId { get; set; }
        // Thêm các trường khác nếu cần
    }
    ```
    
    **`TaskCreateDto.cs` (Dùng để nhận dữ liệu khi tạo mới)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Task;
    
    public class TaskCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    
        public string? Description { get; set; }
    
        public string? Priority { get; set; } = "normal";
    
        // Danh sách ID của những người được giao việc
        public List<long>? AssigneeIds { get; set; }
    
        public DateOnly? StartDate { get; set; }
    
        public DateTime? EndDate { get; set; }
    }
    ```
    
    **`TaskUpdateDto.cs` (Dùng để nhận dữ liệu khi cập nhật)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Task;
    
    public class TaskUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public List<long>? AssigneeIds { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    ```
### **Bước 2: Định Nghĩa Interface `ITaskService`**

1. Trong `PronaFlow.Core/Interfaces`, tạo file `ITaskService.cs`.
2. Thêm nội dung sau:
    ```C#
    using PronaFlow.Core.DTOs.Task;
    
    namespace PronaFlow.Core.Interfaces;
    
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksForProjectAsync(long projectId, long userId);
        Task<TaskDto> CreateTaskAsync(long taskListId, TaskCreateDto dto, long creatorId);
        Task<bool> SoftDeleteTaskAsync(long taskId, long userId);
        // Các phương thức khác sẽ được triển khai sau
    }
    ```
### **Bước 3: Triển Khai Logic trong `TaskService`**

Service này sẽ xử lý việc tạo Task, gán người thực hiện, và các quy tắc nghiệp vụ liên quan.

1. Trong project `PronaFlow.Services`, tạo file class `TaskService.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.EntityFrameworkCore;
    using PronaFlow.Core.Data;
    using PronaFlow.Core.DTOs.Task;
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Core.Models;
    using System.Security;
    
    namespace PronaFlow.Services;
    
    public class TaskService : ITaskService
    {
        private readonly PronaFlowDbContext _context;
    
        public TaskService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        private async Task CheckProjectMembershipAsync(long projectId, long userId)
        {
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    
            if (!isMember)
            {
                throw new SecurityException("User is not a member of this project.");
            }
        }
    
        public async Task<TaskDto> CreateTaskAsync(long taskListId, TaskCreateDto dto, long creatorId)
        {
            var taskList = await _context.TaskLists.FindAsync(taskListId);
            if (taskList == null)
            {
                throw new KeyNotFoundException("TaskList not found.");
            }
    
            await CheckProjectMembershipAsync(taskList.ProjectId, creatorId);
    
            var task = new PronaTask
            {
                ProjectId = taskList.ProjectId,
                TaskListId = taskListId,
                Name = dto.Name,
                Description = dto.Description,
                Priority = dto.Priority,
                CreatorId = creatorId,
                Status = "not-started", // Trạng thái mặc định khi tạo
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
    
            // Xử lý gán người thực hiện (Assignees)
            if (dto.AssigneeIds != null && dto.AssigneeIds.Any())
            {
                var assignees = await _context.Users
                    .Where(u => dto.AssigneeIds.Contains(u.Id))
                    .ToListAsync();
                task.Users = assignees; // Gán danh sách user vào navigation property
            }
    
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
    
            return new TaskDto 
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                TaskListId = task.TaskListId,
                Name = task.Name,
                Description = task.Description,
                Priority = task.Priority,
                Status = task.Status,
                CreatorId = task.CreatorId
            };
        }
    
        public async Task<bool> SoftDeleteTaskAsync(long taskId, long userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;
    
            await CheckProjectMembershipAsync(task.ProjectId, userId);
    
            task.IsDeleted = true; // Thực hiện xóa mềm
            task.DeletedAt = DateTime.UtcNow;
    
            return await _context.SaveChangesAsync() > 0;
        }
    
        public async Task<IEnumerable<TaskDto>> GetTasksForProjectAsync(long projectId, long userId)
        {
            await CheckProjectMembershipAsync(projectId, userId);
    
            return await _context.Tasks
                .Where(t => t.ProjectId == projectId && t.IsDeleted == false)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    TaskListId = t.TaskListId,
                    Name = t.Name,
                    Status = t.Status,
                    Priority = t.Priority
                })
                .ToListAsync();
        }
    }
    ```
### **Bước 4: Tạo `TasksController`**

1. Trong `PronaFlow.API/Controllers`, tạo một API Controller rỗng tên là `TasksController.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PronaFlow.Core.DTOs.Task;
    using PronaFlow.Core.Interfaces;
    using System.Security.Claims;
    
    namespace PronaFlow.API.Controllers;
    
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
    
        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }
    
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    
        [HttpGet("projects/{projectId}/tasks")]
        public async Task<IActionResult> GetTasks(long projectId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _taskService.GetTasksForProjectAsync(projectId, userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpPost("tasklists/{taskListId}/tasks")]
        public async Task<IActionResult> CreateTask(long taskListId, TaskCreateDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var createdTask = await _taskService.CreateTaskAsync(taskListId, dto, userId);
                return Ok(createdTask);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpDelete("tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask(long taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskService.SoftDeleteTaskAsync(taskId, userId);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
    ```
### **Bước 5: Đăng Ký Service trong `Program.cs`**

1. Mở file `Program.cs`.
2. Thêm dòng đăng ký cho `ITaskService`:
    ```C#
    // ...
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ITaskListService, TaskListService>();
    builder.Services.AddScoped<ITaskService, TaskService>(); // <-- THÊM DÒNG NÀY
    ```