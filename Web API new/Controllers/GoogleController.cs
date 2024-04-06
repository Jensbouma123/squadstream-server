using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Web_API_new.DTO.Google;

namespace Web_API_new.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GoogleController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private IConfiguration _configuration;

    public GoogleController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        _configuration = configurationBuilder.Build();

    }

    [HttpPost("recaptcha")]
    public async Task<IActionResult> VerifyCaptcha([FromBody] CaptchaRequestDTO recaptchaRequest)
    {
        var client = _httpClientFactory.CreateClient();
        if (recaptchaRequest.Token != "string" || !recaptchaRequest.Token.IsNullOrEmpty())
        {
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _configuration["RecaptchaSettings:SecretKey"]),
                    new KeyValuePair<string, string>("response", recaptchaRequest.Token),
                }));
            
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Captcha is succesvol gevalideerd");
                return Ok(new { success = true });
            }
        }
        Log.Error("Kon recaptcha token niet verifiëren: " + recaptchaRequest.Token);
        return BadRequest();
    }
}