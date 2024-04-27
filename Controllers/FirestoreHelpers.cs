using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Owin.BuilderProperties;
using System;
using System.Text;
using System.Threading.Tasks;
using static webCore.Models.TourismAttraction;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Globalization;
using static webCore.Models.TripAnalysis;
using static webCore.Models.Feedback;
using static webCore.Models.Notification;
using Microsoft.CodeAnalysis;
using webCore.Models;
using FirebaseAdmin.Auth;
using static webCore.Models.Token;
using static webCore.Models.User;
using System.Security.Cryptography;
using Firebase.Storage;
using Google.Cloud.Firestore.V1;
using Firebase.Auth.Providers;


namespace webCore.Controllers {



    public class FirestoreController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly FirebaseAuth _auth;
        public FirestoreController(FirebaseApp firebaseApp)
        {
            string filePath = "C:/Users/user/Documents/Visual Studio 2022/webCore/Controllers/fir-project-7ba4f-firebase-adminsdk-3pu6p-8d30027601.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

          
            _firestoreDb = FirestoreDb.Create("fir-project-7ba4f");
            _auth = FirebaseAuth.GetAuth(firebaseApp);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
                    var fbAuth = await _auth.CreateUserAsync(new UserRecordArgs
                    {
                        Email = registerModel.Email,
                        Password = registerModel.Password
                    });

                    string uid = fbAuth.Uid;

                    var newUser = new
                    {
                        registerModel.Email,
                        registerModel.Username,
                        registerModel.Name,
                        DateOfBirth = registerModel.DateOfBirth.ToString("d/M/yyyy"),
                        registerModel.Gender,
                        registerModel.PhoneNumber,
                        registerModel.HomeAdd,
                        ProfileImage = (string)null,
                        UserCategory = "User",
                        PreferAttraction = 1,
                        PreferOther = 1,
                        PreferRestaurant = 1,
                        PreferShopping = 1
                    };

                    CollectionReference usersCollection = _firestoreDb.Collection("users");
                    DocumentReference newUserDocRef = usersCollection.Document(uid);
                    await newUserDocRef.SetAsync(newUser);

                    return RedirectToAction("Index", "Home");
               
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string email = HttpContext.Session.GetString("email");
            string uid = HttpContext.Session.GetString("uid");
            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            if (user == null)
            {
                // Handle user not found
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            try
            {
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = uid,
                    Password = model.NewPassword
                });

                // Redirect to a confirmation page or display a success message
                return RedirectToAction("ListAttractions");
            }
            catch (FirebaseAuthException ex)
            {
                // Handle errors (e.g., password not strong enough)
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [SessionCheck]
        public async Task<IActionResult> ListFeedback()
        {
            var feedbackList = new List<FeedbackViewModel>();
            try
            {
                Query queryRef = _firestoreDb.Collection("Feedback").OrderByDescending("FeedbackDate");
                QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var data = document.ToDictionary();
                        var userID = data.ContainsKey("userID") ? data["userID"]?.ToString() : "";

                        // Initialize the user name as an empty string.
                        string userName = "";

                        if (!string.IsNullOrEmpty(userID))
                        {
                            // Fetch the user document based on the userID.
                            DocumentReference docRef = _firestoreDb.Collection("users").Document(userID);
                            DocumentSnapshot userSnapshot = await docRef.GetSnapshotAsync();
                            if (userSnapshot.Exists)
                            {
                                // Assuming the user name is stored under a field named "name".
                                userName = userSnapshot.GetValue<string>("Name");
                            }
                        }

                        string replyExists = "No"; // Default to "No"
                        var replyQueryRef = _firestoreDb.Collection($"Feedback/{document.Id}/reply").Limit(1);
                        var replySnapshot = await replyQueryRef.GetSnapshotAsync();
                        if (replySnapshot.Documents.Count > 0)
                        {
                            replyExists = "Yes";
                        }

                        var feedback = new FeedbackViewModel
                        {
                            Id = document.Id,
                            Read = data.ContainsKey("read") ? data["read"]?.ToString() : "",
                            FeedbackDate = data.ContainsKey("FeedbackDate") && document.TryGetValue<Timestamp>("FeedbackDate", out var timestamp)
                                ? timestamp.ToDateTime() : DateTime.MinValue,
                            FeedbackCategory = data.ContainsKey("FeedbackCategory") ? data["FeedbackCategory"]?.ToString() : "",
                            FeedbackDesc = data.ContainsKey("FeedbackDesc") ? data["FeedbackDesc"]?.ToString() : "",
                            UserName = userName,
                            reply = replyExists,
                        };
                        feedbackList.Add(feedback);
                    }
                    else {

                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception to your logging framework or output window
                Console.WriteLine(ex.Message);
                // Optionally, return an error view
                return View("Error");
            }

            return View("Feedback", feedbackList);
        }

        [SessionCheck]
        public async Task<IActionResult> GetaFeedback(string Id)
        {
            DocumentReference docRef = _firestoreDb.Collection("Feedback").Document(Id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var userID = data.ContainsKey("userID") ? data["userID"]?.ToString() : "";

                // Initialize the user name as an empty string.
                string userName = "";

                if (!string.IsNullOrEmpty(userID))
                {
                    // Fetch the user document based on the userID.
                    DocumentReference docRef1 = _firestoreDb.Collection("users").Document(userID);
                    DocumentSnapshot userSnapshot = await docRef1.GetSnapshotAsync();
                    if (userSnapshot.Exists)
                    {
                        // Assuming the user name is stored under a field named "name".
                        userName = userSnapshot.GetValue<string>("Name");
                    }
                }
                string replyDesc = "";
                DateTime replyDate = DateTime.MinValue;
                string replyID = "";
                string replyExists = "No"; // Default to "No"
                var replyQueryRef = _firestoreDb.Collection($"Feedback/{snapshot.Id}/reply").Limit(1);
                var replySnapshot = await replyQueryRef.GetSnapshotAsync();
                if (replySnapshot.Documents.Count > 0)
                {
                    replyExists = "Yes";
                    var replyData = replySnapshot.Documents.First().ToDictionary();
                    replyDesc = replyData.ContainsKey("replyDesc") ? replyData["replyDesc"]?.ToString() : "-";
                    replyID = replySnapshot.Documents.First().Id;
                    if (replyData.ContainsKey("replyDate") && replyData["replyDate"] is Google.Cloud.Firestore.Timestamp replyTimestamp)
                    {
                        replyDate = replyTimestamp.ToDateTime();
                    }
                }
                var feedback = new FeedbackViewModel
                {
                    Id = snapshot.Id,
                    Read = data.ContainsKey("read") ? data["read"]?.ToString() : "",
                    FeedbackDate = data.ContainsKey("FeedbackDate") && data["FeedbackDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp
                    ? postFullDateTimestamp.ToDateTime()
                    : DateTime.MinValue,
                    FeedbackLocation = data.ContainsKey("FeedbackLocation") ? data["FeedbackLocation"]?.ToString() : "-",
                    FeedbackCategory = data.ContainsKey("FeedbackCategory") ? data["FeedbackCategory"]?.ToString() : "-",
                    FeedbackDesc = data.ContainsKey("FeedbackDesc") ? data["FeedbackDesc"]?.ToString() : "",
                    UserName = userName,
                    reply = replySnapshot.Documents.Count > 0 ? "Yes" : "No",
                    replyDesc = replyDesc,
                    replyDate = replyDate,
                    replyID = replyID
                };
                return View("ViewFeedback", feedback);
            }
            else
            {
                return NotFound();
            }
        }

        [SessionCheck]
        public async Task<IActionResult> ListAttractions()
        {
            if (!UserIsInRole("owner")&&!UserIsInRole("admin")) 
            {
                return View("access_lost");
            } else {
                var attractionsList = new List<TourismAttractionViewModel>();
                Query queryRef = _firestoreDb.Collection("Tourism Attractions").OrderByDescending("TourismPostDate");
                QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();


                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var data = document.ToDictionary();
                        var attraction = new TourismAttractionViewModel
                        {
                            Id = document.Id,
                            Name = data.ContainsKey("TourismName") ? data["TourismName"]?.ToString() : "",
                            Category = data.ContainsKey("TourismCategory") ? data["TourismCategory"]?.ToString() : "",
                            aDescription = data.ContainsKey("TourismDesc") ? data["TourismDesc"]?.ToString() : "",
                            State = data.ContainsKey("TourismState") ? data["TourismState"]?.ToString() : "",
                            PostDate = data.ContainsKey("TourismPostDate") && data["TourismPostDate"] is Google.Cloud.Firestore.Timestamp timestamp
                            ? timestamp.ToDateTime().ToString("dd/MM/yyyy") : "",
                            ImageUrl = data.ContainsKey("TourismImage") ? data["TourismImage"]?.ToString() : ""
                        };
                        attractionsList.Add(attraction);
                    }
                }
                return View("ListAttractions", attractionsList);
            }
        }

