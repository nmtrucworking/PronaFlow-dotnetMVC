using System.Configuration.Internal;
using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;

namespace PronaFlow_MVC.Controllers
{
    public class ProjectController : BaseController
    {
        //=========================================================================================
        //===================================== HELPER METHODS ====================================
        //=========================================================================================

        /// <summary>
        /// [HelperMethod] List of allowed status values for project
        /// </summary>
        private static readonly string[] AllowedStatus = new[] { "temp", "not-started", "in-progress", "in-review", "done" };
        
        /// <summary>
        /// [HelperMethod] StatusDisplayName is a dictionary that maps status values to their display names.
        /// </summary>
        private static readonly Dictionary<string, string> StatusDisplayName = new Dictionary<string, string>
        {
            { "temp", "Temp" },
            { "not-started", "Not Started" },
            { "in-progress", "In Progress" },
            { "in-review", "In Review" },
            { "done", "Done" }
        };

        
        /// <summary>
        /// [HelperMethod] Normalize status input to match database values
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Normalized status string</returns>
        private static string NormalizeStatusFor_context(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "not-started";
            var s = status.Trim().ToLower();
            switch (s)
            {
                case "temp":
                case "on-hold":
                    return "temp";
                case "not-started":
                case "not_started":
                    return "not-started";
                case "in-progress":
                case "in_progress":
                    return "in-progress";
                case "in-review":
                case "in_review":
                    return "in-review";
                case "done":
                case "completed":
                    return "done";
                default:
                    return "not-started";
            }
        }

        /// <summary>
        /// [HelperMethod] Authorize user for workspace.
        /// If user is not authorized, return appropriate response.
        /// </summary>
        private (ActionResult Error, workspaces Workspace) GetAuthorizedWorkspace(long workspaceId)
        {
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return (authError, null);

            var workspace = _context.workspaces.SingleOrDefault(w => w.id == workspaceId);
            if (workspace == null)
            {
                return (HttpNotFound("Workspace không tồn tại."), null);
            }

            if (workspace.owner_id != currentUser.id)
            {
                return (new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Workspace không thuộc quyền của bạn."), null);
            }

            return (null, workspace);
        }

        /// <summary>
        /// Check Authorized Project
        /// </summary>
        /// <param name="projectId">Project_ID</param>
        /// <returns>Tuple Pattern</returns>
        private (ActionResult Error, projects Project) GetAuthorizedProject(int projectId)
        {
            // 1. Kiểm tra User
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return (authError, null);

            // 2. Tìm Project
            var project = _context.projects.SingleOrDefault(p => p.id == projectId && !p.is_deleted);
            if (project == null)
            {
                return (HttpNotFound(ErrorList.ProjectNotFound), null);
            }

            // 3. Kiểm tra quyền sở hữu Workspace
            var ownsWorkspace = _context.workspaces.Any(w => w.id == project.workspace_id && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return (new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Không có quyền truy cập."), null);
            }

            // Thành công: Trả về Error = null và Project object
            return (null, project);
        }

        /// <summary>
        /// Map project entity to ProjectDetailsViewModel.
        /// </summary>
        /// <param name="project">Project entity to map</param>
        /// <returns>ProjectDetailsViewModel</returns>
        private ProjectDetailsViewModel MapToProjectDetailsViewModel(projects project)
        {
            var tags = project.tags?.Select(t => new ProjectTagViewModel
            {
                Id = (int)t.id,
                Name = t.name,
                ColorHex = t.color_hex
            }).ToList() ?? new List<ProjectTagViewModel>();

            var members = project.project_members?.Select(m => new ProjectMemberViewModel
            {
                UserId = (int)m.user_id,
                AvatarUrl = m.users.avatar_url
            }).ToList() ?? new List<ProjectMemberViewModel>();

            var tasks = project.tasks?
                .Where(t => !t.is_deleted)
                .Select(t => new TaskItemViewModel
                {
                    Id = t.id,
                    Name = t.name,
                    Status = t.status,
                    Priority = t.priority,
                    DueDate = t.end_date,
                    ProjectName = project.name,
                    TaskListName = t.task_lists != null ? t.task_lists.name : null
                })
                .ToList() ?? new List<TaskItemViewModel>();



            return new ProjectDetailsViewModel
            {
                Id = (int)project.id,
                WorkspaceId = (int)project.workspace_id,
                Name = project.name,
                Description = project.description,
                CoverImageUrl = project.cover_image_url,
                Status = project.status,
                StartDate = project.start_date,
                EndDate = project.end_date,
                Tags = tags,
                Members = members,
                Tasks = tasks
            };
        }

        private ActionResult RedirectToKanban(projects project)
        {
            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = project.workspace_id, openProjectId = project.id });
        }

        //===============================================================================================

