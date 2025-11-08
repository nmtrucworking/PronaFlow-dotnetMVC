using PronaFlow_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class ProjectController : Controller
    {
        private readonly PronaFlow_DBContext db = new PronaFlow_DBContext();
        // GET: Project
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displayed Kanban Board for a specific workspace
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns>View KanbanBoard with project data</returns>
        //public ActionResult KanbanBoard(long workspaceId)
        //{
        //    var projects = db.projects
        //        .Where(p => p.workspaceId == workspaceId).ToList();

        //    if (projects == null)
        //    {
        //        //return HttpNotFoundResult();
        //    }
        //}

        public ActionResult Details()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}