using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace webCore.Models
{
    [FirestoreData]
    public class ReportsModel
    {

        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string Reason { get; set; }

        [FirestoreProperty]
        public string ReportTime { get; set; }

        [FirestoreProperty]
        public string SocialID { get; set; }

        [FirestoreProperty]
        public string UserID { get; set; }

        [FirestoreProperty]
        public string ReportResult { get; set; }
    }
}
