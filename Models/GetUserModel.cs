using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace webCore.Models
{
    [FirestoreData]
    public class GetUserModel
    {
        [FirestoreProperty]
        public string Email { get; set; }


        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }



        [FirestoreProperty]
        public string ProfileImage { get; set; }


        [FirestoreProperty]
        public string UserCategory { get; set; }
    }

}