        /// <summary>
        /// Get all projects in a workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace ID to get projects for</param>
        /// <returns>ActionResult: View with list of projects</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId)
        {
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return authError;

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : _context.workspaces.Where(w => w.owner_id == currentUser.id).Select(w => w.id).FirstOrDefault();

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound(ErrorList.NoWorkspaceForCurrentUser);
            }

            var (wsError, _) = GetAuthorizedWorkspace(currentWorkspaceId);
            if (wsError != null) return HttpNotFound(ErrorList.WorkspaceNotBelongToYou);

            var projects = _context.projects
                .Where(p => !p.is_deleted && p.workspace_id == currentWorkspaceId)
                .ToList()
                .Select(p => new KanbanProjectCardViewModel
                {
                    Id = (int)p.id,
                    Name = p.name,
                    Status = p.status,
                    StartDate = p.start_date,
                    EndDate = p.end_date,
                    TotalTasks = p.tasks.Count(t => !t.is_deleted),
                    CompletedTasks = p.tasks.Count(t => !t.is_deleted && t.status == "done"),
                    Tags = p.tags.Select(t => new ProjectTagViewModel
                    {
                        Id = (int)t.id,
                        Name = t.name,
                        ColorHex = t.color_hex
                    }),
                    Members = p.project_members.Select(m => new ProjectMemberViewModel
                    {
                        UserId = (int)m.user_id,
                        AvatarUrl = m.users.avatar_url
                    }),
                    IsCompleted = p.status == "done",
                    RemainingDays = p.end_date.HasValue ? (int)(p.end_date.Value - DateTime.Now).TotalDays : 0
                })
                .ToList();

            ViewBag.WorkspaceId = currentWorkspaceId;
            ViewBag.WorkspaceName = _context.workspaces.Where(w => w.id == currentWorkspaceId).Select(w => w.name).FirstOrDefault();

            return View(projects);
        }


        /// <summary>
        /// Get project details partial view.
        /// </summary>
        /// <param name="id">Project ID to get details for</param>
        /// <param name="asPartial"></param>
        /// <returns>ActionResult: Partial view with project details</returns
        [HttpGet]
        public ActionResult DetailsPartial(int id, bool asPartial = true)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            var vm = MapToProjectDetailsViewModel(project);

            var availableTags = _context.tags
                .Where(t => t.workspace_id == project.workspace_id 
                            && !project.tags.Select(pt => pt.id).Contains(t.id))
                .ToList();
            ViewBag.AvailableTags = availableTags;

