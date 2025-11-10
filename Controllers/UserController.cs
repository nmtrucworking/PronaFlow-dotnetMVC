using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models;

namespace PronaFlow_MVC.Controllers
{
    public class UserController : Controller
    {
        readonly PronaFlow_DBContext db = new PronaFlow_DBContext();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            return View();
        }

        //public ActionResult Profile()
        //{
        //    return View();
        //}

        public ActionResult DeleteAccount()
        {
            return View();
        }
    }
}