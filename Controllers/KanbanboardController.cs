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
    public class KanbanboardController : BaseController
    {
        /// <summary>
        /// Authorize Workspace
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        private (ActionResult Error, workspaces Workspace) GetAuthorizedWorkspace(long workspaceId)
        {
            // 1. Kiểm tra đăng nhập
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return (authError, null);

            // 2. Tìm Workspace
            var workspace = _context.workspaces.SingleOrDefault(w => w.id == workspaceId);
            if (workspace == null)
            {
                return (HttpNotFound(ErrorList.NoWorkspaceForCurrentUser), null);
            }

            // 3. Kiểm tra quyền sở hữu (Owner)
            if (workspace.owner_id != currentUser.id)
            {
                // Nếu muốn cho phép thành viên truy cập, cần sửa logic ở đây để check bảng workspace_members
                return (new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, ErrorList.WorkspaceNotOwned(workspaceId)), null);
            }

            return (null, workspace);
        }

        /// <summary>
        /// Normalize Status of Project for Database
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string NormalizeStatusForDb(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "not-started";
            var s = status.ToLower().Trim().Replace("-", "");

            // Mapping nhanh các trường hợp
            if (s.Contains("temp")) return "temp";
            if (s.Contains("start")) return "not-started";
            if (s.Contains("progress")) return "in-progress";
            if (s.Contains("review")) return "in-review";
            if (s.Contains("done") || s.Contains("complet")) return "done";

            return "not-started";
        }

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

            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            var today = DateTime.Now.Date;
            var next7 = today.AddDays(7);


            return new KanbanBoardViewModel
            {
                CurrentWorkspaceId = (int)workspace.id,
                WorkspaceName = workspace.name,
                WorkspaceDescription = workspace.description,
                Projects = projectViewModels
            };
        }

        /// <summary>
        /// Mapping
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private KanbanProjectCardViewModel MapToKanbanCardViewModel(projects project)
        {
            int totalTasks = project.tasks?.Count ?? 0;
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
                AvatarUrl = pm.users.avatar_url
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

        //====================================================================================

        /// <summary>
        /// Displayed Kanban Board | Get: `/Kanbanboard`
        /// </summary>
        /// <returns>View Index.cshtml</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId, int? openProjectId)
        {
            LogConsole("Index", $"Accessing KanbanBoard. WorkspaceId: {workspaceId}");

            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return authError;

            // Chọn workspace mặc định theo owner; hoặc kiểm tra workspaceId truyền vào
            long targetWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : _context.workspaces
                          .Where(w => w.owner_id == currentUser.id)
                          .Select(w => w.id)
                          .FirstOrDefault();

            if (targetWorkspaceId == 0)
            {
                var defaultWs = _context.workspaces.FirstOrDefault(w => w.owner_id == currentUser.id);
                if (defaultWs == null) {
                    LogConsole("Index", "No default workspace found.", true);
                    return HttpNotFound(ErrorList.NoWorkspaceForCurrentUser);
                }
                targetWorkspaceId = defaultWs.id;
            }

            // Chỉ cho phép vào workspace thuộc user
            var (wsError, workspace) = GetAuthorizedWorkspace(targetWorkspaceId);
            if (wsError != null) return wsError;

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
            LogConsole("UpdateProjectStatus", $"Project: {projectId}, NewStatus: {newStatus}");

            var (authError, currentUser) = GetAuthenticatedUserOrErrorJson();
            if (authError != null) return authError;

            var normalizedStatus = NormalizeStatusForDb(newStatus);
            try
            {
                var project = _context.projects.SingleOrDefault(p => p.id == projectId);
                if (project == null)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = ErrorList.ProjectNotFound 
                    });
                }

                // Kiểm tra quyền: project phải thuộc workspace của user
                var ownsWorkspace = _context.workspaces.Any(w => w.id == project.workspace_id && w.owner_id == currentUser.id);
                if (!ownsWorkspace)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = ErrorList.UnauthorizedUpdateProject
                    });
                }

                project.status = normalizedStatus;
                project.updated_at = DateTime.Now;
                _context.SaveChanges();

                LogConsole("UpdateProjectStatus", "Update success.");

                return Json(new 
                { 
                    success = true, 
                    message = $"Status updated to {newStatus}" 
                });
            }
            catch (Exception ex)
            {
                LogConsole("UpdateProjectStatus", ex.Message, true);

                return Json(new 
                { 
                    success = false, 
                    message = ErrorList.UpdateFailedPrefix + ex.Message
                });
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
            LogConsole("CreateProject", $"Workspace: {workspaceId}, Name: {projectName}");

            var (authError, currentUser) = GetAuthenticatedUserOrErrorJson();
            if (authError != null) return authError;

            // Chỉ cho phép tạo project trong workspace thuộc user
            var ownsWorkspace = _context.workspaces.Any(w => w.id == workspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return Json(new 
                { 
                    success = false, 
                    message = ErrorList.UnauthorizedCreateProject
                });
            }

            try
            {
                var normalizedStatus = NormalizeStatusForDb(initialStatus);
                var now = DateTime.Now;
                var newProject = new projects
                {
                    workspace_id = workspaceId,
                    name = projectName,
                    status = normalizedStatus,
                    created_at = now,
                    updated_at = now,
                    description = "",
                    is_archived = false,
                    is_deleted = false,
                    cover_image_url = "",
                    project_type = ProjectType[0]

                };

                _context.projects.Add(newProject);
                _context.SaveChanges();

                _context.project_members.Add(new project_members
                {
                    project_id = newProject.id,
                    user_id = currentUser.id,
                    role = RoleMember[0]
                });
                _context.SaveChanges();

                var projectViewModel = MapToKanbanCardViewModel(newProject);

                LogConsole("CreateProject", $"Created Project ID: {newProject.id}");

                return Json(new { success = true, project = projectViewModel });
            }
            catch (Exception ex)
            {
                return Json(new 
                { 
                    success = false, 
                    message = ErrorList.CreateFailedPrefix + ex.Message 
                });
            }
        }        
    }
}


