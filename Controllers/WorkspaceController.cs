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
        public ActionResult Index(int? workspaceId)
        {
            int currentWorkspaceId = (int)(workspaceId ?? _context.workspaces.FirstOrDefault()?.id ?? 0);

            //var viewModel = GetKanbanBoardData(currentWorkspaceId);

            if (currentWorkspaceId == 0)
            {
                return Json(new
                {
                    succes = false,
                    message = "No Workspace Choosen"
                });
            }

            var workspace = _context.workspaces.SingleOrDefault(w => w.id == currentWorkspaceId);

            if (workspace == null)
            {
                return HttpNotFound($"Workspace with ID {currentWorkspaceId} not found.");
            }

            // Initialize View Model
            var viewModel = new WorkspaceDetailViewModel
            {
                Id = (int)workspace.id,
                Name = workspace.name,
                Description = workspace.description
            };

            return View(viewModel);
        }

        /// <summary>
        /// List of Workspace
        /// </summary>
        /// <returns>JSON: list of workspaces</returns>
        public ActionResult GetWorkspaces()
        {
            var workspaces = _context.workspaces
                .Select(w => new ListOfWorkspaces{ 
                    Id = (int)w.id, 
                    Name = w.name
                })
                // Authentication
                .ToList();
            return PartialView("_WorkspaceList", workspaces);
        }

        /// <summary>
        /// Creates a new workspace
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns>JSON: workspace information</returns>
        public ActionResult CreateWorkspace(string name, string description)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return Json(new
                {
                    success = false,
                    message = "Workspace name must not empty!"
                });
            }
            try
            {
                var newWorkspace = new workspaces
                {
                    name = name,
                    description = description ?? "",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                    //owner_id
                };

                _context.workspaces.Add(newWorkspace);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    id = newWorkspace.id,
                    message = "Create workspace successful."
                });
            }
            catch (Exception ex)
            {
                // Ghi log ngoại lệ (Logging is essential for professional applications)
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