            return PartialView("~/Views/Project/_ProjectDetails.cshtml", vm);
        }


        /// <summary>
        /// Create projetc by Kanban-col Actions.
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public ActionResult Create(int? workspaceId)
        {
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return authError;

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : _context.workspaces.Where(w => w.owner_id == currentUser.id).Select(w => w.id).FirstOrDefault();

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound("Workspace không hợp lệ.");
            }

            var (wsError, workspace) = GetAuthorizedWorkspace(currentWorkspaceId);
            if (wsError != null) return wsError;

            ViewBag.WorkspaceId = currentWorkspaceId;
            ViewBag.WorkspaceName = _context.workspaces.Where(w => w.id == currentWorkspaceId).Select(w => w.name).FirstOrDefault();
            return View();
        }

        /// <summary>
        /// Create a new project with minimal information.
        /// </summary>
        /// <param name="workspaceId">Workspace ID to create project in</param>
        /// <param name="name">Project name</param>
        /// <param name="status">Project status</param>
        /// <returns>ActionResult: Redirect to project details or back to Kanbanboard</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMinimal(int workspaceId, string name, string status)
        {
            var (wsError, workspace) = GetAuthorizedWorkspace(workspaceId);
            if (wsError != null) return wsError;

            // Validate input
            if (string.IsNullOrWhiteSpace(name))
            {
                SetErrorToast(ErrorList.ProjectNameRequired);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId });
            }

            var now = DateTime.Now;
            var project = new projects
            {
                workspace_id = workspaceId,
                name = name,
                description = null,
                cover_image_url = null,
                status = NormalizeStatusFor_context(status),
                project_type = "personal",
                start_date = null,
                end_date = null,
                is_archived = false,
                is_deleted = false,
                created_at = now,
                updated_at = now
            };

            _context.projects.Add(project);
            _context.SaveChanges();

            _context.project_members.Add(new project_members
            {
                project_id = project.id,
                user_id = CurrentUser.id,
                role = RoleMember[0]
            });
            _context.SaveChanges();

            SetSuccessToast(SuccessList.Project.Created);

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId, openProjectId = project.id });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string name, string description, string status, DateTime? startDate, DateTime? endDate, users currentUser)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Tên project là bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                var vm = new ProjectDetailsViewModel
                {
                    Id = (int)project.id,
                    WorkspaceId = (int)project.workspace_id,
                    Name = name,
                    Description = description,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate
                };
                return View(vm);
            }

            project.name = name;
            project.description = description;
            project.status = NormalizeStatusFor_context(status);
            project.start_date = startDate;
            project.end_date = endDate;
            project.updated_at = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Details", "Project", new { id = project.id });
        }

        /// <summary>
        /// Get Tasks
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asPartial"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTasks(int id, bool asPartial)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            var tasks = project.tasks
                .Where(t => !t.is_deleted)
                .Select(t => new TaskItemViewModel
                {
                    Id = t.id,
                    Name = t.name,
                    Status = t.status,
                    Priority = t.priority,
                    DueDate = t.end_date,
                    ProjectName = project.name,
                    TaskListName = t.task_lists != null ? t.task_lists.name : null
                })
                .ToList();

            return PartialView("~/Views/Project/_TaskList.cshtml", tasks);
        }

        

        /// <summary>
        /// Update Name and Description of Project | POST /UpdateNameDescription
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="name">Project Name</param>
        /// <param name="description">Project Description</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateNameDescription(int id, string name, string description)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            if (string.IsNullOrWhiteSpace(name))
            {
                SetErrorToast(ErrorList.ProjectNameRequired);
                return RedirectToKanban(project);
            }

            project.name = name.Trim();
            project.description = description;
            project.updated_at = DateTime.Now;
            _context.SaveChanges();

            LogConsole("UpdateNameDescription", $"Updated Project {id}");
            SetSuccessToast(SuccessList.Project.Updated);
            
            return RedirectToKanban(project);
        }

        /// <summary>
        /// Update Status[] of Project | POST /Update Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            project.status = NormalizeStatusFor_context(status);
            project.updated_at = DateTime.Now;
            _context.SaveChanges();

            return RedirectToKanban(project);
        }

        /// <summary>
        /// Update Deadline of Project | POST /UpdateDeadline
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDeadline(int id, DateTime? startDate, DateTime? endDate)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            project.start_date = startDate;
            project.end_date = endDate;
            project.updated_at = DateTime.Now;
            _context.SaveChanges();

            return RedirectToKanban(project);
        }

        /// <summary>
        /// Add member to project and set member's role is "member" | POST /AddMember
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="memberEmail">Member Email</param>
        /// <returns>Redirect to Kanbanboard: {workspaceId, openProjectId = id}</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(int id, string memberEmail)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            if (string.IsNullOrWhiteSpace(memberEmail))
            {

                SetToastMessage("error", "Member Email is required.");
            }

            var user = _context.users.SingleOrDefault(u => u.email == memberEmail && !u.is_deleted);
            if (user == null)
            {
                SetToastMessage("error", "Not found any user with this email.");
            }

            var exists = _context.project_members.Any(pm => pm.project_id == project.id && pm.user_id == user.id);
            if (!exists)
            {
                _context.project_members.Add(new project_members
                {
                    project_id = project.id,
                    user_id = user.id,
                    role = RoleMember[1]
                });
                _context.SaveChanges();
            }

            return RedirectToKanban(project);
        }


        /// <summary>
        /// Remove member from project | POST /RemoveMember
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMember(int id, int userId)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            var pm = _context.project_members.SingleOrDefault(m => m.project_id == project.id && m.user_id == userId);
            if (pm != null)
            {
                _context.project_members.Remove(pm);
                _context.SaveChanges();
            }

            return RedirectToKanban(project);
        }

        /// <summary>
        /// Add Tags to Project | POST /AddTag
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTag(int id, int tagId)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            var tag = _context.tags.SingleOrDefault(t => t.id == tagId && t.workspace_id == project.workspace_id);
            if (tag == null)
            {
                
                TempData["Error"] = "Tag không hợp lệ.";
            }

            if (!project.tags.Any(t => t.id == tag.id))
            {
                project.tags.Add(tag);
                _context.SaveChanges();
            }

            return RedirectToKanban(project);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveTag(int id, int tagId)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            var tag = project.tags.SingleOrDefault(t => t.id == tagId);
            if (tag != null)
            {
                project.tags.Remove(tag);
                _context.SaveChanges();
            }

            return RedirectToKanban(project);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="colorHex"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTagAndAssign(int id, string name, string colorHex)
        {
            var (error, project) = GetAuthorizedProject(id);
            if (error != null) return error;

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên tag là bắt buộc.";
                return RedirectToKanban(project);
            }

            var tag = new tags
            {
                workspace_id = project.workspace_id,
                name = name.Trim(),
                color_hex = string.IsNullOrWhiteSpace(colorHex) ? "#80c8ff" : colorHex.Trim()
            };
            _context.tags.Add(tag);
            _context.SaveChanges();

            var createdTag = _context.tags.SingleOrDefault(t => t.id == tag.id);
            if (createdTag != null && !project.tags.Any(t => t.id == createdTag.id))
            {
                project.tags.Add(createdTag);
                _context.SaveChanges();
            }

            return RedirectToKanban(project);
        }
    }
}