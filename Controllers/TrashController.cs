using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class TrashController : Controller
    {
        // GET: Trash
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult RestoreItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeletePermanent()
        {
            return View();
        }
    }
}