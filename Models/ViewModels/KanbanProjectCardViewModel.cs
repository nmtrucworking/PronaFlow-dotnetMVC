using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PronaFlow_MVC.Models.ViewModels
{
    /// <summary>
    /// Project Card displayed in the Kanban Board
    /// </summary>
    public class KanbanProjectCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public IEnumerable<ProjectTagViewModel> Tags { get; set; }
        public IEnumerable<ProjectMemberViewModel> Members { get; set; }
        public bool IsCompleted { get; set; }
        public int RemainingDays { get; set; }
    }

    /// <summary>
    /// Tags list be assigned to a project
    /// </summary>
    public class ProjectTagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
    }

    /// <summary>
    /// Member be assigned to a project
    /// </summary>
    public class ProjectMemberViewModel
    {
        public int UserId { get; set; }
        public string AvatarUrl { get; set; }
    }

    /// <summary>
    /// Information of the Kanban Board:
    /// - Current Workspace
    ///  + Name;
    ///  + Description
    /// - List of Projects in the Workspace
    /// </summary>
    public class KanbanBoardViewModel
    {
        public int CurrentWorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public string WorkspaceDescription { get; set; }
        public List<KanbanProjectCardViewModel> Projects { get; set; }
    }
}