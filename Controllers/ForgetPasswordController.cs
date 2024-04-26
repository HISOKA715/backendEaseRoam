using Microsoft.AspNetCore.Mvc;
using static webCore.Models.User;
using webCore.Controllers;
using Google.Cloud.Firestore;


namespace webCore.Controllers
{
    public class ForgetPasswordController : Controller
    {

        private readonly FirebaseService _firebaseService;
        
        public ForgetPasswordController(FirebaseService FirebaseService)
        {
            _firebaseService = FirebaseService;
            
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _firebaseService.SendPasswordResetEmailAsync(user.Email);
                    // Redirect to a confirmation page or display a success message
                    ViewBag.Message = "A password reset link has been sent to your email address.";
                    return View("ForgetPasswordConfirmation","Home");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError(string.Empty, "Error sending password reset email: " + ex.Message);
                }
            }
            return View(user.Email);
        }


    }
}
