using PronaFlow_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class TaskController : Controller
    {
        private readonly PronaFlow_DBContext _db = new PronaFlow_DBContext();

        // GET: Task
        [HttpGet]
        public ActionResult Index(int? workspaceId)
        {
            long currentWorkspaceId = workspaceId.HasValue 
                ? (long)workspaceId.Value 
                : (_db.workspaces.FirstOrDefault()?.id ?? 0);

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound(ErrorList.NoWorkspaceSelectedOrExists);
            }

            var workspace = _db.workspaces.SingleOrDefault(w => w.id == currentWorkspaceId);
            if (workspace == null)
            {
                return HttpNotFound(ErrorList.WorkspaceWithIdNotFound(currentWorkspaceId));
            }

            var tasksQuery = _db.tasks
                .Where(t => !t.is_deleted && t.projects.workspace_id == currentWorkspaceId);

            var taskItems = tasksQuery.Select(t => new PronaFlow_MVC.Models.ViewModels.TaskItemViewModel
            {
                Id = t.id,
                Name = t.name,
                Status = t.status,
                Priority = t.priority,
                DueDate = t.end_date,
                ProjectName = t.projects != null ? t.projects.name : null,
                TaskListName = t.task_lists != null ? t.task_lists.name : null
            }).ToList();

            var viewModel = new PronaFlow_MVC.Models.ViewModels.MyTasksViewModel
            {
                WorkspaceId = currentWorkspaceId,
                WorkspaceName = workspace.name,
                Tasks = taskItems
            };

            ViewBag.Title = "My Task | PronaFlow";

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult GetAll(long projectId, string search = null, string status = null, string sortBy = "creation-date", string sortDir = "desc")
        {
            var query = _db.tasks.Where(t => !t.is_deleted && t.project_id == projectId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.name.Contains(s) || (t.description != null && t.description.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.status == status);
            }

            switch ((sortBy ?? "").ToLower())
            {
                case "due-date":
                    query = (sortDir == "asc")
                        ? query.OrderBy(t => t.end_date)
                        : query.OrderByDescending(t => t.end_date);
                    break;
                case "priority":
                    // priority sort: high > normal > low (desc), otherwise asc alphabetical
                    Func<string, int> prioRank = p =>
                        (p == "high" ? 3 : p == "normal" ? 2 : p == "low" ? 1 : 0);
                    query = (sortDir == "asc")
                        ? query.OrderBy(t => prioRank(t.priority)).ThenBy(t => t.name)
                        : query.OrderByDescending(t => prioRank(t.priority)).ThenBy(t => t.name);
                    break;
                case "alphabetical":
                    query = (sortDir == "asc")
                        ? query.OrderBy(t => t.name)
                        : query.OrderByDescending(t => t.name);
                    break;
                case "creation-date":
                default:
                    query = (sortDir == "asc")
                        ? query.OrderBy(t => t.created_at)
                        : query.OrderByDescending(t => t.created_at);
                    break;
            }

            var data = query.Select(t => new
            {
                id = t.id,
                name = t.name,
                status = t.status,
                priority = t.priority,
                dueDate = t.end_date,
                projectName = t.projects != null ? t.projects.name : null,
                taskListName = t.task_lists != null ? t.task_lists.name : null
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetById(long id)
        {
            var t = _db.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
            if (t == null)
            {
                return Json(new { success = false, message = "Task not found." }, JsonRequestBehavior.AllowGet);
            }

            var data = new
            {
                id = t.id,
                name = t.name,
                description = t.description,
                priority = t.priority,
                status = t.status,
                startDate = t.start_date,
                endDate = t.end_date,
                projectId = t.project_id,
                taskListId = t.task_list_id,
                projectName = t.projects != null ? t.projects.name : null,
                taskListName = t.task_lists != null ? t.task_lists.name : null,
                createdAt = t.created_at,
                updatedAt = t.updated_at
            };

            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(long taskListId, string name, string description = null, string priority = "normal", string status = "not-started", DateTime? startDate = null, DateTime? endDate = null)
        {
            var taskList = _db.task_lists.FirstOrDefault(tl => tl.id == taskListId);
            if (taskList == null)
            {
                return Json(new { success = false, message = "Task list not found." });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Task name is required." });
            }

            var creator = _db.users.FirstOrDefault(); // TODO: thay bằng user đang đăng nhập khi có auth
            var now = DateTime.Now;

            var newTask = new tasks
            {
                name = name.Trim(),
                description = description,
                priority = string.IsNullOrWhiteSpace(priority) ? "normal" : priority.Trim().ToLower(),
                status = string.IsNullOrWhiteSpace(status) ? "not-started" : status.Trim().ToLower(),
                start_date = startDate,
                end_date = endDate,
                is_recurring = false,
                recurrence_rule = null,
                next_recurrence_date = null,
                is_deleted = false,
                deleted_at = null,
                created_at = now,
                updated_at = now,
                task_list_id = taskListId,
                project_id = taskList.project_id,
                creator_id = creator?.id ?? 0
            };

            _db.tasks.Add(newTask);
            _db.SaveChanges();

            return Json(new { success = true, data = new { id = newTask.id, name = newTask.name } });
        }

        [HttpPost]
        public ActionResult Update(long id, string name = null, string description = null, string priority = null, string status = null, DateTime? startDate = null, DateTime? endDate = null, long? taskListId = null)
        {
            var t = _db.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
            if (t == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            if (!string.IsNullOrWhiteSpace(name)) t.name = name.Trim();
            if (description != null) t.description = description;
            if (!string.IsNullOrWhiteSpace(priority)) t.priority = priority.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(status)) t.status = status.Trim().ToLower();

            if (startDate.HasValue) t.start_date = startDate;
            if (endDate.HasValue) t.end_date = endDate;

            if (taskListId.HasValue)
            {
                var tl = _db.task_lists.FirstOrDefault(x => x.id == taskListId.Value);
                if (tl == null)
                {
                    return Json(new { success = false, message = "Target task list not found." });
                }
                t.task_list_id = tl.id;
                t.project_id = tl.project_id; // đồng bộ project khi đổi task list
            }

            t.updated_at = DateTime.Now;
            _db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(long id)
        {
            var t = _db.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
            if (t == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }

            t.is_deleted = true;
            t.deleted_at = DateTime.Now;
            t.updated_at = DateTime.Now;
            _db.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult GetDetails(int id)
        {
            var task = _db.tasks.Find(id);
            return View();
        }
    }
}