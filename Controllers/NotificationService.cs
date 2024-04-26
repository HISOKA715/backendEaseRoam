using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using webCore.Models;
using Newtonsoft.Json;


namespace webCore.Controllers
{
    public class FirebaseNotificationService
    {
        private readonly HttpClient _httpClient;
        private const string FirebaseUrl = "https://fcm.googleapis.com/fcm/send";
        private const string ServerKey = "AAAANArE01I:APA91bEJ3d2gBp4O_LgPm-r6cr3Kl2uVETRPZt91PIZABeIRRzaYsDOffpT2EMmX6SSE3Fk6c5oK55Ny6AW_z00YWL8fpOpmQhP0UHMVqVAg6-5XRXGoxhfZxOXLB2GVmyW1xg7Ah3NU"; 

        public FirebaseNotificationService(HttpClient httpClient)
        {

            _httpClient = httpClient;
        }
        public async Task<bool> SendNotificationAsync(string to, string title, string body, string imageUrl = null)
        {
            var messageInformation = new
            {
                to, // Recipient device token
                notification = new
                {
                    title,
                    body,
                    image = imageUrl // Include the image URL in the notification payload
                }
            };


            string jsonMessage = System.Text.Json.JsonSerializer.Serialize(messageInformation);
            var request = new HttpRequestMessage(HttpMethod.Post, FirebaseUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("key", $"={ServerKey}");
            request.Headers.TryAddWithoutValidation("Sender", $"id={ServerKey}");
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }



    }
}
