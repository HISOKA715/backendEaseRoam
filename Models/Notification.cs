using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class Notification
    {
        public class NotificationModel
        {
            public string? Id { get; set; }
            public string? ImageURL { get; set; }

            [Required(ErrorMessage = "The Title is required.")]
            public string Title { get; set; }

            [Required(ErrorMessage = "The Body is required.")]
            public string body { get; set; }

            public DateTime date { get; set; }
            public string stringdate { get; set; }
            public DateTime RepostDate{ get; set; }
            public string stringRepostDate { get; set; }
            public Timestamp PostDate { get; set; }
            public Timestamp CreateDate { get; set; }
        }
    }
}
