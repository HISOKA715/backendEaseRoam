using Google.Cloud.Firestore;
using System;
using System.ComponentModel.DataAnnotations;

namespace webCore.Models
{
    [FirestoreData]
    public class RegisterModel
    {
        [FirestoreProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [FirestoreProperty]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#\\$%^&*()\\-_+=\\[\\]{}|;:'\",.<>?/])(?=\\S+$).{8,}$",
           ErrorMessage = "Password must be at least 8 characters long and contain at least one digit, one lowercase letter, one uppercase letter, and one special character.")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [FirestoreProperty]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Full name is required")]
        public string Name { get; set; }

        [FirestoreProperty]
        [DataType(DataType.Date)]
        
        public DateTime DateOfBirth { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [FirestoreProperty]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [FirestoreProperty]
        [Required(ErrorMessage = "Home Address is required")]
        public string HomeAdd { get; set; }

        [FirestoreProperty]
        public string UserCategory { get; set; }

        [FirestoreProperty]
        public string ImageUrl { get; set; }
    }
}
