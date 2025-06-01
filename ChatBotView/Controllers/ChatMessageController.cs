using ChatBotView.Models.DTO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ChatBotView.Models;

namespace ChatBotView.Controllers
{
    public class ChatMessageController : Controller
    {
       

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatMessageController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7148/");
        }


        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
                return RedirectToAction("Login", "Auth");

            string sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionId", sessionId);
            }

            ViewBag.SessionId = sessionId;

            return View();
        }
     


        [HttpPost]
        public async Task<IActionResult> SendMessage(ChatMessageRequestDto model, int? messageId)
        {
            string token = HttpContext.Session.GetString("JWT");
            string sessionId = HttpContext.Session.GetString("SessionId"); 
            if (string.IsNullOrEmpty(model.SessionId))
            {
                model.SessionId = sessionId;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (messageId.HasValue)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model.Message), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/chat/{messageId.Value}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("History");

                ViewBag.Error = "Could not edit message.";
                ViewBag.SessionId = model.SessionId; 
                return View("Index");
            }
            else
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/chat/send", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ChatMessageResponseDto>(await response.Content.ReadAsStringAsync());
                    ViewBag.SessionId = model.SessionId; 
                    return View("Index", result);
                }

                ViewBag.Error = "Could not send message.";
                ViewBag.SessionId = model.SessionId; 
                return View("Index");
            }
        }



        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            string token = HttpContext.Session.GetString("JWT");
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/chat/history?page=1&pageSize=50");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<List<ChatMessage>>(await response.Content.ReadAsStringAsync());
                return View("History", result);
            }

            ViewBag.Error = "Failed to load history.";
            return View("History", new List<ChatMessage>());
        }


        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string token = HttpContext.Session.GetString("JWT");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"api/chat/{id}");
            return RedirectToAction("History");
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, string updatedText)
        {
            string token = HttpContext.Session.GetString("JWT");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(updatedText), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/chat/{id}", content);

            return RedirectToAction("History");
        }



        [HttpPost]
        public IActionResult PrepareEdit(int id, string message, string sessionId)
        {
            var editMessage = new ChatMessage
            {
                Id = id,
                Message = message,
                SessionId = sessionId
            };

            TempData["EditMessage"] = JsonConvert.SerializeObject(editMessage);
            ViewBag.SessionId = sessionId;
            return RedirectToAction("Index");
        }







    }
}
