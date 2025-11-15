using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PronaFlow_MVC.Models;
using PronaFlow_MVC.Models.ViewModels;

namespace PronaFlow_MVC.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly PronaFlow_DBContext _context = new PronaFlow_DBContext();

        /// <summary>
        /// Workspace Page
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns>View Model</returns>
        [HttpGet]
        public ActionResult Index(int? workspaceId)
        {
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            long currentWorkspaceId = workspaceId.HasValue
                ? (long)workspaceId.Value
                : _context.workspaces
                          .Where(w => w.owner_id == currentUser.id)
                          .Select(w => w.id)
                          .FirstOrDefault();

            if (currentWorkspaceId == 0)
            {
                return HttpNotFound(ErrorList.NoWorkspaceForCurrentUser);
            }

            var workspace = _context.workspaces
                                    .SingleOrDefault(w => w.id == currentWorkspaceId && w.owner_id == currentUser.id);
            if (workspace == null)
            {
                return HttpNotFound(ErrorList.WorkspaceNotOwned(currentWorkspaceId));
            }

            var viewModel = new WorkspaceDetailViewModel
            {
                Id = (int)workspace.id,
                Name = workspace.name,
                Description = workspace.description
            };

            return View(viewModel);
        }

        /// <summary>
        /// List of Workspace theo user hiện tại
        /// </summary>
        public ActionResult GetWorkspaces()
        {
            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            var workspaces = (currentUser == null)
                ? new List<ListOfWorkspaces>()
                : _context.workspaces
                    .Where(w => w.owner_id == currentUser.id)
                    .Select(w => new ListOfWorkspaces
                    {
                        Id = (int)w.id,
                        Name = w.name
                    })
                    .ToList();

            return PartialView("_WorkspaceList", workspaces);
        }

        /// <summary>
        /// Creates a new workspace gán owner là user hiện tại
        /// </summary>
        public ActionResult CreateWorkspace(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new
                {
                    success = false,
                    message = "Tên workspace không được để trống."
                });
            }

            var email = User?.Identity?.Name;
            var currentUser = _context.users.FirstOrDefault(u => u.email == email && !u.is_deleted);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Bạn phải đăng nhập để tạo workspace." });
            }

            try
            {
                var newWorkspace = new workspaces
                {
                    name = name,
                    description = description ?? "",
                    owner_id = currentUser.id,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.workspaces.Add(newWorkspace);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    id = newWorkspace.id,
                    message = "Tạo workspace thành công."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed created workspace: " + ex.Message
                });
            }
        }
        
        /// <summary>
        /// Delete workspace
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JSON</returns>
        public ActionResult DeleteWorkspace(int id)
        {
            try
            {
                var workspace = _context.workspaces.SingleOrDefault(w => w.id == id);

                if (workspace == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Workspace with {id} not found."
                    });
                }

                _context.workspaces.Remove(workspace);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Workspace '{workspace.name} be deleted"
                });
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed deleted Workspace " + ex.Message
                });
            }
        }
    }
}