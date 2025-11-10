using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models.ViewModels;


namespace PronaFlow_MVC.Controllers
{
    public class SettingsController : Controller
    {
        private readonly PronaFlow_MVC.Models.PronaFlow_DBContext _db = new PronaFlow_MVC.Models.PronaFlow_DBContext();

        [HttpGet]
        public ActionResult Index()
        {
            var user = _db.users.FirstOrDefault();
            if (user == null)
            {
                ViewBag.Error = "No user found in database.";
                return View();
            }

            var model = new PronaFlow_MVC.Models.ViewModels.SettingsViewModel
            {
                Id = user.id,
                Username = user.username,
                Email = user.email,
                FullName = user.full_name,
                AvatarUrl = user.avatar_url,
                Bio = user.bio,
                ThemePreference = user.theme_preference
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateProfile(string fullName, string bio)
        {
            var user = _db.users.FirstOrDefault();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.full_name = fullName ?? user.full_name;
            user.bio = bio ?? user.bio;
            user.updated_at = DateTime.Now;

            _db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Profile updated.",
                data = new { fullName = user.full_name, bio = user.bio }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTheme(string theme)
        {
            var user = _db.users.FirstOrDefault();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var normalized = (theme ?? "").Trim().ToLower();
            if (normalized != "light" && normalized != "dark")
            {
                return Json(new { success = false, message = "Invalid theme. Use 'light' or 'dark'." });
            }

            user.theme_preference = normalized;
            user.updated_at = DateTime.Now;
            _db.SaveChanges();

            return Json(new { success = true, message = "Theme updated.", data = new { theme = user.theme_preference } }, JsonRequestBehavior.AllowGet);
        }
    }
}