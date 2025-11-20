using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class ProjectActivityViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}