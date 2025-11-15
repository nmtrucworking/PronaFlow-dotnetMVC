using Internal;
using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace PronaFlow_MVC.Controllers
{
    public class ProjectController : Controller
    {
        /// <summary>
        /// db is the database context for PronaFlow_DBContext. This context is used to interact with the database.
        /// </summary>
        private readonly PronaFlow_DBContext db = new PronaFlow_DBContext();
        

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
        /// [HelperMethod] List of allowed role values for project member {0 - member, 1 - admin}
        /// </summary>
        private static readonly string[] RoleMember = { "member", "admin" }; // indexing: 0 - member, 1 - admin

        /// <summary>
        /// [HelperMethod] List of allowed project type values {0 - personal, 1 - team}
        /// </summary>
        private static readonly string[] ProjectType = { "personal", "team" }; // indexing: 0 - personal, 1 - team

        /// <summary>
        /// [HelperMethod] Normalize status input to match database values
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Normalized status string</returns>
        private static string NormalizeStatusForDb(string status)
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

        // Helpers kiểm tra và xác thực dùng chung
        /// <summary>
        /// [HelperMethod] Authorize user based on session and database.
        /// If user is not authorized, return appropriate response.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="asPartial"></param>
        /// <returns>ActionResult</returns>
        private ActionResult AuthorizeUser(out users currentUser, bool asPartial = false)
        {
            currentUser = null;
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                if (asPartial)
                {
                    return new HttpStatusCodeResult(401);
                    Console.WriteLine("User not authorized as partial.");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                    Console.WriteLine("User not authorized. Redirecting to login.");
                }
            }
        
            currentUser = db.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                if (asPartial)
                {
                    return new HttpStatusCodeResult(401);
                    Console.WriteLine("User not authorized as partial.");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                    Console.WriteLine("User not authorized. Redirecting to login.");
                } 
            }
            return null;
        }

        /// <summary>
        /// [HelperMethod] Authorize user for workspace.
        /// If user is not authorized, return appropriate response.
        /// </summary>
        /// <param name="workspaceId">Workspace ID to check authorization for</param>
        /// <param name="currentUser">Current user object</param>
        /// <param name="asPartial">Flag indicating whether to return partial response (401) or full redirect (302)</param>
        /// <returns>ActionResult: null if authorized, otherwise appropriate response</returns>
        private ActionResult AuthorizeForWorkspace(long workspaceId, out users currentUser, bool asPartial = false)
        {
            currentUser = null;
            var userResult = AuthorizeUser(out currentUser, asPartial);
            if (userResult != null) return userResult;
        
            var ownsWorkspace = db.workspaces.Any(w => w.id == workspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound("Workspace không thuộc quyền của bạn.");
                // WorkspaceNotFound: Workspace does not exist or user does not own it
            }
        
            return null;
        }

        private ActionResult AuthorizeForProject(int projectId, out users currentUser, out projects project, bool asPartial = false)
        {
            project = null;
            currentUser = null;
        
            var userResult = AuthorizeUser(out currentUser, asPartial);
            if (userResult != null) return userResult;
        
            project = db.projects.SingleOrDefault(p => p.id == projectId && !p.is_deleted);
            if (project == null)
            {
                return HttpNotFound(ErrorList.ProjectNotFound);
                // Project not found or deleted -> ProjectNotFound
            }
        
            var ownsWorkspace = db.workspaces.Any(w => w.id == project.workspace_id && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound(ErrorList.UnauthorizedUpdateProject);
            }
        
            return null;
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

        /// <summary>
        /// Get all projects in a workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace ID to get projects for</param>
        /// <returns>ActionResult: View with list of projects</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId)
        {
            var email = User?.Identity?.Name;
            var currentUser = db.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : db.workspaces.Where(w => w.owner_id == currentUser.id).Select(w => w.id).FirstOrDefault();

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound(ErrorList.NoWorkspaceForCurrentUser);
            }

            var ownsWorkspace = db.workspaces.Any(w => w.id == currentWorkspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound(ErrorList.WorkspaceNotBelongToYou);
            }

            var projects = db.projects
                .Where(p => !p.is_deleted && p.workspace_id == currentWorkspaceId)
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
            ViewBag.WorkspaceName = db.workspaces.Where(w => w.id == currentWorkspaceId).Select(w => w.name).FirstOrDefault();

            return View(projects);
        }


        /// <summary>
        /// Get project details partial view.
        /// </summary>
        /// <param name="id">Project ID to get details for</param>
        /// <returns>ActionResult: Partial view with project details</returns>
        [HttpGet]
        public ActionResult DetailsPartial(int id)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project, asPartial: true);
            if (authResult != null) return authResult;

            var vm = MapToProjectDetailsViewModel(project);

            var availableTags = db.tags
                .Where(t => t.workspace_id == project.workspace_id && !project.tags.Select(pt => pt.id).Contains(t.id))
                .ToList();
            ViewBag.AvailableTags = availableTags;

            return PartialView("~/Views/Project/_ProjectDetails.cshtml", vm);
        }


        /// <summary>
        /// Taoj Project
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public ActionResult Create(int? workspaceId)
        {
            var userResult = AuthorizeUser(out var currentUser);
            if (userResult != null) return userResult;

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : db.workspaces.Where(w => w.owner_id == currentUser.id).Select(w => w.id).FirstOrDefault();

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound("Workspace không hợp lệ.");
            }

            var wsResult = AuthorizeForWorkspace(currentWorkspaceId, out currentUser);
            if (wsResult != null) return wsResult;

            ViewBag.WorkspaceId = currentWorkspaceId;
            ViewBag.WorkspaceName = db.workspaces.Where(w => w.id == currentWorkspaceId).Select(w => w.name).FirstOrDefault();
            return View();
        }

        public ActionResult Edit(int id)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            var vm = MapToProjectDetailsViewModel(project);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string name, string description, string status, DateTime? startDate, DateTime? endDate)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

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
            project.status = NormalizeStatusForDb(status);
            project.start_date = startDate;
            project.end_date = endDate;
            project.updated_at = DateTime.Now;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = project.id });
        }

        [HttpGet]
        public ActionResult GetTasks(int projectId)
        {
            var authResult = AuthorizeForProject(projectId, out var currentUser, out var project, asPartial: true);
            if (authResult != null) return authResult;

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
            var wsResult = AuthorizeForWorkspace(workspaceId, out var currentUser);
            if (wsResult != null) return wsResult;

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Tên project là bắt buộc.");
                // name, status are required fields
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId });
            }

            var now = DateTime.Now;
            var normalizedStatus = NormalizeStatusForDb(status);
            var project = new projects
            {
                workspace_id = workspaceId,
                name = name,
                description = null,
                cover_image_url = null,
                status = normalizedStatus,
                project_type = "personal",
                start_date = null,
                end_date = null,
                is_archived = false,
                is_deleted = false,
                created_at = now,
                updated_at = now
            };

            db.projects.Add(project);
            db.SaveChanges();

            db.project_members.Add(new project_members
            {
                project_id = project.id,
                user_id = currentUser.id,
                role = "admin"
            });
            db.SaveChanges();

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId, openProjectId = project.id });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên project là bắt buộc.";
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            project.name = name.Trim();
            project.description = description;
            project.updated_at = DateTime.Now;
            db.SaveChanges();

            return
                RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            project.status = NormalizeStatusForDb(status);
            project.updated_at = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            project.start_date = startDate;
            project.end_date = endDate;
            project.updated_at = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(memberEmail))
            {
                TempData["Error"] = "Email thành viên là bắt buộc.";
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            var user = db.users.SingleOrDefault(u => u.email == memberEmail && !u.is_deleted);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng với email này.";
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            var exists = db.project_members.Any(pm => pm.project_id == project.id && pm.user_id == user.id);
            if (!exists)
            {
                db.project_members.Add(new project_members
                {
                    project_id = project.id,
                    user_id = user.id,
                    role = "member"
                });
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            var pm = db.project_members.SingleOrDefault(m => m.project_id == project.id && m.user_id == userId);
            if (pm != null)
            {
                db.project_members.Remove(pm);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new 
                { 
                    workspaceId = (int)project.workspace_id, 
                    openProjectId = id 
                });
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
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            var tag = db.tags.SingleOrDefault(t => t.id == tagId && t.workspace_id == project.workspace_id);
            if (tag == null)
            {
                TempData["Error"] = "Tag không hợp lệ.";
                return RedirectToAction("Index", "Kanbanboard", new 
                    { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            if (!project.tags.Any(t => t.id == tag.id))
            {
                project.tags.Add(tag);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new 
                { workspaceId = (int)project.workspace_id, openProjectId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveTag(int id, int tagId)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            var tag = project.tags.SingleOrDefault(t => t.id == tagId);
            if (tag != null)
            {
                project.tags.Remove(tag);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTagAndAssign(int id, string name, string colorHex)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project);
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên tag là bắt buộc.";
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            var tag = new tags
            {
                workspace_id = project.workspace_id,
                name = name.Trim(),
                color_hex = string.IsNullOrWhiteSpace(colorHex) ? "#80c8ff" : colorHex.Trim()
            };
            db.tags.Add(tag);
            db.SaveChanges();

            var createdTag = db.tags.SingleOrDefault(t => t.id == tag.id);
            if (createdTag != null && !project.tags.Any(t => t.id == createdTag.id))
            {
                project.tags.Add(createdTag);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
        }
    }
}