using Microsoft.AspNetCore.Mvc;
using webCore.Models;
using webCore.Controllers;
using Google.Cloud.Firestore;
using Humanizer;
using static webCore.Models.Notification;
using Google.Cloud.Firestore.V1;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Owin.BuilderProperties;
using System;
using System.Threading.Tasks;
using static webCore.Models.TourismAttraction;
using static webCore.Models.TripAnalysis;
using static webCore.Models.Feedback;
using webCore.Models;
using Microsoft.CodeAnalysis;
using static Google.Rpc.Context.AttributeContext.Types;

namespace webCore.Controllers
{
    public class NotificationController : Controller
    {
        private readonly FirebaseNotificationService _notificationService;
        private readonly FirestoreDb _firestoreDb;

        public NotificationController(FirebaseNotificationService notificationService, FirestoreDb firestoreDb)
        {
            _notificationService = notificationService;
            _firestoreDb = firestoreDb;
        }

        private async Task<(bool Success, string Url)> UploadFileToFirebaseStorage(IFormFile file, string bucketName)
        {
            try
            {
                var storageClient = StorageClient.Create();
                var fileName = $"notification/{DateTime.UtcNow.Ticks}-{file.FileName}";
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



        [HttpPost]
        public async Task<IActionResult> RepostNotification(NotificationModel request)
        {

            bool result = await _notificationService.SendNotificationAsync(
               "eJ9FiR91QLSq9rIr-3LSi5:APA91bE99FMoJrO71mvfp4ruoyHd7JOU3ZQE2klss8zDVQJuHX8TK8bgYFWtCxcvNAIFHOSBH7iWyT3MFw4nhS7LfLBU9YWKdVRa6MwZAOdDA6YkjWZoxbgYrWlS6-lUWWqNIIUaya0K",
                request.Title,
                request.body,
                request.ImageURL);

            if (result)
            {
                DocumentReference docRef = _firestoreDb.Collection("Notification").Document(request.Id);

                DateTime newPostDate = DateTime.UtcNow.AddHours(8);
                var update = await docRef.UpdateAsync("PostDate", Timestamp.FromDateTime(newPostDate.ToUniversalTime()));


                var url = Url.Action("ListNotification", "Firestore");
                return Json(new { success = true, redirectUrl = url });
            }
            else
            {
                return StatusCode(500);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Reply(FeedbackViewModel feedback)
        {
            if (!string.IsNullOrWhiteSpace(feedback.Id))
            {
                ModelState.Remove("Read");
                ModelState.Remove("FeedbackDate");
                ModelState.Remove("FeedbackCategory");
                ModelState.Remove("FeedbackDesc");
                ModelState.Remove("UserID");
                ModelState.Remove("UserName");
                ModelState.Remove("FeedbackLocation");
                ModelState.Remove("FeedbackAccuracy");
                ModelState.Remove("reply");
                ModelState.Remove("replyDate");
                ModelState.Remove("replyID");
            }
            if (feedback.replyID == "" || feedback.replyID == null)
            {

                bool result = await _notificationService.SendNotificationAsync(
                    "eJ9FiR91QLSq9rIr-3LSi5:APA91bE99FMoJrO71mvfp4ruoyHd7JOU3ZQE2klss8zDVQJuHX8TK8bgYFWtCxcvNAIFHOSBH7iWyT3MFw4nhS7LfLBU9YWKdVRa6MwZAOdDA6YkjWZoxbgYrWlS6-lUWWqNIIUaya0K",
                    feedback.FeedbackDesc,
                    feedback.replyDesc,
                    "https://firebasestorage.googleapis.com/v0/b/fir-project-7ba4f.appspot.com/o/download%20(2).jpeg?alt=media&token=3607755c-d0ae-4c8e-8bd5-2459712ea25b");

                if (result)
                {
                    // Update the 'read' field of the feedback document
                    DocumentReference docRef = _firestoreDb.Collection("Feedback").Document(feedback.Id);
                    await docRef.UpdateAsync("read", "false"); // Assuming you want to mark it as read, hence "true"

                    // Add a reply to the 'reply' subcollection of the feedback document
                    CollectionReference replyCollectionRef = docRef.Collection("reply");
                    // Create a reply document with the necessary data
                    Dictionary<string, object> replyData = new Dictionary<string, object>
        {
            { "replyDesc", feedback.replyDesc },
            { "replyDate", DateTime.UtcNow}, // Use server timestamp if possible
            // Add any other fields as necessary
        };
                    await replyCollectionRef.AddAsync(replyData);

                    // Redirect or return a success response
                    return Json(new { redirectUrl = Url.Action("ListFeedback", "Firestore") });
                }
                else
                {
                    return Json(new { error = "An error occurred" });
                }
            }
            else
            {
                return Json(new { error = "An error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(NotificationRequest request, IFormFile imageFile)
        {
           
            string bucketName = "fir-project-7ba4f.appspot.com";
            string newImageUrl = "";
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
                return View("sendNotification");
            }

            if (!string.IsNullOrEmpty(newImageUrl))
            {
                request.ImageUrl = newImageUrl;
            }

            bool result = await _notificationService.SendNotificationAsync(
               "eJ9FiR91QLSq9rIr-3LSi5:APA91bE99FMoJrO71mvfp4ruoyHd7JOU3ZQE2klss8zDVQJuHX8TK8bgYFWtCxcvNAIFHOSBH7iWyT3MFw4nhS7LfLBU9YWKdVRa6MwZAOdDA6YkjWZoxbgYrWlS6-lUWWqNIIUaya0K",
                request.Title,
                request.Body,
                request.ImageUrl);

            if (result)
            {
               
                Dictionary<string, object> adds = new Dictionary<string, object>
        {
            { "Title",request.Title },
            {"Body",request.Body},
            { "ImageURL",  request.ImageUrl ?? "" },
            { "CreateDate",  Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(8)) },
            { "PostDate",  Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(8)) },

        };

                // Add the log to Firestore
                CollectionReference colRef = _firestoreDb.Collection("Notification");
                DocumentReference docRef = await colRef.AddAsync(adds);

                var url = Url.Action("ListNotification", "Firestore");
                return Json(new { success = true, redirectUrl = url });
            }
            else 
            {
                return StatusCode(500);
            }
                
        }
    }

}


