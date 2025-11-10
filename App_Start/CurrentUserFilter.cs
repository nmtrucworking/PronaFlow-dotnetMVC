using System.Linq;
using System.Web.Mvc;
using PronaFlow_MVC.Models;

namespace PronaFlow_MVC.App_Start
{
    public class CurrentUserFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller == null) return;

            var identity = controller.HttpContext?.User?.Identity;
            if (identity != null && identity.IsAuthenticated)
            {
                var email = identity.Name;
                using (var db = new PronaFlow_DBContext())
                {
                    var user = db.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
                    controller.ViewBag.CurrentUser = user;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}