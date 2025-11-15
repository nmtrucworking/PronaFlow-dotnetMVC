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
            int totalProjects = _db.projects.Count();
            int totalTasks = _db.tasks.Count();

            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalTasks = totalTasks;

            ViewBag.Titel = "Dashboard | PronaFlow";

            return View();
        }
    }
}