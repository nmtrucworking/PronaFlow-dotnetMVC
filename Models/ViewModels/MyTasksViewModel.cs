using System.Collections.Generic;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class MyTasksViewModel
    {
        public long WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public List<TaskItemViewModel> Tasks { get; set; }
    }
}