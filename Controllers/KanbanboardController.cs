using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class KanbanboardController : Controller
    {
        // GET: Kanbanboard
        public ActionResult Index()
        {
            return View();
        }
    }
}