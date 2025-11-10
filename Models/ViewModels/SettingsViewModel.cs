using System;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class SettingsViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }

        public string ThemePreference { get; set; }
    }
}