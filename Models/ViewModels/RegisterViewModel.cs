using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PronaFlow_MVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please, Enter Username.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please, Enter Email.")]
        [EmailAddress(ErrorMessage = "Email is unvalidation.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please, Enter Password.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận Mật khẩu")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}