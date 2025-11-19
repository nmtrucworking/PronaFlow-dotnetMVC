using System.Web.Mvc;
using System.Linq;
using PronaFlow_MVC.Models;

namespace PronaFlow_MVC.Controllers
{
    public class BaseController : Controller
    {
        protected readonly PronaFlow_DBContext _context = new PronaFlow_DBContext();

        // Property lưu User (được set ở OnActionExecuting)
        protected users CurrentUser { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var email = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                CurrentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            }
        }

        /// <summary>
        /// Helper kiểm tra đăng nhập và trả về kết quả kép.
        /// </summary>
        /// <returns>Tuple (ErrorResult, UserObject)</returns>
        protected (ActionResult Error, users User) GetAuthenticatedUserOrError()
        {
            if (CurrentUser == null)
            {
                return (RedirectToAction("Login", "Account"), null);
            }
            return (null, CurrentUser);
        }

        /// <summary>
        /// Helper kiểm tra đăng nhập cho JSON request
        /// </summary>
        protected (ActionResult Error, users User) GetAuthenticatedUserOrErrorJson()
        {
            if (CurrentUser == null)
            {
                return (Json(new { success = false, message = "Bạn cần đăng nhập." }), null);
            }
            return (null, CurrentUser);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}