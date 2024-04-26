using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace webCore.Controllers
{
    public class FirebaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyCuZB5q-EkLEIH3AA1mbAvAfFd7igBO_cs"; // Firebase API key

        public FirebaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "AIzaSyCuZB5q-EkLEIH3AA1mbAvAfFd7igBO_cs";
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var payload = new { requestType = "PASSWORD_RESET", email };
            var jsonPayload = JsonConvert.SerializeObject(payload);

            var response = await _httpClient.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            // Optionally, handle the response
        }
    }
}
