using System;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class TaskItemViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string ProjectName { get; set; }
        public string TaskListName { get; set; }
        public string WorkspaceName { get; set; }
    }
}