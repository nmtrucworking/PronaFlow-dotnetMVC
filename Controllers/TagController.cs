using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;

namespace PronaFlow_MVC.Controllers
{
    public class TagController : Controller
    {
        private readonly PronaFlow_DBContext _DBContext = new PronaFlow_DBContext();
        // GET: Tag
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTagsByWorkspace(long workspaceId)
        {
            var tags = _DBContext.tags
                .Where(t => t.workspace_id == workspaceId)
                .ToList();
            return PartialView(tags);
        }
    }
}