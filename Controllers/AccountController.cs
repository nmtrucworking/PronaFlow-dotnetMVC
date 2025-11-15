using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PronaFlow_MVC.Models.ViewModels;
using PronaFlow_MVC.Models;

namespace PronaFlow_MVC.Controllers
{
    // Account Cotroller: 
    // This controller handles user account related actions like:
    // - Login
    // - Register
    // - Logout
    // - Change Password
    // - Reset Password
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
        /// Displays the login page (full view)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Displays the register page (full view)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// Handles the login POST request
        /// </summary>
        /// <param name="model">The login view model containing user input</param>
        /// <returns>
        /// Returns a JSON object if the request is AJAX:
        /// - success: true if login is successful, false otherwise
        /// - redirectUrl: the URL to redirect to if login is successful
        /// - errors: a list of error messages if login fails
        /// If the request is not AJAX, returns the login view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, errors = GetModelErrors(ModelState) });
                }
                return View("Login", model);
            }

            var user = _db.users.FirstOrDefault(u => u.email == model.Email && u.is_deleted == false);
            if (user == null || !VerifyPassword(model.Password, user.password_hash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, errors = GetModelErrors(ModelState) });
                }
                return View("Login", model);
            }

            FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });
            }
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
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, errors = GetModelErrors(ModelState) });
                }
                return View("Register", model);
            }

            var exists = _db.users.Any(u => u.email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "Email đã tồn tại.");
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, errors = GetModelErrors(ModelState) });
                }
                return View("Register", model);
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
            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });
            }
            return RedirectToAction("Index", "Dashboard");
        }

        /*===========================================================================================
         *===================================== HELPER METHODS ======================================
         */
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

        /// <summary>
        /// Verifies that the provided password matches the stored hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="storedHash">The stored hash to compare against.</param>
        /// <returns>
        /// Returns true if the password matches the stored hash, false otherwise.
        /// </returns>
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

        /// <summary>
        /// Compares two byte arrays in a time-constant manner to prevent timing attacks.
        /// </summary>
        /// <param name="a">The first byte array to compare.</param>
        /// <param name="b">The second byte array to compare.</param>
        /// <returns>
        /// Returns true if the two byte arrays are equal in a time-constant manner, false otherwise.
        /// </returns>
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

        /// <summary>
        /// Displays the Forgot Password view.
        /// </summary>
        /// <returns>The Forgot Password view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Displays the Reset Password view.
        /// </summary>
        /// <returns>The Reset Password view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// Extracts error messages from the ModelStateDictionary.
        /// </summary>
        /// <param name="modelState">The ModelStateDictionary containing validation errors.</param>
        /// <returns>
        /// A collection of error messages extracted from the ModelStateDictionary.
        /// </returns>
        private static IEnumerable<string> GetModelErrors(System.Web.Mvc.ModelStateDictionary modelState)
        {
            return modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid input." : e.ErrorMessage)
                .ToList();
        }
    }
}