using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please, Enter Email.")]
        [EmailAddress(ErrorMessage = "Email is unvalidation!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please, Enter Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}