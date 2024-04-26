using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class Feedback
    {
        public class FeedbackViewModel
        {
            public string Id { get; set; }
            public string Read { get; set; }
            public DateTime FeedbackDate { get; set; }
            public string FeedbackCategory { get; set; }
            public string FeedbackDesc { get; set; }
            public string UserID { get; set; }
            public string UserName { get; set; }
            public string FeedbackLocation { get; set; }
            public int FeedbackAccuracy { get; set; }
            public string reply { get; set; }

            [Required(ErrorMessage = "The Reply Desciption")]
            public string replyDesc { get; set; }

            public DateTime replyDate { get; set; }
            public string replyID { get; set; }
        }
    }
}
