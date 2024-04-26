using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class TourismAttraction
    {
        public class TourismAttractionViewModel
        {
            public string? Id { get; set; }
      

            [Required(ErrorMessage = "The name is required.")]
            [StringLength(100, ErrorMessage = "The name must be less than 100 characters.")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "The category is required.")]
            public string? Category { get; set; }

            public SelectList? CategoryOptions { get; set; }
            public SelectList? StateOptions { get; set; }

            [Required(ErrorMessage = "The Description is required.")]
            [StringLength(1000, ErrorMessage = "The desciption must be less than 1000 characters.")]
            public string? aDescription { get; set; }



            [Required(ErrorMessage = "The state is required.")]
            public string? State { get; set; }

            public string? PostDate { get; set; } // Assuming string to simplify date handling

            public string? PostTime { get; set; }


            public string? ImageUrl { get; set; }
            public string? NameLowercase { get; set; }

            [Required(ErrorMessage = "the address is required")]
            public string? address { get; set; }

            [Required(ErrorMessage = "The click rate is required.")]
            public int? ClickRate { get; set;}

            [Required(ErrorMessage = "The Latitude is required")]
            [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees.")]
            public double Latitude { get; set; }

            [Required(ErrorMessage = "The Longitude is required")]
            [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees.")]
            public double Longitude { get; set; }

            public DateTime? PostFullDate { get; set; }

            [Required(ErrorMessage = "The date is required")]
            public string? stringFullDate { get; set; }
        }
    }
}
