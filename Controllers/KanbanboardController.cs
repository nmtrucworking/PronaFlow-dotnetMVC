using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;


namespace PronaFlow_MVC.Controllers
{
    //[Authentication
    public class KanbanboardController : Controller
    {
        private readonly PronaFlow_DBContext _context = new PronaFlow_DBContext();

        /// <summary>
        /// Displayed Kanban Board | Get: `/Kanbanboard`
        /// </summary>
        /// <returns>View Index.cshtml</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId, int? openProjectId)
        {
            // Lấy user hiện tại theo email
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Chọn workspace mặc định theo owner; hoặc kiểm tra workspaceId truyền vào
            long targetWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : _context.workspaces
                          .Where(w => w.owner_id == currentUser.id)
                          .Select(w => w.id)
                          .FirstOrDefault();

            if (targetWorkspaceId == 0)
            {
                return HttpNotFound("Không tìm thấy workspace cho người dùng hiện tại.");
            }

            // Chỉ cho phép vào workspace thuộc user
            var belongsToUser = _context.workspaces.Any(w => w.id == targetWorkspaceId && w.owner_id == currentUser.id);
            if (!belongsToUser)
            {
                return HttpNotFound($"Workspace ID {targetWorkspaceId} không thuộc quyền của người dùng hiện tại.");
            }

            var viewModel = GetKanbanBoardData((int)targetWorkspaceId);
            ViewBag.OpenProjectId = openProjectId;
            ViewBag.CurrentWorkspaceId = (int)targetWorkspaceId;
            return View(viewModel);
        }

        /// <summary>
        /// Update Project Status | Post: `/Kanbanboard/UpdateProjectStatus`
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="newStatus"></param>
        /// <returns>Json and SaveChanges</returns>
        [HttpPost]
        public ActionResult UpdateProjectStatus(int projectId, string newStatus)
        {
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            var normalizedStatus = NormalizeStatusForDb(newStatus);
            try
            {
                var project = _context.projects.SingleOrDefault(p => p.id == projectId);
                if (project == null)
                {
                    return Json(new { success = false, message = "Project không tồn tại." });
                }

                // Kiểm tra quyền: project phải thuộc workspace của user
                var ownsWorkspace = _context.workspaces.Any(w => w.id == project.workspace_id && w.owner_id == currentUser.id);
                if (!ownsWorkspace)
                {
                    return Json(new { success = false, message = "Không có quyền cập nhật project ở workspace này." });
                }

                project.status = normalizedStatus;
                _context.SaveChanges();

                return Json(new { success = true, message = $"Status updated to {newStatus}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + ex.Message });
            }
        }

        /// <summary>
        /// Create Project and displayed project card if success
        /// | POST: `/Kanbanboard/CreateProject`
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="projectName"></param>
        /// <param name="initialStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateProject(int workspaceId, string projectName, string initialStatus)
        {
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            // Chỉ cho phép tạo project trong workspace thuộc user
            var ownsWorkspace = _context.workspaces.Any(w => w.id == workspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return Json(new { success = false, message = "Không có quyền tạo project trong workspace này." });
            }

            var normalizedStatus = NormalizeStatusForDb(initialStatus);
            try
            {
                var newProject = new projects
                {
                    workspace_id = workspaceId,
                    name = projectName,
                    status = normalizedStatus,
                    created_at = DateTime.Now,
                    description = "",
                    is_archived = false,
                    is_deleted = false,
                    updated_at = DateTime.Now,
                    cover_image_url = ""
                };

                _context.projects.Add(newProject);
                _context.SaveChanges();

                var projectViewModel = MapToKanbanCardViewModel(newProject);

                return Json(new { success = true, project = projectViewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Create failed: " + ex.Message });
            }
        }

        //========================== HELPER METHODS //==========================

        /// <summary>
        /// Helper Method to Get Kanban Board Data
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        private KanbanBoardViewModel GetKanbanBoardData(int workspaceId)
        {
            var workspace = _context.workspaces.SingleOrDefault(w => w.id == workspaceId);

            if (workspace == null)
            {
                return new KanbanBoardViewModel
                {
                    WorkspaceName = "Workspace Not Found",
                    WorkspaceDescription = "",
                    Projects = new List<KanbanProjectCardViewModel>()
                };
            }

            var projectsInWorkspace = _context.projects
                .Include("tags")
                .Include("project_members.users")
                .Include("tasks")
                .Where(p => p.workspace_id == workspaceId &&
                             (p.is_deleted == false))
                .ToList();

            var projectViewModels = projectsInWorkspace
                                    .Select(MapToKanbanCardViewModel)
                                    .ToList();
            // Map projects to KanbanProjectCardViewModel
            return new KanbanBoardViewModel
            {
                CurrentWorkspaceId = (int)workspace.id,
                WorkspaceName = workspace.name,
                WorkspaceDescription = workspace.description,
                Projects = projectViewModels
            };
        }

        private KanbanProjectCardViewModel MapToKanbanCardViewModel(projects project)
        {
            // Cần đảm bảo các trường của project.tasks, project.tags, project.project_members có dữ liệu
            // nếu không Entity Framework có thể trả về null, gây ra lỗi NullReference.

            int totalTasks = project.tasks?.Count ?? 0;
            // Giả định trường status của task là 'status' và status hoàn thành là 'completed'
            int completedTasks = project.tasks?.Count(t => t.status != null && t.status.ToLower() == "completed") ?? 0;

            int remainingDays = 0;
            if (project.end_date.HasValue)
            {
                remainingDays = (int)Math.Ceiling((project.end_date.Value - DateTime.Now).TotalDays);
            }

            var tags = project.tags?.Select(t => new ProjectTagViewModel
            {
                Id = (int)t.id,
                ColorHex = t.color_hex,
                Name = t.name
            }).ToList() ?? new List<ProjectTagViewModel>();

            var members = project.project_members?.Select(pm => new ProjectMemberViewModel
            {
                UserId = (int)pm.user_id,
                AvatarUrl = pm.users.avatar_url // Giả định project_members có navigation property users
            }).ToList() ?? new List<ProjectMemberViewModel>();

            return new KanbanProjectCardViewModel
            {
                Id = (int)project.id,
                Name = project.name,
                Status = project.status,
                StartDate = project.start_date,
                EndDate = project.end_date,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                Tags = tags,
                Members = members,
                IsCompleted = (project.status != null && (project.status.ToLower() == "completed" || project.status.ToLower() == "done")),
                RemainingDays = remainingDays
            };
        }

        private string NormalizeStatusForDb(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "unknown";
            var s = status.ToLower().Trim();
            return s.Replace("-", "");
        }
    }
}


