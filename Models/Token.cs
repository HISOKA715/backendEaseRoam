using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class Token
    {
        public class TokenModel
        {
            public string IdToken { get; set; }
        }
    }
}