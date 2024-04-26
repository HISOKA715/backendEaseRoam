using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;

namespace webCore.Models
{
    [FirestoreData]
    public class Message
    {
        [FirestoreProperty]
        public string UserID { get; set; }
        [FirestoreProperty]
        public string RecipientID { get; set; }
        [FirestoreProperty]
        public string MessageText { get; set; }
        [FirestoreProperty]
        public string MessageImage { get; set; }

        [FirestoreProperty]
        public string MessageID { get; set; }

        [FirestoreProperty]
        public string MessageDate { get; set; }

        [FirestoreProperty]
        public string MessageChannel { get; set; }
        public bool IsReceived { get; internal set; }
        public bool IsSent { get; internal set; }
    }
}

