using System;
using System.Collections.Generic;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<ProjectTagViewModel> Tags { get; set; }
        public List<ProjectMemberViewModel> Members { get; set; }
        public List<TaskItemViewModel> Tasks { get; set; }
    }
}