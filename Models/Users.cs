using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace webCore.Models
{
    public class User
    {
        public class UserModel
        {          
            public string UserName { get; set; }

            [Required(ErrorMessage = "Gender is required")]
            public string Gender { get; set; }

            public string Email { get; set; }

            public string HomeAdd { get; set; }

            [Required(ErrorMessage = "Name is required")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Date of Birth is required")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Phone Number is required")]
            [Phone(ErrorMessage = "Invalid Phone Number")]
            public string PhoneNumber { get; set; }
           
            public string ImageUrl { get; set; }
        }
        public class ForgotPasswordModel
        {
            [Required(ErrorMessage = "The email are required")]
            [EmailAddress(ErrorMessage = "Not a valid Email")]
            public string Email { get; set; }
        }

        public class ChangePasswordViewModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string CurrentPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

    }
  
}
