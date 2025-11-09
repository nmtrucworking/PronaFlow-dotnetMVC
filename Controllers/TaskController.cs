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
        private readonly PronaFlow_DBContext db = new PronaFlow_DBContext();

        // GET: Task
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDetails(int id)
        {
            var task = db.tasks.Find(id);

            return View(tasks);
        }
    }
}