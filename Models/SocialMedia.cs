using Google.Cloud.Firestore;

namespace webCore.Models
{
    [FirestoreData]
    public class SocialMediaModel
    {
        [FirestoreProperty]
        public string SocialID { get; set; }

        [FirestoreProperty]
        public string SocialContent { get; set; }

        [FirestoreProperty]
        public string SocialDate { get; set; }

        [FirestoreProperty]
        public string SocialImage { get; set; }

        [FirestoreProperty]
        public string UserID { get; set; }
    }
}