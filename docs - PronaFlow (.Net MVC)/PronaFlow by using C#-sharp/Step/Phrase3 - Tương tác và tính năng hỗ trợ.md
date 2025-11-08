# 1. Subtask
### **Bước 1: Tạo Các Lớp DTO (Data Transfer Objects) cho Subtask**

1. Trong project `PronaFlow.Core`, tạo một thư mục mới bên trong `DTOs` tên là `Subtask`.
2. Tạo 3 file C# sau bên trong thư mục `PronaFlow.Core/DTOs/Subtask`:
    
    **`SubtaskDto.cs` (Dùng để trả về dữ liệu)**
    ```C#
    namespace PronaFlow.Core.DTOs.Subtask;
    
    public class SubtaskDto
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public int Position { get; set; }
    }
    ```
    
    **`SubtaskCreateDto.cs` (Dùng để nhận dữ liệu khi tạo mới)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Subtask;
    
    public class SubtaskCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
    ```
    
    **`SubtaskUpdateDto.cs` (Dùng để nhận dữ liệu khi cập nhật)**
    ```C#
    using System.ComponentModel.DataAnnotations;
    
    namespace PronaFlow.Core.DTOs.Subtask;
    
    public class SubtaskUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    
        [Required]
        public bool IsCompleted { get; set; }
    }
    ```
### **Bước 2: Định Nghĩa Interface `ISubtaskService`**

1. Trong `PronaFlow.Core/Interfaces`, tạo file `ISubtaskService.cs`.
2. Thêm nội dung sau:
    ```C#
    using PronaFlow.Core.DTOs.Subtask;
    
    namespace PronaFlow.Core.Interfaces;
    
    public interface ISubtaskService
    {
        Task<IEnumerable<SubtaskDto>> GetSubtasksForTaskAsync(long taskId, long userId);
        Task<SubtaskDto> CreateSubtaskAsync(long taskId, SubtaskCreateDto dto, long userId);
        Task<bool> UpdateSubtaskAsync(long subtaskId, SubtaskUpdateDto dto, long userId);
        Task<bool> DeleteSubtaskAsync(long subtaskId, long userId);
    }
    ```
### **Bước 3: Triển Khai Logic trong `SubtaskService`**

Service này sẽ chịu trách nhiệm kiểm tra quyền truy cập vào công việc cha (`PronaTask`) trước khi cho phép thao tác trên công việc con.

1. Trong project `PronaFlow.Services`, tạo file class `SubtaskService.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.EntityFrameworkCore;
    using PronaFlow.Core.Data;
    using PronaFlow.Core.DTOs.Subtask;
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Core.Models;
    using System.Security;
    
    namespace PronaFlow.Services;
    
    public class SubtaskService : ISubtaskService
    {
        private readonly PronaFlowDbContext _context;
    
        public SubtaskService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        // Helper để kiểm tra quyền truy cập vào công việc cha
        private async Task CheckParentTaskAccessAsync(long taskId, long userId)
        {
            var canAccess = await _context.Tasks
                .Where(t => t.Id == taskId)
                .AnyAsync(t => t.Project.ProjectMembers.Any(pm => pm.UserId == userId));
    
            if (!canAccess)
            {
                throw new SecurityException("Permission denied to access this task.");
            }
        }
    
        public async Task<SubtaskDto> CreateSubtaskAsync(long taskId, SubtaskCreateDto dto, long userId)
        {
            await CheckParentTaskAccessAsync(taskId, userId);
    
            var maxPosition = await _context.Subtasks
                .Where(st => st.TaskId == taskId)
                .Select(st => (int?)st.Position)
                .MaxAsync() ?? -1;
    
            var subtask = new Subtask
            {
                TaskId = taskId,
                Name = dto.Name,
                IsCompleted = false,
                Position = maxPosition + 1
            };
    
            await _context.Subtasks.AddAsync(subtask);
            await _context.SaveChangesAsync();
    
            return new SubtaskDto { /* ... map properties ... */ };
        }
    
        public async Task<bool> DeleteSubtaskAsync(long subtaskId, long userId)
        {
            var subtask = await _context.Subtasks.FindAsync(subtaskId);
            if (subtask == null) return false;
    
            await CheckParentTaskAccessAsync(subtask.TaskId, userId);
    
            _context.Subtasks.Remove(subtask);
            return await _context.SaveChangesAsync() > 0;
        }
    
        public async Task<IEnumerable<SubtaskDto>> GetSubtasksForTaskAsync(long taskId, long userId)
        {
            await CheckParentTaskAccessAsync(taskId, userId);
    
            return await _context.Subtasks
                .Where(st => st.TaskId == taskId)
                .OrderBy(st => st.Position)
                .Select(st => new SubtaskDto
                {
                    Id = st.Id,
                    TaskId = st.TaskId,
                    Name = st.Name,
                    IsCompleted = st.IsCompleted,
                    Position = st.Position
                })
                .ToListAsync();
        }
    
        public async Task<bool> UpdateSubtaskAsync(long subtaskId, SubtaskUpdateDto dto, long userId)
        {
            var subtask = await _context.Subtasks.FindAsync(subtaskId);
            if (subtask == null) return false;
    
            await CheckParentTaskAccessAsync(subtask.TaskId, userId);
    
            subtask.Name = dto.Name;
            subtask.IsCompleted = dto.IsCompleted;
    
            return await _context.SaveChangesAsync() > 0;
        }
    }
    ```
### **Bước 4: Tạo `SubtasksController`**

1. Trong `PronaFlow.API/Controllers`, tạo một API Controller rỗng tên là `SubtasksController.cs`.
2. Thêm nội dung sau:
    ```C#
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PronaFlow.Core.DTOs.Subtask;
    using PronaFlow.Core.Interfaces;
    using System.Security.Claims;
    
    namespace PronaFlow.API.Controllers;
    
    [Authorize]
    [ApiController]
    public class SubtasksController : ControllerBase
    {
        private readonly ISubtaskService _subtaskService;
    
        public SubtasksController(ISubtaskService subtaskService)
        {
            _subtaskService = subtaskService;
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
    
        [HttpGet("api/tasks/{taskId}/subtasks")]
        public async Task<IActionResult> GetSubtasks(long taskId)
        {
            var userId = GetCurrentUserId();
            var subtasks = await _subtaskService.GetSubtasksForTaskAsync(taskId, userId);
            return Ok(subtasks);
        }
    
        [HttpPost("api/tasks/{taskId}/subtasks")]
        public async Task<IActionResult> CreateSubtask(long taskId, SubtaskCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var createdSubtask = await _subtaskService.CreateSubtaskAsync(taskId, dto, userId);
            return Ok(createdSubtask);
        }
    
        [HttpPut("api/subtasks/{subtaskId}")]
        public async Task<IActionResult> UpdateSubtask(long subtaskId, SubtaskUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            var success = await _subtaskService.UpdateSubtaskAsync(subtaskId, dto, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    
        [HttpDelete("api/subtasks/{subtaskId}")]
        public async Task<IActionResult> DeleteSubtask(long subtaskId)
        {
            var userId = GetCurrentUserId();
            var success = await _subtaskService.DeleteSubtaskAsync(subtaskId, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
    ```
### **Bước 5: Đăng Ký Service trong `Program.cs`**

1. Mở file `Program.cs`.
2. Thêm dòng đăng ký cho `ISubtaskService`:
    ```C#
    // ...
    builder.Services.AddScoped<ITaskListService, TaskListService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<ISubtaskService, SubtaskService>(); // <-- THÊM DÒNG NÀY
    ```

# 2. Activity & Notifications System
Đây là tính năng "xương sống" cho việc hợp tác trong ứng dụng của bạn. Mục tiêu của nó là:

1. **Ghi lại (log)** các hành động quan trọng của người dùng (ai đó đã tạo một công việc, ai đó đã thêm bạn vào dự án...).
2. **Tạo thông báo** cho những người dùng liên quan đến hành động đó.

> Thay vì chỉ tạo một controller mới, chúng ta sẽ tạo một **Service trung tâm** và sau đó **tích hợp (weave)** nó vào các service hiện có (`ProjectService`, `TaskService`...).
### **Phần 1: Xây Dựng Service Trung Tâm `ActivityService`**

Chúng ta sẽ tạo một service chuyên dụng chỉ để xử lý việc ghi lại hoạt động và tạo thông báo.

#### **Bước 1: Định Nghĩa Interface `IActivityService`**

1. Trong `PronaFlow.Core/Interfaces`, tạo file `IActivityService.cs`.
2. Thêm nội dung sau:
    ```C#
    namespace PronaFlow.Core.Interfaces;
    
    public interface IActivityService
    {
        Task LogActivityAsync(long userId, string actionType, long targetId, string targetType, string? content = null);
    }
    ```
    
    _Phương thức này đủ linh hoạt để ghi lại bất kỳ loại hoạt động nào._
#### **Bước 2: Triển Khai Logic trong `ActivityService`**

1. Trong `PronaFlow.Services`, tạo file `ActivityService.cs`.
2. Thêm nội dung sau. Service này sẽ tạo bản ghi `Activity` và sau đó (trong tương lai) sẽ xử lý logic tạo `NotificationRecipient` dựa trên `actionType`.
    ```C#
    using PronaFlow.Core.Data;
    using PronaFlow.Core.Interfaces;
    using PronaFlow.Core.Models;
    
    namespace PronaFlow.Services;
    
    public class ActivityService : IActivityService
    {
        private readonly PronaFlowDbContext _context;
    
        public ActivityService(PronaFlowDbContext context)
        {
            _context = context;
        }
    
        public async Task LogActivityAsync(long userId, string actionType, long targetId, string targetType, string? content = null)
        {
            var activity = new Activity
            {
                UserId = userId,
                ActionType = actionType,
                TargetId = targetId,
                TargetType = targetType,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
    
            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();
    
            // TODO: Xử lý logic tạo thông báo (Notification) ở đây
            // Ví dụ: Dựa vào actionType, quyết định ai sẽ nhận được thông báo
            // và tạo bản ghi trong bảng NotificationRecipients.
        }
    }
    ```
#### **Bước 3: Đăng Ký Service trong `Program.cs`**

1. Mở `Program.cs` và thêm dòng đăng ký cho service mới:
    ```C#
    // ...
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<ISubtaskService, SubtaskService>();
    builder.Services.AddScoped<IActivityService, ActivityService>(); // <-- THÊM DÒNG NÀY
    ```
### **Phần 2: Tích Hợp `ActivityService` Vào Các Service Hiện Có**

Bây giờ, chúng ta sẽ "tiêm" `IActivityService` vào các service khác và gọi nó sau khi một hành động thành công.

#### **Ví dụ 1: Tích hợp vào `ProjectService`**

1. Mở `ProjectService.cs`.
2. Tiêm `IActivityService` vào constructor:
    ```C#
    public class ProjectService : IProjectService
    {
        private readonly PronaFlowDbContext _context;
        private readonly IActivityService _activityService; // <-- Thêm dòng này
    
        public ProjectService(PronaFlowDbContext context, IActivityService activityService) // <-- Sửa constructor
        {
            _context = context;
            _activityService = activityService; // <-- Thêm dòng này
        }
        // ...
    }
    ```
    
3. Trong phương thức `CreateProjectAsync`, sau khi đã lưu project thành công, hãy gọi đến `LogActivityAsync`:
    ```C#
    public async Task<ProjectDto> CreateProjectAsync(long workspaceId, ProjectCreateDto projectDto, long creatorId)
    {
        // ... (code tạo project như cũ) ...
    
        await _context.ProjectMembers.AddAsync(projectMember);
        await _context.SaveChangesAsync();
    
        // GHI LẠI HOẠT ĐỘNG
        await _activityService.LogActivityAsync(creatorId, "project_create", project.Id, "project");
    
        return new ProjectDto { /* ... */ };
    }
    ```
#### **Ví dụ 2: Tích hợp vào `TaskService`**

1. Tương tự, hãy tiêm `IActivityService` vào constructor của `TaskService.cs`.
2. Trong phương thức `CreateTaskAsync`, sau khi lưu task thành công, hãy ghi lại hoạt động:
    ```C#
    public async Task<TaskDto> CreateTaskAsync(long taskListId, TaskCreateDto dto, long creatorId)
    {
        // ... (code tạo task và gán assignee như cũ) ...
    
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    
        // GHI LẠI HOẠT ĐỘNG
        await _activityService.LogActivityAsync(creatorId, "task_create", task.Id, "task", dto.Name);
    
        // Nếu có gán người thực hiện, ghi thêm hoạt động "task_assign"
        if (task.Users.Any())
        {
            foreach (var assignee in task.Users)
            {
                await _activityService.LogActivityAsync(creatorId, "task_assign", task.Id, "task", $"{{ \"assignee_id\": {assignee.Id} }}");
            }
        }
    
        return new TaskDto { /* ... */ };
    }
    ```
### **Phần 3: Tạo API Endpoint để xem Hoạt Động**

Cuối cùng, chúng ta cần một API để frontend có thể lấy và hiển thị danh sách các hoạt động này.

1. **Tạo `ActivityDto.cs`** trong `PronaFlow.Core/DTOs/Activity/`:
    ```C#
    namespace PronaFlow.Core.DTOs.Activity;
    
    public class ActivityDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string ActionType { get; set; }
        public long TargetId { get; set; }
        public string TargetType { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    ```
    
2. **Thêm phương thức vào `IActivityService` và `ActivityService`** để lấy danh sách hoạt động:
    - **`IActivityService.cs`:**
        ```C#
        Task<IEnumerable<ActivityDto>> GetActivitiesForTargetAsync(long targetId, string targetType);
        ```
        
    - **`ActivityService.cs`:**
        ```C#
        public async Task<IEnumerable<ActivityDto>> GetActivitiesForTargetAsync(long targetId, string targetType)
        {
            return await _context.Activities
                .Include(a => a.User) // Join với bảng User để lấy tên người thực hiện
                .Where(a => a.TargetId == targetId && a.TargetType == targetType)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserFullName = a.User.FullName,
                    ActionType = a.ActionType,
                    TargetId = a.TargetId,
                    TargetType = a.TargetType,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }
        ```
        
3. **Tạo `ActivitiesController.cs`** trong `PronaFlow.API/Controllers/`:

    ```C#
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PronaFlow.Core.Interfaces;
    
    namespace PronaFlow.API.Controllers;
    
    [Authorize]
    [Route("api/activities")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;
    
        public ActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }
    
        [HttpGet]
        public async Task<IActionResult> GetActivities([FromQuery] long targetId, [FromQuery] string targetType)
        {
            if (targetId <= 0 || string.IsNullOrEmpty(targetType))
            {
                return BadRequest("targetId and targetType are required.");
            }
    
            var activities = await _activityService.GetActivitiesForTargetAsync(targetId, targetType);
            return Ok(activities);
        }
    }
    ```

> **Result!** Xây dựng thành công nền tảng cho hệ thống hoạt động. Giờ đây, mỗi khi một project hoặc task được tạo, hệ thống sẽ tự động ghi lại. Bạn có thể gọi đến API `GET /api/activities?targetType=project&targetId=1` để xem lịch sử hoạt động của project có ID là 1.
# 3.

