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
        /// 
        /// </summary>
        private static readonly string[] AllowedStatus = new[] { "temp", "not-started", "in-progress", "in-review", "done" };
        
        /// <summary>
        /// 
        /// </summary>
        private static readonly Dictionary<string, string> StatusDisplayName = new Dictionary<string, string>
        {
            { "temp", "Temp" },
            { "not-started", "Not Started" },
            { "in-progress", "In Progress" },
            { "in-review", "In Review" },
            { "done", "Done" }
        };
        private static readonly string[] RoleMember = { "member", "admin" }; // indexing: 0 - member, 1 - admin

        /// <summary>
        /// Normalize status input to match database values
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
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
        private ActionResult AuthorizeUser(out users currentUser, bool asPartial = false)
        {
            currentUser = null;
            var email = User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return asPartial ? new HttpStatusCodeResult(401) : RedirectToAction("Login", "Account");
            }
        
            currentUser = db.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return asPartial ? new HttpStatusCodeResult(401) : RedirectToAction("Login", "Account");
            }
            return null;
        }

        private ActionResult AuthorizeForWorkspace(long workspaceId, out users currentUser, bool asPartial = false)
        {
            currentUser = null;
            var userResult = AuthorizeUser(out currentUser, asPartial);
            if (userResult != null) return userResult;
        
            var ownsWorkspace = db.workspaces.Any(w => w.id == workspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound("Workspace không thuộc quyền của bạn.");
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
                return HttpNotFound("Project không tồn tại.");
            }
        
            var ownsWorkspace = db.workspaces.Any(w => w.id == project.workspace_id && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound("Bạn không có quyền cập nhật project này.");
            }
        
            return null;
        }

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
                return HttpNotFound("Không tìm thấy workspace cho người dùng hiện tại.");
            }

            var ownsWorkspace = db.workspaces.Any(w => w.id == currentWorkspaceId && w.owner_id == currentUser.id);
            if (!ownsWorkspace)
            {
                return HttpNotFound("Workspace không thuộc quyền của bạn.");
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DetailsPartial(int id)
        {
            var authResult = AuthorizeForProject(id, out var currentUser, out var project, asPartial: true);
            if (authResult != null) return authResult;

            var vm = new ProjectDetailsViewModel
            {
                Id = (int)project.id,
                WorkspaceId = (int)project.workspace_id,
                Name = project.name,
                Description = project.description,
                CoverImageUrl = project.cover_image_url,
                Status = project.status,
                StartDate = project.start_date,
                EndDate = project.end_date,
                Tags = project.tags.Select(t => new ProjectTagViewModel
                {
                    Id = (int)t.id,
                    Name = t.name,
                    ColorHex = t.color_hex
                }).ToList(),
                Members = project.project_members.Select(m => new ProjectMemberViewModel
                {
                    UserId = (int)m.user_id,
                    AvatarUrl = m.users.avatar_url
                }).ToList(),
                Tasks = project.tasks
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
                    .ToList()
            };

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

            var vm = new ProjectDetailsViewModel
            {
                Id = (int)project.id,
                WorkspaceId = (int)project.workspace_id,
                Name = project.name,
                Description = project.description,
                CoverImageUrl = project.cover_image_url,
                Status = project.status,
                StartDate = project.start_date,
                EndDate = project.end_date
            };

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMinimal(int workspaceId, string name, string status)
        {
            var wsResult = AuthorizeForWorkspace(workspaceId, out var currentUser);
            if (wsResult != null) return wsResult;

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Tên project là bắt buộc.");
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

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
        /// <param name="id"></param>
        /// <param name="memberEmail"></param>
        /// <returns></returns>
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

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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
                return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
            }

            if (!project.tags.Any(t => t.id == tag.id))
            {
                project.tags.Add(tag);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Kanbanboard", new { workspaceId = (int)project.workspace_id, openProjectId = id });
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