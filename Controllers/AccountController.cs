using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models.ViewModels;
using PronaFlow_MVC.Models;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;

namespace PronaFlow_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly PronaFlow_DBContext _db = new PronaFlow_DBContext();

        // GET: Account
        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public ActionResult Register()
        {
            return PartialView("_RegisterPartial", new RegisterViewModel());
        }

        /// <summary>
        /// Action (POST) logic for Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var user = _db.users.FirstOrDefault(u => u.email == model.Email && u.is_deleted == false);
            if (user == null || !VerifyPassword(model.Password, user.password_hash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View();
            }

            FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
            return RedirectToAction("Index", "Dashboard");
        }

        //==========================================

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var exists = _db.users.Any(u => u.email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "Email đã tồn tại.");
                return View();
            }

            var user = new users
            {
                username = model.Username,
                email = model.Email,
                password_hash = HashPassword(model.Password),
                full_name = model.Username,
                avatar_url = null,
                bio = null,
                theme_preference = "light",
                role = "user",
                is_deleted = false,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _db.users.Add(user);
            _db.SaveChanges();

            FormsAuthentication.SetAuthCookie(model.Email, false);
            return RedirectToAction("Index", "Dashboard");
        }

        //===================================== HELPER METHODS
        private static string HashPassword(string password)
        {
            const int iterations = 10000;
            const int saltSize = 16;
            const int keySize = 32;

            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[saltSize];
                rng.GetBytes(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var key = pbkdf2.GetBytes(keySize);
                    return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
                }
            }
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split('$');
                if (parts.Length == 4 && parts[0] == "PBKDF2")
                {
                    var iterations = int.Parse(parts[1]);
                    var salt = Convert.FromBase64String(parts[2]);
                    var key = Convert.FromBase64String(parts[3]);

                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                    {
                        var computed = pbkdf2.GetBytes(key.Length);
                        return FixedTimeEquals(computed, key);
                    }
                }
                // Fallback legacy (plaintext or other scheme)
                return storedHash == password;
            }
            catch
            {
                return false;
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }
    }
}