using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class TaskListController : Controller
    {
        // GET: TaskList
        public ActionResult Index()
        {
            return View();
        }
    }
}