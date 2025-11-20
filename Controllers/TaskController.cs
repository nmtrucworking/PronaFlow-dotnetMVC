using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class TaskController : BaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(long id, string currentStatus)
        {
            // Logic: Nếu đang done -> not-started, ngược lại -> done
            var newStatus = (currentStatus == "done") ? "not-started" : "done";

            var task = _context.tasks.Find(id);
            if (task != null)
            {
                task.status = newStatus;
                _context.SaveChanges();
            }
            SetSuccessToast(SuccessList.Task.StatusUpdated(currentStatus, newStatus));

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// My Tasks Page - View all tasks in the selected workspace
        /// </summary>
        /// <param name="workspaceId">[int]workspaceId</param>
        /// <returns>View Page: My Task Page</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId)
        {
            var (authError, currentUser) = GetAuthenticatedUserOrError();
            if (authError != null) return authError;

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : (_context.workspaces.FirstOrDefault()?.id ?? 0);

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound(ErrorList.NoWorkspaceSelectedOrExists);
            }

            var workspace = _context.workspaces.SingleOrDefault(w => w.id == currentWorkspaceId);

            var tasksQuery = _context.tasks
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

        /// <summary>
        /// Create Minimal Task (just with name) - used in various places
        /// </summary>
        /// <param name="projectId">[long]</param>
        /// <param name="name">[string]</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMinimal (long projectId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                SetErrorToast(ErrorList.Task.NameRequired);
                return Redirect(Request.UrlReferrer.ToString());
            }

            var taskList = _context.task_lists
                .Where(tl => tl.project_id == projectId && tl.is_deleted == false)
                .OrderBy(tl => tl.id) // Lấy list cũ nhất
                .FirstOrDefault();

            if (taskList == null)
            {
                taskList = new task_lists
                {
                    project_id = projectId,
                    name = "To Do",
                    is_deleted = false,
                    created_at = DateTime.Now
                };
                _context.task_lists.Add(taskList);
                _context.SaveChanges();
            }

            var newTask = new tasks
            {
                project_id = projectId,
                task_list_id = taskList.id,
                creator_id = CurrentUser.id, // Lấy từ BaseController
                name = name.Trim(),
                status = "not-started",
                priority = "normal",
                is_deleted = false,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                is_recurring = false
            };

            _context.tasks.Add(newTask);
            _context.SaveChanges();

            SetSuccessToast(SuccessList.Task.Created);

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Update Task Details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="priority"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="taskListId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(long id, string name = null, string description = null, string priority = null, string status = null, DateTime? startDate = null, DateTime? endDate = null, long? taskListId = null)
        {
            var t = _context.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
            if (t == null)
            {
                SetErrorToast(ErrorList.Task.NotFound);
                return Json(new { success = false, message = ErrorList.Task.NotFound });
            }

            if (description != null) t.description = description;
            if (!string.IsNullOrWhiteSpace(priority)) t.priority = priority.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(status)) t.status = status.Trim().ToLower();

            if (startDate.HasValue) t.start_date = startDate;
            if (endDate.HasValue) t.end_date = endDate;

            if (taskListId.HasValue)
            {
                var tl = _context.task_lists.FirstOrDefault(x => x.id == taskListId.Value);
                if (tl == null)
                {
                    return Json(new { success = false, message = "Target task list not found." });
                }
                t.task_list_id = tl.id;
                t.project_id = tl.project_id; // đồng bộ project khi đổi task list
            }

            t.updated_at = DateTime.Now;
            _context.SaveChanges();

            SetSuccessToast(SuccessList.Task.Updated);

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Soft Delete Task
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            var task = _context.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
            if (task == null)
            {
                return Json(new { success = false, message = ErrorList.Task.NotFound });
            }

            task.is_deleted = true;
            task.deleted_at = DateTime.Now;
            task.updated_at = DateTime.Now;
            _context.SaveChanges();

            SetSuccessToast(SuccessList.Task.Deleted);

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Task Detials Modal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details(int id)
        {
            var task = _context.tasks.FirstOrDefault(t => t.id == id && !t.is_deleted);
            if (task == null) return HttpNotFound();

            var tvm = new TaskItemViewModel
            {
                Id = task.id,
                Name = task.name,
                Status = task.status,
                Priority = task.priority,
                DueDate = task.end_date,
                ProjectName = task.projects?.name,
                TaskListName = task.task_lists?.name,
                // ... map thêm description, subtasks ...
            };

            return PartialView("_TaskDetails", tvm);
        }

        /// <summary>
        /// Rename task
        /// </summary>
        /// <param name="id">taskId</param>
        /// <param name="name">taskName</param>
        /// <returns>Save new name and Redirect</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rename(long id, string name)
        {
            var task = _context.tasks.Find(id);
            if (task != null && !string.IsNullOrWhiteSpace(name))
            {
                task.name = name;
                _context.SaveChanges();
            }

            SetSuccessToast(SuccessList.Task.Renamed);

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Copy Task with the new Task-name is "Original Name (Copy)"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Duplicate(long id)
        {
            var original = _context.tasks.AsNoTracking().FirstOrDefault(t => t.id == id);
            if (original != null)
            {
                var copy = new tasks
                {
                    project_id = original.project_id,
                    task_list_id = original.task_list_id,
                    creator_id = CurrentUser.id,
                    name = original.name + " (Copy)",
                    description = original.description,
                    priority = original.priority,
                    status = "not-started",
                    start_date = original.start_date,
                    end_date = original.end_date,
                    is_deleted = false,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.tasks.Add(copy);
                _context.SaveChanges();

                SetSuccessToast("Đã sao chép công việc.");
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult GetTasksPartial(int workspaceId)
        {
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);

            if (currentUser == null) return Content("");

            var today = DateTime.Now.Date;
            var next7 = today.AddDays(7);

            var tasks = _context.tasks
                .Where(t => !t.is_deleted
                    && t.projects.workspace_id == workspaceId
                    && t.users1.Any(u => u.id == currentUser.id) // Giả định users1 là navigation property cho assignees
                    && t.status != "done" && t.status != "completed")
                .OrderBy(t => t.end_date)
                .Take(10)
                .Select(t => new PronaFlow_MVC.Models.ViewModels.TaskItemViewModel
                {
                    Id = t.id,
                    Name = t.name,
                    Status = t.status,
                    Priority = t.priority,
                    DueDate = t.end_date,
                    ProjectName = t.projects.name,
                    TaskListName = t.task_lists.name,
                    //WorkspaceName = workspace.name
                })
                .ToList();

            return PartialView("_TaskListPartial", tasks);
        }

        [HttpGet]
        public ActionResult GetAll(long projectId, string search = null, string status = null, string sortBy = "creation-date", string sortDir = "desc")
        {
            var query = _context.tasks.Where(t => !t.is_deleted && t.project_id == projectId);

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
            var t = _context.tasks.FirstOrDefault(x => x.id == id && !x.is_deleted);
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

        /// <summary>
        /// Create Task
        /// </summary>
        /// <param name="taskListId"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="priority"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(long taskListId, string name, string description = null, string priority = "normal", string status = "not-started", DateTime? startDate = null, DateTime? endDate = null)
        {
            var (authError, currentUser) = GetAuthenticatedUserOrErrorJson();
            if (authError != null) return authError;

            var taskList = _context.task_lists.FirstOrDefault(tl => tl.id == taskListId);
            if (taskList == null)
            {
                return Json(new { success = false, message = "Task list not found." });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Task name is required." });
            }

            var creator = _context.users.FirstOrDefault(); // TODO: thay bằng user đang đăng nhập khi có auth
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
                creator_id = currentUser.id
            };

            _context.tasks.Add(newTask);
            _context.SaveChanges();

            SetSuccessToast(SuccessList.Task.Created);

            return Json(new { success = true, data = new { id = newTask.id, name = newTask.name } });
        }

    }
}