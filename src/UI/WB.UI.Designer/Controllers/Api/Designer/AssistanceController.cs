using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssistanceController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        public class AssistanceRequest
        {
            public string Prompt { get; set; }
            public List<Message> Messages { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AssistanceRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return BadRequest("OpenAI API key is not configured.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            List<Message> messages = request.Messages;
            if (messages == null || messages.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(request.Prompt))
                {
                    messages = new List<Message> {
                        new Message { Role = "user", Content = request.Prompt }
                    };
                }
                else
                {
                    return BadRequest("Either 'messages' or 'prompt' must be provided.");
                }
            }

            // Reject any system messages from the client
            if (messages.Any(m => m.Role != null && m.Role.Trim().ToLower() == "system"))
            {
                return BadRequest("System messages are not allowed from the client.");
            }

            var payloadObj = new {
                model = "gpt-3.5-turbo",
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList()
            };
            var payload = System.Text.Json.JsonSerializer.Serialize(payloadObj);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseBody);

            return Content(responseBody, "application/json");
        }
    }
}
