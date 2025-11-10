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