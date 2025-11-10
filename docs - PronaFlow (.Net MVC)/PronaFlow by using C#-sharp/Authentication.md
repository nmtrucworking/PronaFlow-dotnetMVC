Triển khai Authentication (login/register) dùng Forms Authentication cho dự án MVC5
Sử dụng bảng: `users` (EF6)
Mục tiêu: đăng nhập bằng email/password, tạo cookie đăng nhập, bảo vệ toàn bộ ứng dụng trừ trang Home và các action của Account.
Các thay đổi triển khai:
- Thêm Forms Authentication vào `Web.config` .
- Thêm global `[Authorize]` filter, cho phép bỏ qua với `[AllowAnonymous]` .
- Cập nhật `AccountController` để xử lý Login/Logout và Register với `FormsAuthentication` .
- Bổ sung hàm hash/verify mật khẩu (PBKDF2) và fallback legacy.
- Chuyển `_RegisterPartial.cshtml` thành form MVC post về `Account/Register` .
- Cho phép truy cập ẩn danh: `HomeController.Index` và các action `Account` .
## Các cập nhật mã nguồn:
### 1. Cấu hình Forms Authentication trong `Web.config`
```config
<configuration>
  // ... existing code ...
  <system.web>
    <compilation debug="true" targetFramework="4.7.2" />
    <httpRuntime targetFramework="4.7.2" />
    // ... existing code ...
    <authentication mode="Forms">
      <forms loginUrl="~/Home/Index" timeout="60" slidingExpiration="true" />
    </authentication>
  </system.web>
  // ... existing code ...
</configuration>
```
***Giải thích**: bật Forms Auth, trỏ loginUrl về Home/Index nơi đang hiển thị form đăng nhập/đăng ký.*
### 2. Thêm global Authorize filter:
```cs 
//(FilterConfig.cs)

using System.Web.Mvc;

namespace PronaFlow_MVC
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AuthorizeAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
```
### 3. Đăng ký FilterConfig trong Application_Start
```cs
//Global.asax.cs
namespace PronaFlow_MVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            // ... existing code ...
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            // ... existing code ...
        }
    }
}
```

## 4. Cho phép ẩn danh vào Home/Index
```cs
// HomeController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models.ViewModels;
// ... existing code ...
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
        // ... existing code ...

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
        // ... existing code ...

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
        // ... existing code ...

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
                        return CryptographicOperations.FixedTimeEquals(computed, key);
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
```

### 5. Triển khai Login/Logout/Register trong AccountController với hash mật khẩu
```cs
// AccountController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models.ViewModels;
// ... existing code ...
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
        // ... existing code ...

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
        // ... existing code ...

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
        // ... existing code ...

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
                        return CryptographicOperations.FixedTimeEquals(computed, key);
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
```

### 6. Chuyển Register partial thành form MVC và binding vào `RegisterViewModel`
```cs
// _RegisterPartial.cs
@model PronaFlow_MVC.Models.ViewModels.RegisterViewModel

<div class="login-card__form-wrapper login-card__form-wrapper--register">
    @using (Html.BeginForm("Register", "Account", FormMethod.Post, new { role = "form" }))
    {
        @Html.AntiForgeryToken()
        <h1 class="form__title">Registration</h1>
        <div class="form__input-groups">
            <div class="form__input-group">
                @Html.LabelFor(m => m.Username)
                @Html.TextBoxFor(m => m.Username, new { @class = "form__input", placeholder = "Username", minlength = "4", maxlength = "20" })
                @Html.ValidationMessageFor(m => m.Username, "", new { @class = "form__error" })
                <i data-lucide="user" class="form__icon"></i>
            </div>
            <div class="form__input-group">
                @Html.LabelFor(m => m.Email)
                @Html.TextBoxFor(m => m.Email, new { @class = "form__input", placeholder = "Email" })
                @Html.ValidationMessageFor(m => m.Email, "", new { @class = "form__error" })
                <i data-lucide="mail" class="form__icon"></i>
            </div>
            <div class="form__input-group">
                @Html.LabelFor(m => m.Password)
                @Html.PasswordFor(m => m.Password, new { @class = "form__input", placeholder = "Password", minlength = "8" })
                @Html.ValidationMessageFor(m => m.Password, "", new { @class = "form__error" })
            </div>
            <div class="form__input-group">
                @Html.LabelFor(m => m.ConfirmPassword)
                @Html.PasswordFor(m => m.ConfirmPassword, new { @class = "form__input", placeholder = "Confirm Password" })
                @Html.ValidationMessageFor(m => m.ConfirmPassword, "", new { @class = "form__error" })
                <i data-lucide="lock" class="form__icon"></i>
            </div>
        </div>
        <button id="signUpBtn" type="submit" class="form__submit-btn">Register</button>
    }
    <p class="form__text">or register with social platforms</p>
    <div class="form_socials">
        <a href="#" class="form__social-link"><i class="bx bxl-google"></i></a>
        <a href="#" class="form__social-link"><i class="bx bxl-facebook"></i></a>
        <a href="#" class="form__social-link"><i class="bx bxl-github"></i></a>
        <a href="#" class="form__social-link"><i class="bx bxl-linkedin"></i></a>
    </div>
</div>
```

## Lưu ý:
Lưu ý và bước tiếp theo

- Sử dụng `[AllowAnonymous]` cho mọi action cần truy cập trước khi đăng nhập: Home.Index , Account.Login (GET/POST), Account.Register (GET/POST), ForgotPassword , ResetPassword .
- Sau khi đăng nhập, có thể lấy người dùng hiện tại bằng User.Identity.Name (email). Ví dụ trong SettingsController , thay vì FirstOrDefault() , nên lấy var email = User.Identity.Name; var user = _db.users.FirstOrDefault(u => u.email == email); .
- Nếu DB đang có dữ liệu password_hash dạng plaintext, hàm VerifyPassword đã có fallback cho phép đăng nhập. Bạn có thể “nâng cấp” hash bằng cách re-hash lại và lưu sau khi đăng nhập thành công.
- Nếu muốn phân quyền theo vai trò, có thể lưu role vào FormsAuthenticationTicket.UserData và kiểm tra trong AuthorizeAttribute tùy biến.