using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PronaFlow_MVC.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Home Page View - Landing Page (Public)
        /// | GET: /Home
        /// </summary>
        /// <returns>View: Index.cshtml</returns>
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

    }
}