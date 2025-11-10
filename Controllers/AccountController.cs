using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models.ViewModels;

namespace PronaFlow_MVC.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Child Action (GET) to display LoginForm
        /// </summary>
        /// <returns>Partial View: _LoginPartial.cshtml</returns>
        [HttpGet]
        [ChildActionOnly]
        public ActionResult Login()
        {
            return PartialView("_LoginPartial", new LoginViewModel());
        }

        /// <summary>
        /// Child Action (GET) to display RegisterForm
        /// </summary>
        /// <returns>Partial View: _RegisterPartial.cshtml</returns>
        [HttpGet]
        [ChildActionOnly]
        public ActionResult Register()
        {
            return PartialView("_RegisterPartial", new LoginViewModel());
        }

        /// <summary>
        /// Action (POST) logic for Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool LoginSuccess = true;
                if (LoginSuccess)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            return View();
        }

        //==========================================

        public ActionResult Logout()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        //===================================== HELPER METHODS
        
    }
}