        [SessionCheck]
        public async Task<IActionResult> GetaAttractions(String Id) {

            DocumentReference docRef = _firestoreDb.Collection("Tourism Attractions").Document(Id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var attraction = new TourismAttractionViewModel

                {
                    Id = snapshot.Id,
                    Name = data.ContainsKey("TourismName") ? data["TourismName"]?.ToString() : "",
                    aDescription = data.ContainsKey("TourismDesc") ? data["TourismDesc"]?.ToString() : "",
                    State = data.ContainsKey("TourismState") ? data["TourismState"]?.ToString() : "",
                    Category = data.ContainsKey("TourismCategory") ? data["TourismCategory"]?.ToString() : "",
                    PostDate = data.ContainsKey("TourismPostDate") && data["TourismPostDate"] is Google.Cloud.Firestore.Timestamp timestamp
                        ? timestamp.ToDateTime().ToString("dd/MM/yyyy") : "",
                    ImageUrl = data.ContainsKey("TourismImage") ? data["TourismImage"]?.ToString() : "",
                    address = data.ContainsKey("TourismAddress") ? data["TourismAddress"]?.ToString() : "",
                    ClickRate = data.ContainsKey("clickRate") && data["clickRate"] != null ? Convert.ToInt32(data["clickRate"]) : 0,
                    PostFullDate = data.ContainsKey("TourismPostDate") && data["TourismPostDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp
                    ? postFullDateTimestamp.ToDateTime()
                    : DateTime.MinValue, // Default value if not present
                };

                attraction.CategoryOptions = new SelectList(new List<string>
                {
                    "Shopping",
                    "Restaurant",
                    "Tourism Attraction",
                    "Unknown"
                });
                attraction.StateOptions = new SelectList(new List<string>
                {
                   "Kuala Lumpur" ?? "Wilayah Persekutuan Kuala Lumpur",
                    "Selangor",
                    "Sabah",
                    "Sarawak",
                    "Perak",
                    "Pahang",
                    "Negeri Sembilan",
                    "Kedah",
                    "Kelantan",
                    "Terengganu",
                    "Perlis",
                    "Johor",
                    "Pulau Pinang"
                });

                if (data.ContainsKey("point") && data["point"] is Google.Cloud.Firestore.GeoPoint geoPoint)
                {
                    attraction.Latitude = geoPoint.Latitude;
                    attraction.Longitude = geoPoint.Longitude;
                }



                // Pass the TourismAttractionViewModel to the view
                return View("EditAttraction", attraction);
            }
            else
            {
                // Handle the case where the attraction does not exist
                return NotFound();
            }
        }

        [SessionCheck]
        public IActionResult GotoPassword()
        {
            return View("EditPassword");
        }


        [SessionCheck]
        public async Task<IActionResult> GetaUser()
        {
            var uid = HttpContext.Session.GetString("uid");
            DocumentReference docRef = _firestoreDb.Collection("users").Document(uid);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                DateTime utcDateOfBirth = data.ContainsKey("DateOfBirth") && data["DateOfBirth"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp
    ? DateTime.SpecifyKind(postFullDateTimestamp.ToDateTime(), DateTimeKind.Utc)
    : DateTime.MinValue;
                DateTime localDateOfBirth = utcDateOfBirth.ToLocalTime();

                var user = new UserModel
                {
                    UserName = data.ContainsKey("Username") ? data["Username"]?.ToString() : "",
                    Gender = data.ContainsKey("Gender") ? data["Gender"]?.ToString() : "",
                    HomeAdd = data.ContainsKey("HomeAdd") ? data["HomeAdd"]?.ToString() : "",
                    Email = data.ContainsKey("Email") ? data["Email"]?.ToString() : "",
                    Name = data.ContainsKey("Name") ? data["Name"]?.ToString() : "",
                    PhoneNumber = data.ContainsKey("PhoneNumber") ? data["PhoneNumber"]?.ToString() : "",
                    DateOfBirth = localDateOfBirth,
                    ImageUrl = data.ContainsKey("ProfileImage") ? data["ProfileImage"]?.ToString() : "",
                };
                return View("EditUser", user);
            }
            else 
            {
                return NotFound();
            }
        }

        [SessionCheck]
        public async Task<IActionResult> GetAccuracy()
        {
            var feedbackList = new List<FeedbackViewModel>(); // Assume this is your model class
            Query queryRef = _firestoreDb.Collection("Feedback")
                                    .WhereEqualTo("FeedbackCategory", "Recommendation");
            QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var data = document.ToDictionary();
                    var feedback = new FeedbackViewModel
                    {
                        FeedbackAccuracy = data.ContainsKey("FeedbackAccuracy") && data["FeedbackAccuracy"] != null ? Convert.ToInt32(data["FeedbackAccuracy"]) : 0,
                        FeedbackLocation = data.ContainsKey("FeedbackLocation") ? data["FeedbackLocation"]?.ToString() : "",
                    };
                    feedbackList.Add(feedback);
                }
            }
            // Group by FeedbackLocation and calculate status
            var groupedFeedback = feedbackList.GroupBy(f => f.FeedbackLocation)
                                        .Select(group =>
                                        {
                                            var averageAccuracy = group.Average(f => f.FeedbackAccuracy) * 100;
                                            var status = averageAccuracy >= 50 ? "Success" : "Fail";
                                            return new TripAnalysisViewModel
                                            {
                                                area = group.Key,
                                                percentage = averageAccuracy,
                                                status = status
                                            };
                                        }).ToList();

