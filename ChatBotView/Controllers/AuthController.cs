using ChatBotView.Models.Auth;
using ChatBotView.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ChatBotView.Controllers
{
    public class AuthController : Controller
    {
    
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7148/api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Registration failed";
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();
        

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model); 
            }
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7148/api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (string.IsNullOrEmpty(data?.Token) || string.IsNullOrEmpty(data?.UserId))
                {
                    ViewBag.Error = "Login failed: Missing user information.";
                    return View(model);
                }

                HttpContext.Session.SetString("JWT", data.Token);
                HttpContext.Session.SetString("UserId", data.UserId);

                return RedirectToAction("Index", "ChatMessage");
            }

            ViewBag.Error = "Login failed";
            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}


