using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models;

namespace PronaFlow_MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly PronaFlow_DBContext _db = new PronaFlow_DBContext();
        
        /// <summary>
        /// Displayed Dashboard main page
        /// </summary>
        /// <returns>View Index.cshtml with statitics data</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var email = User?.Identity?.Name;
            var currentUser = _db.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var workspaceIds = _db.workspaces.Where(w => w.owner_id == currentUser.id).Select(w => w.id);
            var totalProjects = _db.projects.Count(p => !p.is_deleted && workspaceIds.Contains(p.workspace_id));

            var assignedTasks = _db.tasks.Where(t => !t.is_deleted && t.users1.Any(u => u.id == currentUser.id));
            var inProgressCount = assignedTasks.Count(t => t.status == "inprogress" || t.status == "in-progress");
            var overdueCount = assignedTasks.Count(t => t.end_date.HasValue && t.end_date.Value < DateTime.Now && !(t.status == "done" || t.status == "completed"));

            var today = DateTime.Now.Date;
            var next7 = today.AddDays(7);
            var upcomingTasks = _db.tasks
                .Include("projects.workspaces")
                .Include("task_lists")
                .Where(t => !t.is_deleted && t.users1.Any(u => u.id == currentUser.id))
                .Where(t => t.end_date.HasValue && t.end_date.Value >= today && t.end_date.Value <= next7)
                .Where(t => !(t.status == "done" || t.status == "completed"))
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
                    WorkspaceName = t.projects.workspaces.name
                })
                .ToList();

            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalTasks = inProgressCount;
            ViewBag.TotalTasksOverdue = overdueCount;
            ViewBag.UpcomingTasks = upcomingTasks;
            ViewBag.Title = "Dashboard | PronaFlow";

            return View();
        }
    }
}