            return View("TripAnalysis", groupedFeedback);
        }
        
        [SessionCheck]
        public async Task<IActionResult> generateReport()
        {

            var feedbackList = new List<FeedbackViewModel>(); // Assume this is your model class
            Query queryRef = _firestoreDb.Collection("Feedback")
                                    .WhereEqualTo("FeedbackCategory", "Recommendation");
            QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var data = document.ToDictionary();
                    var feedback = new FeedbackViewModel
                    {
                        FeedbackAccuracy = data.ContainsKey("FeedbackAccuracy") && data["FeedbackAccuracy"] != null ? Convert.ToInt32(data["FeedbackAccuracy"]) : 0,
                        FeedbackLocation = data.ContainsKey("FeedbackLocation") ? data["FeedbackLocation"]?.ToString() : "",
                    };
                    feedbackList.Add(feedback);
                }
            }
            // Group by FeedbackLocation and calculate status
            var groupedFeedback = feedbackList.GroupBy(f => f.FeedbackLocation)
                                        .Select(group =>
                                        {
                                            var averageAccuracy = group.Average(f => f.FeedbackAccuracy) * 100;
                                            var status = averageAccuracy >= 50 ? "Success" : "Fail";
                                            return new TripAnalysisViewModel
                                            {
                                                area = group.Key,
                                                percentage = averageAccuracy,
                                                status = status
                                            };
                                        }).ToList();

            return View("report", groupedFeedback);

        }
        
        [SessionCheck]
        public IActionResult goback1()
        {

            return RedirectToAction("ListAttractions");
        }

        [SessionCheck]
        public async Task<IActionResult> ListNotification()
        {
            if (!UserIsInRole("owner")&&!UserIsInRole("admin")) 
            {
                return View("access_lost");
            }
            else
            {
                var NotificationList = new List<NotificationModel>();
                Query queryRef = _firestoreDb.Collection("Notification").OrderByDescending("CreateDate");
                QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {

                        var data = document.ToDictionary();
                        DateTime date = data.ContainsKey("CreateDate") && document.GetValue<Timestamp>("CreateDate") != null
                               ? document.GetValue<Timestamp>("CreateDate").ToDateTime()
                               : DateTime.MinValue;
                        DateTime postDate = data.ContainsKey("PostDate") && document.GetValue<Timestamp>("PostDate") != null
                               ? document.GetValue<Timestamp>("PostDate").ToDateTime()
                               : DateTime.MinValue;
                        string stringPostDate = postDate == DateTime.MinValue ? "N/A" : postDate.ToString("dd/MM/yyyy");
                        string stringDate = date == DateTime.MinValue ? "N/A" : date.ToString("dd/MM/yyyy");
                        var notification = new NotificationModel
                        {
                            Id = document.Id,
                            Title = data.ContainsKey("Title") ? data["Title"]?.ToString() : "",
                            body = data.ContainsKey("Body") ? data["Body"]?.ToString() : "",
                            date = data.ContainsKey("CreateDate") ? document.GetValue<Timestamp>("CreateDate").ToDateTime() : DateTime.MinValue,
                            stringdate = stringDate,
                            ImageURL = data.ContainsKey("ImageURL") ? data["ImageURL"]?.ToString() : "",
                            RepostDate = postDate,
                            stringRepostDate = stringPostDate,
                        };
                        NotificationList.Add(notification);
                    }
                }
                return View("Notification", NotificationList);
            }
            
        }


        [SessionCheck]
        [HttpGet]
        public async Task<IActionResult> DeleteAttraction(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Handle the case where no document ID is provided
                ModelState.AddModelError("", "Document ID is required.");
                return View("Error"); // Replace "Error" with an appropriate view for displaying errors
            }

            try
            {
                DocumentReference docRef = _firestoreDb.Collection("Tourism Attractions").Document(id);
                await docRef.DeleteAsync();
                return RedirectToAction("ListAttractions"); // Redirect to the list action/view after successful update
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document: {ex.Message}");
                // Handle the error (you might want to return to a specific view and show the error message)
                ModelState.AddModelError("", "An error occurred while deleting the Attraction. Please try again.");
                return View("Error"); // Replace "Error" with an appropriate view for displaying errors
            }

        }


        [SessionCheck]
        [HttpGet]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Handle the case where no document ID is provided
                ModelState.AddModelError("", "Document ID is required.");
                return View("Error"); // Replace "Error" with an appropriate view for displaying errors
            }

            try
            {
                DocumentReference docRef = _firestoreDb.Collection("Notification").Document(id);
                await docRef.DeleteAsync();
                return RedirectToAction("ListNotification"); // Redirect to the list action/view after successful update
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document: {ex.Message}");
                // Handle the error (you might want to return to a specific view and show the error message)
                ModelState.AddModelError("", "An error occurred while deleting the notification. Please try again.");
                return View("Error"); // Replace "Error" with an appropriate view for displaying errors
            }

        }

        [SessionCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNotification(NotificationModel model, IFormFile imageFile) 
        {
            if (!string.IsNullOrWhiteSpace(model.Id)) 
            {
                ModelState.Remove("imageFile");
                ModelState.Remove("CreateDate");
                ModelState.Remove("PostDate");
                ModelState.Remove("stringRepostDate");
                ModelState.Remove("stringdate");
                ModelState.Remove("RepostDate");
                ModelState.Remove("date");

            }
            string bucketName = "fir-project-7ba4f.appspot.com";
            string newImageUrl = model.ImageURL;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadResult = await UploadFileToFirebaseStorage(imageFile, bucketName);
                if (uploadResult.Success)
                {
                    newImageUrl = uploadResult.Url; // Update with new URL only if upload succeeds
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload image.");
                    // Optionally log the error or handle further
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {

                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
                ModelState.AddModelError("", $"Validation failed: {errors.ToString()}");
                return View("EditNotification", model);
            }
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                model.ImageURL = newImageUrl;
            }
            try
            {
                DocumentReference docRef = _firestoreDb.Collection("Notification").Document(model.Id);
                Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Title", model.Title },
            {"Body",model.body},

           
        };
                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    updates.Add("ImageURL", newImageUrl);
                }

                await docRef.UpdateAsync(updates);
                return RedirectToAction("ListNotification"); // Redirect to the list action/view after successful update
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while updating the notification. Please try again.");
                // Repopulate the SelectList items in case of an error
                
                return View("EditNotification", model);
            }



        }


        [HttpPost]
        public async Task<IActionResult> VerifyToken([FromBody] TokenModel model)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(model.IdToken);
                var uid = decodedToken.Uid;
                // Proceed with uid, maybe set a session or something similar
                DocumentReference docRef = _firestoreDb.Collection("users").Document(uid);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var data = snapshot.ToDictionary();

                    string verify = data.ContainsKey("UserCategory") ? data["UserCategory"]?.ToString() : "";
                    string username = data.ContainsKey("Username") ? data["Username"]?.ToString() : "";
                    string email = data.ContainsKey("Email") ? data["Email"]?.ToString() : "";
                    string userCategory = data.ContainsKey("UserCategory") ? data["UserCategory"]?.ToString().ToLower() : "";
                    if (userCategory == "admin" || userCategory == "customer support admin" || userCategory == "owner")
                    {
                        HttpContext.Session.SetString("username", username);
                        HttpContext.Session.SetString("category", verify);
                        HttpContext.Session.SetString("category1", userCategory);
                        HttpContext.Session.SetString("uid", uid);
                        HttpContext.Session.SetString("_UserId", uid);
                        HttpContext.Session.SetString("email",email);
                        switch (userCategory)
                        {
                            case "admin":
                                return Json(new { success = true, RedirectUrl = "/Firestore/ListAttractions" });
                            case "owner":
                                return Json(new { success = true, RedirectUrl = "/Firestore/ChangeUserRole" });
                            case "customer support admin":
                                return Json(new { success = true, RedirectUrl = "/Firestore/UserChat" });
                            default:
                                return Json(new { success = false, Message = "You are not allowed to log in here." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, Message = "you are not allow to login here" });   
                    }

                }
                else 
                {
                    return Json(new { Success = false, Message = "user not found" });
                }          
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        private bool UserIsInRole(string requiredRole)
        {
            var currentRole = HttpContext.Session.GetString("category");
            return currentRole?.Equals(requiredRole, StringComparison.OrdinalIgnoreCase) ?? false;
        }




        [SessionCheck]
        public IActionResult addAttraction()
        {
            var attraction = new TourismAttractionViewModel { };
            attraction.CategoryOptions = new SelectList(new List<string>
                {
                    "Shopping",
                    "Restaurant",
                    "Tourism Attraction",
                    "Unknown"
                });

            attraction.StateOptions = new SelectList(new List<string>
                {
                   "Kuala Lumpur" ?? "Wilayah Persekutuan Kuala Lumpur",
                    "Selangor",
                    "Sabah",
                    "Sarawak",
                    "Perak",
                    "Pahang",
                    "Negeri Sembilan",
                    "Kedah",
                    "Kelantan",
                    "Terengganu",
                    "Perlis",
                    "Johor",
                    "Pulau Pinang"
                });

            return View("addAttraction",attraction);
        }
        [SessionCheck]
        public IActionResult GotoNotification()
        {

            return View("sendNotification");
        }

        [SessionCheck]
        [HttpGet]
        public async Task<IActionResult> GotoLocation(string id)
        {
            var feedbackList = new List<FeedbackViewModel>(); // Your model class
            Query queryRef = _firestoreDb.Collection("Feedback")
                                        .WhereEqualTo("FeedbackCategory", "Recommendation")
                                        .WhereEqualTo("FeedbackLocation", id);
            QuerySnapshot snapshot = await queryRef.GetSnapshotAsync();

            int accurateCount = 0;
            int notAccurateCount = 0;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();
                    int feedbackAccuracy = data.ContainsKey("FeedbackAccuracy") && data["FeedbackAccuracy"] != null ? Convert.ToInt32(data["FeedbackAccuracy"]) : 0;
                    string feedbackLocation = data.ContainsKey("FeedbackLocation") ? data["FeedbackLocation"]?.ToString() : "";

                    // Assuming FeedbackViewModel contains these properties
                    var feedback = new FeedbackViewModel
                    {
                        FeedbackAccuracy = feedbackAccuracy,
                        FeedbackLocation = feedbackLocation
                    };
                    feedbackList.Add(feedback);

                    // Increment counts based on FeedbackAccuracy
                    if (feedback.FeedbackAccuracy == 1)
                    {
                        accurateCount++;
                    }
                    else if (feedback.FeedbackAccuracy == 0)
                    {
                        notAccurateCount++;
                    }
                }
            }

            // Calculate the accuracy percentage
            double accuracyPercentage = feedbackList.Count > 0 ? Math.Round((double)accurateCount / feedbackList.Count * 100, 2) : 0;

            // Prepare the ViewModel with the required counts
            var locationAnalysisViewModel = new LocationViewModel
            {
                location = id, // Since you have the area parameter, you can directly assign it
                TotalFeedback = feedbackList.Count,
                Accurate = accurateCount,
                NotAccurate = notAccurateCount,
                AccuracyPercentage = accuracyPercentage
            };

            // Pass the ViewModel to the view
            return View("viewLocation", locationAnalysisViewModel); // Ensure the "viewLocation" view exists and is expecting a LocationViewModel
        }


        [SessionCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecordAttraction(TourismAttractionViewModel model, IFormFile imageFile) {
            ModelState.Remove("PostDate");
            ModelState.Remove("PostTime");
            ModelState.Remove("StateOptions");
            ModelState.Remove("CategoryOptions");
            ModelState.Remove("imageFile");
            ModelState.Remove("PostFullDate");

           
            

            string bucketName = "fir-project-7ba4f.appspot.com";
            string newImageUrl = model.ImageUrl;
            // check got file image or not
            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload the file to Firebase Storage and get the URL
                var uploadResult = await UploadFileToFirebaseStorage(imageFile, bucketName);
                if (uploadResult.Success)
                {
                    newImageUrl = uploadResult.Url; // Update with new URL only if upload succeeds
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload image.");
                }
            }
            // check validation
            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {

                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
                ModelState.AddModelError("", $"Validation failed: {errors.ToString()}");
                model.CategoryOptions = new SelectList(new List<string> { "Shopping", "Restaurant", "Tourism Attraction", "Unknown" });
                model.StateOptions = new SelectList(new List<string> { "Kuala Lumpur", "Selangor", "Sabah", "Sarawak", "Perak", "Pahang", "Negeri Sembilan", "Kedah", "Kelantan", "Terengganu", "Perlis", "Johor", "Pulau Pinang" });
                return View("addAttraction", model);
            }


            if (!string.IsNullOrEmpty(newImageUrl))
            {
              model.ImageUrl = newImageUrl;
            }


            model.NameLowercase = model.Name.ToLower();
            string stringdatetime = model.stringFullDate;
            DateTime parsedDateTime = DateTime.ParseExact(stringdatetime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture).AddHours(8);
            Timestamp timestamp = Timestamp.FromDateTime(parsedDateTime.ToUniversalTime());


            try
            {

                Dictionary<string, object> adds = new Dictionary<string, object>
        {
            { "TourismName", model.Name },
            {"TourismNameLowercase",model.NameLowercase},
            { "TourismDesc", model.aDescription },
            { "TourismState", model.State },
            { "TourismCategory", model.Category },
            { "clickRate", model.ClickRate },
           { "TourismPostDate", timestamp},
            { "point", new GeoPoint(model.Latitude, model.Longitude) },
            { "TourismImage", model.ImageUrl},
            {"TourismAddress", model.address},

        };
                DocumentReference docRef = await _firestoreDb.Collection("Tourism Attractions").AddAsync(adds);
                
                return RedirectToAction("ListAttractions"); 
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while adding the attraction. Please try again.");
                // Repopulate the SelectList items in case of an error
                model.CategoryOptions = new SelectList(new List<string> { "Shopping", "Restaurant", "Tourism Attraction", "Unknown" });
                model.StateOptions = new SelectList(new List<string> { "Kuala Lumpur", "Selangor", "Sabah", "Sarawak", "Perak", "Pahang", "Negeri Sembilan", "Kedah", "Kelantan", "Terengganu", "Perlis", "Johor", "Pulau Pinang" });
                return View("addAttraction", model);
            }


        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear the session
            return RedirectToAction("Index", "Home"); // Redirect to homepage or login page
        }
       

        [ValidateAntiForgeryToken]
        [HttpPost]
        [SessionCheck]
        public async Task<IActionResult> EditUser(UserModel user, IFormFile imageFile)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Email");
            ModelState.Remove("imageFile");

            string bucketName = "fir-project-7ba4f.appspot.com";
            string newImageUrl = user.ImageUrl;
            var uid = HttpContext.Session.GetString("uid");
            DocumentReference docRef = _firestoreDb.Collection("users").Document(uid);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload the file to Firebase Storage and get the URL
                var uploadResult = await UploadFileToFirebaseStorage(imageFile, bucketName);
                if (uploadResult.Success)
                {
                    newImageUrl = uploadResult.Url; // Update with new URL only if upload succeeds
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload image.");
                }
            }


            if (!ModelState.IsValid)
            {
                // StringBuilder to collect error messages
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {

                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }

                // Log the error messages or return them to the view
                // For demonstration, adding it to the ModelState to show in the view
                ModelState.AddModelError("", $"Validation failed: {errors.ToString()}");

                // Repopulate the SelectList items if model validation fails
                
                return View("EditUser", user);
            }



            if (snapshot.Exists)
            {
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "Name", user.Name },
                    { "HomeAdd", user.HomeAdd },
                    { "PhoneNumber", user.PhoneNumber },
                    { "DateOfBirth", Timestamp.FromDateTime(user.DateOfBirth.ToUniversalTime()) }, // Firestore expects Timestamp
                    { "Gender", user.Gender },
                };
                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    updates.Add("ProfileImage", newImageUrl);
                }

                await docRef.UpdateAsync(updates);

                // Redirect to a confirmation page or back to the user profile
                return RedirectToAction("ListAttractions");
            }
            else
            {
                // Handle the case where the user does not exist
                return NotFound();
            }
        }

        [SessionCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttraction(TourismAttractionViewModel model, IFormFile imageFile)
        {
            model.NameLowercase = model.Name.ToLower();

            if (!string.IsNullOrWhiteSpace(model.Id))
            {
                ModelState.Remove("address"); 
                ModelState.Remove("PostDate");
                ModelState.Remove("PostTime");
                ModelState.Remove("StateOptions");
                ModelState.Remove("CategoryOptions");
                ModelState.Remove("imageFile");
                ModelState.Remove("stringFullDate");
            }
            string bucketName = "fir-project-7ba4f.appspot.com";
            string newImageUrl = model.ImageUrl;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload the file to Firebase Storage and get the URL
                var uploadResult = await UploadFileToFirebaseStorage(imageFile, bucketName);
                if (uploadResult.Success)
                {
                    newImageUrl = uploadResult.Url; // Update with new URL only if upload succeeds
                }
                else
                {
                    ModelState.AddModelError("", "Failed to upload image.");
                    // Optionally log the error or handle further
                }
            }


            if (!ModelState.IsValid)
            {
                // StringBuilder to collect error messages
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {

                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }

                // Log the error messages or return them to the view
                // For demonstration, adding it to the ModelState to show in the view
                ModelState.AddModelError("", $"Validation failed: {errors.ToString()}");

                // Repopulate the SelectList items if model validation fails
                model.CategoryOptions = new SelectList(new List<string> { "Shopping", "Restaurant", "Tourism Attraction", "Unknown" });
                model.StateOptions = new SelectList(new List<string> { "Kuala Lumpur", "Selangor", "Sabah", "Sarawak", "Perak", "Pahang", "Negeri Sembilan", "Kedah", "Kelantan", "Terengganu", "Perlis", "Johor", "Pulau Pinang" });
                return View("EditAttraction", model);
            }


            try
            {
                DocumentReference docRef = _firestoreDb.Collection("Tourism Attractions").Document(model.Id);
                Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "TourismName", model.Name },
            {"TourismNameLowercase",model.NameLowercase},
            { "TourismDesc", model.aDescription },
            { "TourismState", model.State },
            { "TourismCategory", model.Category },
            // Add other fields as necessary
            { "clickRate", model.ClickRate },
            // Assuming you want to update the GeoPoint
            { "point", new GeoPoint(model.Latitude, model.Longitude) }
        };
            if (!string.IsNullOrEmpty(newImageUrl))
        {
            updates.Add("TourismImage", newImageUrl);
        }

                await docRef.UpdateAsync(updates);
                return RedirectToAction("ListAttractions"); // Redirect to the list action/view after successful update
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while updating the attraction. Please try again.");
                // Repopulate the SelectList items in case of an error
                model.CategoryOptions = new SelectList(new List<string> { "Shopping", "Restaurant", "Tourism Attraction", "Unknown" });
                model.StateOptions = new SelectList(new List<string> { "Kuala Lumpur", "Selangor", "Sabah", "Sarawak", "Perak", "Pahang", "Negeri Sembilan", "Kedah", "Kelantan", "Terengganu", "Perlis", "Johor", "Pulau Pinang" });
                return View("EditAttraction", model);
            }
        }
        private async Task<(bool Success, string Url)> UploadFileToFirebaseStorage(IFormFile file, string bucketName)
        {
            try
            {
                var storageClient = StorageClient.Create();
                var fileName = $"attractions/{DateTime.UtcNow.Ticks}-{file.FileName}";
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Specify the predefined ACL to publicRead to make the object publicly readable
                var options = new UploadObjectOptions
                {
                    PredefinedAcl = PredefinedObjectAcl.PublicRead
                };

                var storageObject = await storageClient.UploadObjectAsync(bucketName, fileName, null, memoryStream, options);

                // Construct the public URL
                var fileUrl = $"https://storage.googleapis.com/{bucketName}/{fileName}";
                return (true, fileUrl);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        [SessionCheck]
        public async Task<IActionResult> GetaNotification(String Id) 
        {
            DocumentReference docRef = _firestoreDb.Collection("Notification").Document(Id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var notification = new NotificationModel

                {
                    Id = snapshot.Id,
                    Title = data.ContainsKey("Title") ? data["Title"]?.ToString() : "",
                    body = data.ContainsKey("Body") ? data["Body"]?.ToString() : "",
                    ImageURL = data.ContainsKey("ImageURL") ? data["ImageURL"]?.ToString() : "",
                    date = data.ContainsKey("CreateDate") && data["CreateDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp
                    ? postFullDateTimestamp.ToDateTime()
                    : DateTime.MinValue, 
                    RepostDate = data.ContainsKey("PostDate") && data["PostDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp1
                    ? postFullDateTimestamp1.ToDateTime()
                    : DateTime.MinValue, 
                };

                // Pass the TourismAttractionViewModel to the view
                return View("viewNotification", notification);
            }
            else
            {
                return NotFound();
            }
        }
        [SessionCheck]
        public async Task<IActionResult> GetaNotification1(String Id) 
        {
            DocumentReference docRef = _firestoreDb.Collection("Notification").Document(Id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var notification = new NotificationModel

                {
                    Id = snapshot.Id,
                    Title = data.ContainsKey("Title") ? data["Title"]?.ToString() : "",
                    body = data.ContainsKey("Body") ? data["Body"]?.ToString() : "",
                    ImageURL = data.ContainsKey("ImageURL") ? data["ImageURL"]?.ToString() : "",
                    date = data.ContainsKey("CreateDate") && data["CreateDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp
                    ? postFullDateTimestamp.ToDateTime()
                    : DateTime.MinValue,
                    RepostDate = data.ContainsKey("PostDate") && data["PostDate"] is Google.Cloud.Firestore.Timestamp postFullDateTimestamp1
                    ? postFullDateTimestamp1.ToDateTime()
                    : DateTime.MinValue,
                };

                // Pass the TourismAttractionViewModel to the view
                return View("EditNotification", notification);
            }
            else
            {
                return NotFound();
            }
        }

        // shihui
        public async Task<SocialMediaModel> GetSocialMediaBySocialId(string socialId)
        {

            Query socialMediaQuery = _firestoreDb.Collection("SocialMedia").WhereEqualTo("SocialID", socialId);
            QuerySnapshot socialMediaQuerySnapshot = await socialMediaQuery.GetSnapshotAsync();


            if (socialMediaQuerySnapshot.Documents.Any())
            {

                return socialMediaQuerySnapshot.Documents.First().ConvertTo<SocialMediaModel>();
            }
            else
            {

                return null;
            }
        }
        private async Task<string> GenerateMessageIdAsync()
        {
            var latestMessage = await _firestoreDb.Collection("Message")
                .OrderByDescending("MessageID")
                .Limit(1)
                .GetSnapshotAsync();

            if (latestMessage.Documents.Count > 0)
            {
                var latestMessageId = latestMessage.Documents[0].GetValue<string>("MessageID");
                var latestMessageNumber = int.Parse(latestMessageId.Substring(1));
                var newMessageNumber = latestMessageNumber + 1;
                return "M" + newMessageNumber.ToString().PadLeft(9, '0');
            }
            else
            {
                return "M000000001";
            }
        }

        [SessionCheck]
        public async Task<IActionResult> UserChat()
        {
            if (!UserIsInRole("owner")&&!UserIsInRole("customer support admin")&&!UserIsInRole("admin")) 
            {
                return View("access_lost");
            } 
            else 
            {
                var uid = HttpContext.Session.GetString("_UserId");
                var userDocRef = _firestoreDb.Collection("users").Document(uid);
                var userSnapshot = await userDocRef.GetSnapshotAsync();
                if (userSnapshot.Exists)
                {
                    var userD = userSnapshot.ToDictionary();
                    ViewData["IsLoggedIn"] = !string.IsNullOrEmpty(HttpContext.Session.GetString("_UserToken"));
                    CollectionReference messagesRef = _firestoreDb.Collection("Message");
                    QuerySnapshot messageQuerySnapshot = await messagesRef.GetSnapshotAsync();

                    CollectionReference usersRef = _firestoreDb.Collection("users");
                    QuerySnapshot userQuerySnapshot = await usersRef.GetSnapshotAsync();

                    Dictionary<string, (string, string)> userIdToUserDataMap = userQuerySnapshot.Documents
                        .ToDictionary(doc => doc.Id, doc => (doc.ConvertTo<GetUserModel>().Username, doc.ConvertTo<GetUserModel>().ProfileImage));

                    HashSet<string> displayedUserIds = new HashSet<string>();

                    List<(Message, string, string)> messagesWithUserData = new List<(Message, string, string)>();

                    foreach (DocumentSnapshot messageSnapshot in messageQuerySnapshot.Documents)
                    {
                        Message message = messageSnapshot.ConvertTo<Message>();

                        if (message.RecipientID != uid && message.UserID != uid && message.MessageChannel != "Respond Service" && !displayedUserIds.Contains(message.UserID))
                        {
                            var userData = userIdToUserDataMap.GetValueOrDefault(message.UserID);
                            messagesWithUserData.Add((message, userData.Item1, userData.Item2));
                            displayedUserIds.Add(message.UserID);
                        }
                    }

                    return View(messagesWithUserData);
                }
            }
               
            return RedirectToAction("GetAccuracy");
        }
        public async Task<IActionResult> Chat(string userId)
        {
            var uid = HttpContext.Session.GetString("_UserId");

            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("UserChat");
            }

            var chatChannel = "Customer Service";


            CollectionReference receivedMessageRef = _firestoreDb.Collection("Message");
            QuerySnapshot receivedMessageQuerySnapshot = await receivedMessageRef
                .WhereEqualTo("UserID", userId)
                .WhereEqualTo("MessageChannel", chatChannel)
                .GetSnapshotAsync();

            var receivedMessages = receivedMessageQuerySnapshot.Documents
                .Select(doc => doc.ConvertTo<Message>())
                .Select(message =>
                {
                    message.IsReceived = true;
                    return message;
                })
                .ToList();

            CollectionReference sentMessageRef = _firestoreDb.Collection("Message");
            QuerySnapshot sentMessageQuerySnapshot = await sentMessageRef
                .WhereEqualTo("UserID", uid)
                .WhereEqualTo("RecipientID", userId)
                .WhereEqualTo("MessageChannel", "Respond Service")
                .GetSnapshotAsync();

            var sentMessages = sentMessageQuerySnapshot.Documents
                .Select(doc => doc.ConvertTo<Message>())
                .Select(message =>
                {
                    message.IsSent = true;

                    return message;
                })
                .ToList();

            var chatMessages = receivedMessages.Concat(sentMessages).OrderBy(m => DateTime.Parse(m.MessageDate)).ToList();


            Dictionary<string, string> userIdToUsernameMap = await GetUsernamesForMessages(chatMessages);

            return View((chatMessages, userIdToUsernameMap, userId));
        }
        private async Task<Dictionary<string, string>> GetUsernamesForMessages(List<Message> messages)
        {
            var userIds = messages.Select(m => m.UserID).Distinct().ToList();
            var userIdToUsernameMap = new Dictionary<string, string>();

            CollectionReference usersRef = _firestoreDb.Collection("users");
            QuerySnapshot userQuerySnapshot = await usersRef.WhereIn(FieldPath.DocumentId, userIds).GetSnapshotAsync();

            foreach (var doc in userQuerySnapshot.Documents)
            {
                var userModel = doc.ConvertTo<GetUserModel>();
                userIdToUsernameMap[doc.Id] = userModel.Username;
            }
            return userIdToUsernameMap;
        }
        [HttpPost]
        public async Task<IActionResult?> SendMessage(string userId, string messageText, IFormFile file)
        {

            if (string.IsNullOrWhiteSpace(messageText))
            {

                return RedirectToAction("Chat", new { userId = userId });
            }
            var uid = HttpContext.Session.GetString("_UserId");
            var messageId = await GenerateMessageIdAsync();
            var message = new Message
            {
                UserID = uid,
                RecipientID = userId,
                MessageText = messageText,
                MessageDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageChannel = "Respond Service",
                MessageID = messageId
            };


            if (file != null && file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var storage = new FirebaseStorage("fir-project-7ba4f.appspot.com");

                var imageUrl = await storage.Child("Message").Child(fileName).PutAsync(file.OpenReadStream());


                message.MessageImage = imageUrl;
            }
            await _firestoreDb.Collection("Message").Document(messageId).SetAsync(message);


            return RedirectToAction("Chat", new { userId = userId });
        }
        public async Task<IActionResult> Reports()
        {
                var uid = HttpContext.Session.GetString("_UserId");

                var userDocRef = _firestoreDb.Collection("users").Document(uid);
                var userSnapshot = await userDocRef.GetSnapshotAsync();

                if (userSnapshot.Exists)
                {
                    var userData = userSnapshot.ToDictionary();
                    if (userData.TryGetValue("UserCategory", out var userCategoryObj))
                    {
                        string userCategory = userCategoryObj.ToString();
                        if (userCategory != "Customer Support Admin")
                        {
                            ViewData["IsLoggedIn"] = !string.IsNullOrEmpty(HttpContext.Session.GetString("_UserToken"));
                            List<ReportsModel> reports = await GetReports();
                            return View(reports);
                        }
                        else
                        {
                            ViewData["AlertAccess"] = "Access denied. You do not have permission to access this page.";
                            return RedirectToAction("Index");
                        }
                    }

                }
            return RedirectToAction("Index");
        }
        public async Task<List<ReportsModel>> GetReports()
        {
            List<ReportsModel> reports = new List<ReportsModel>();

            Query reportsQuery = _firestoreDb.Collection("Reports").OrderByDescending("ReportTime");
            QuerySnapshot reportsQuerySnapshot = await reportsQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot reportSnapshot in reportsQuerySnapshot.Documents)
            {
                ReportsModel report = reportSnapshot.ConvertTo<ReportsModel>();
                report.Id = reportSnapshot.Id;
                reports.Add(report);

            }

            return reports;
        }
        public async Task<IActionResult> SocialMedia(string socialId)
        {

            if (string.IsNullOrEmpty(socialId))
            {
                return BadRequest("SocialID is required.");
            }


            SocialMediaModel socialMediaPost = await GetSocialMediaBySocialId(socialId);

            if (socialMediaPost != null)
            {
                return View(socialMediaPost);
            }
            else
            {
                TempData["Message"] = "No post found or post removed.";
                return RedirectToAction("Reports");
            }
        }
        public ActionResult Delete(string socialId, string reportTime)
        {
            var reportsRef = _firestoreDb.Collection("Reports");
            var reportQuery = reportsRef.WhereEqualTo("SocialID", socialId)
                                         .WhereEqualTo("ReportTime", reportTime);
            var reportSnapshot = reportQuery.GetSnapshotAsync().Result;
            var reportDoc = reportSnapshot.Documents.FirstOrDefault();

            if (reportDoc != null)
            {
                var socialMediaRef = _firestoreDb.Collection("SocialMedia").Document(socialId);
                var socialMediaSnapshot = socialMediaRef.GetSnapshotAsync().Result;
                if (socialMediaSnapshot.Exists)
                {
                    socialMediaRef.DeleteAsync();
                }
                else
                {
                    var relatedReportsRef = reportsRef.WhereEqualTo("SocialID", socialId);
                    var relatedReportsSnapshot = relatedReportsRef.GetSnapshotAsync().Result;
                    foreach (var relatedReport in relatedReportsSnapshot.Documents)
                    {
                        relatedReport.Reference.UpdateAsync("ReportResult", "Post Removed");
                    }
                }

                reportDoc.Reference.UpdateAsync("ReportResult", "Post Removed");
            }

            return RedirectToAction("Reports");
        }
        public ActionResult Keep(string socialId, string reportTime)
        {
            var reportsRef = _firestoreDb.Collection("Reports");
            var reportQuery = reportsRef.WhereEqualTo("SocialID", socialId)
                                         .WhereEqualTo("ReportTime", reportTime);
            var reportSnapshot = reportQuery.GetSnapshotAsync().Result;
            var reportDoc = reportSnapshot.Documents.FirstOrDefault();

            if (reportDoc != null)
            {
                var socialMediaRef = _firestoreDb.Collection("SocialMedia").Document(socialId);
                var socialMediaSnapshot = socialMediaRef.GetSnapshotAsync().Result;
                if (!socialMediaSnapshot.Exists)
                {
                    var relatedReportsRef = reportsRef.WhereEqualTo("SocialID", socialId);
                    var relatedReportsSnapshot = relatedReportsRef.GetSnapshotAsync().Result;
                    foreach (var relatedReport in relatedReportsSnapshot.Documents)
                    {
                        relatedReport.Reference.UpdateAsync("ReportResult", "Post Removed");
                    }
                }
                else
                {
                    reportDoc.Reference.UpdateAsync("ReportResult", "Post Remain");
                }
            }

            return RedirectToAction("Reports");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string email, string newRole)
        {
            try
            {

                var userDocRef = _firestoreDb.Collection("users").WhereEqualTo("Email", email);
                var snapshot = await userDocRef.GetSnapshotAsync();


                foreach (var document in snapshot.Documents)
                {
                    var dictionary = document.ToDictionary();
                    dictionary["UserCategory"] = newRole;
                    await document.Reference.SetAsync(dictionary);
                }

                return Ok("User role updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ChangeUserRole()
        {
            if (!UserIsInRole("owner"))
            {
                return View("access_lost");
            }
            else 
            {
                var uid = HttpContext.Session.GetString("_UserId");

                var userDocRef = _firestoreDb.Collection("users").Document(uid);
                var userSnapshot = await userDocRef.GetSnapshotAsync();

                if (userSnapshot.Exists)
                {
                    var userData = userSnapshot.ToDictionary();
                    if (userData.TryGetValue("UserCategory", out var userCategoryObj))
                    {
                        string userCategory = userCategoryObj.ToString();
                        if (userCategory == "Owner" || userCategory == "admin")
                        {
                            ViewData["IsLoggedIn"] = !string.IsNullOrEmpty(HttpContext.Session.GetString("_UserToken"));
                            var userList = await GetAllUsers();

                            return View(userList);
                        }
                        else
                        {
                            ViewData["AlertAccess"] = "Access denied. You do not have permission to access this page.";
                            return RedirectToAction("Index");
                        }
                    }



                }
                return RedirectToAction("Index");
            }     
        }


        private async Task<List<GetUserModel>> GetAllUsers()
        {
            var userList = new List<GetUserModel>();

            QuerySnapshot snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var user = document.ConvertTo<GetUserModel>();
                    userList.Add(user);
                }
            }

            return userList;
        }

    }
}