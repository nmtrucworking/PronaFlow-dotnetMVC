using System.Data.Entity.Core.Metadata.Edm;
using System.Web.Mvc;
using PronaFlow_MVC.App_Start;

namespace PronaFlow_MVC
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AuthorizeAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CurrentUserFilter());
        }
    }
}