using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WeatherApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace WeatherApp.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string apiKey = "fe33e2446aec6b2a1402c297863c6070"; // Geçerli API key

        public WeatherController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                ViewBag.Error = "Lütfen şehir adı girin!";
                return View();
            }

            var cityEncoded = Uri.EscapeDataString(city.Trim());
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityEncoded}&appid={apiKey}&units=metric&lang=tr";

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"API çağrısında hata oluştu: {ex.Message}";
                return View();
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewBag.Error = "API key geçersiz veya aktif değil! Lütfen key’i kontrol edin.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ViewBag.Error = "Girilen şehir bulunamadı. Lütfen doğru şehir adı girin.";
                }
                else
                {
                    ViewBag.Error = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                }

                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            var weather = new WeatherModel
            {
                City = data["name"].ToString(),
                Description = data["weather"][0]["description"].ToString(),
                Temperature = double.Parse(data["main"]["temp"].ToString()),
                Humidity = int.Parse(data["main"]["humidity"].ToString()),
                WindSpeed = double.Parse(data["wind"]["speed"].ToString()),
                Icon = data["weather"][0]["icon"].ToString()
            };

            return View(weather);
        }
    }
}
