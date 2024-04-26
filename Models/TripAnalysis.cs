using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace webCore.Models
{
    public class TripAnalysis
    {
        public class TripAnalysisViewModel 
        {
            public string? area { get; set; }
            public double? percentage { get; set; }
            public string? status { get; set; }
        }

        public class LocationViewModel
        {
            public string location { get; set; }
            public int TotalFeedback { get; set; }
            public int Accurate { get; set; }
            public int NotAccurate { get; set; }
            public double AccuracyPercentage { get; set; }
        }
        

    }
}
