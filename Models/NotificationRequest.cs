using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class NotificationRequest
    {
        [Required(ErrorMessage = "The title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The body is required.")]
        public string Body { get; set; }

        public string? ImageUrl { get; set; }
    }
}
