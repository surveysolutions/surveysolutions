using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Assistant;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IModelSettings modelSettings;
        private readonly ILogger<AssistanceController> logger;

        private readonly IQuestionnaireContextProvider questionnaireContextProvider;
        //private readonly IQuestionnaireContextProvider questionnaireContextProvider;

        public AssistanceController(IConfiguration configuration, 
            //IQuestionnaireContextProvider questionnaireContextProvider,
            ILogger<AssistanceController> logger,
            IQuestionnaireContextProvider questionnaireContextProvider)
        {
            this.configuration = configuration;
            //this.questionnaireContextProvider = questionnaireContextProvider;
            this.logger = logger;
            this.questionnaireContextProvider = questionnaireContextProvider;

            // Switch between OpenAIModelSettings and LlamaModelSettings as needed
            // Example: this.modelSettings = new OpenAIModelSettings(configuration);
            this.modelSettings = new Llama32ModelSettings();
        }

        public class Message
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        public class AssistanceRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public List<Message> Messages { get; set; } = new List<Message>();
            public string QuestionnaireId { get; set; } = string.Empty;
            public string EntityId { get; set; } = string.Empty;
            public string Area { get; set; } = string.Empty;
        }

        public interface IModelSettings
        {
            string ModelName { get; set; }
            string ApiUrl { get; set; }
            string? ApiKey { get; set; }
        }

        public class OpenAIModelSettings : IModelSettings
        {
            public string ModelName { get; set; } = "gpt-3.5-turbo";
            public string ApiUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
            public string? ApiKey { get; set; }

            public OpenAIModelSettings(IConfiguration configuration)
            {
                ApiKey = configuration["OpenAI:ApiKey"];
            }
        }

        public class LlamaModelSettings : IModelSettings
        {
            public string ModelName { get; set; } = "llama-3.2-3b-instruct";
            public string ApiUrl { get; set; } = "http://localhost:1234/v1/chat/completions";
            public string? ApiKey { get; set; } = "";
        }

        public class Llama32ModelSettings : IModelSettings
        {
            public string ModelName { get; set; } = "llama3.2";
            public string ApiUrl { get; set; } = "http://localhost:11434/v1/chat/completions";
            public string? ApiKey { get; set; } = "";
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AssistanceRequest request)
        {
            if (modelSettings is OpenAIModelSettings && string.IsNullOrWhiteSpace(modelSettings.ApiKey))
                return BadRequest("OpenAI API key is not configured.");

            var messages = request.Messages;
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
            
            if (!Guid.TryParse(request.QuestionnaireId, out var questionnaireId))
                return  BadRequest("Either 'questionnaireId' must be provided.");

            // Reject any system messages from the client
            if (messages.Any(m => m.Role != null && m.Role.Trim().ToLower() == "system"))
            {
                return BadRequest("System messages are not allowed from the client.");
            }
            
            var systemPrompt = await GetSystemPrompt();
            
            var questionnaireJson = questionnaireContextProvider.GetQuestionnaireContext(questionnaireId);
            if (!string.IsNullOrWhiteSpace(questionnaireJson))
            {
                systemPrompt += "\n\nCurrent questionnaire context:\n" + questionnaireJson;
            }
            
            messages.Insert(0, new Message { Role = "system", Content = systemPrompt });

            
            var payloadObj = new {
                model = modelSettings.ModelName,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
                max_tokens = 700,
                temperature = 0.7,
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, modelSettings.ApiUrl)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payloadObj), System.Text.Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrWhiteSpace(modelSettings.ApiKey))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", modelSettings.ApiKey);
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.SendAsync(requestMessage);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                    return Ok(responseBody);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(406, "Error communicating with the AI model service. Try again later.");
            }
        }

        private async Task<string> GetSystemPrompt()
        {
            string systemPrompt;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "WB.UI.Designer.Resources.AssistantSystemPrompt.txt";
                
                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }
                
                using var reader = new StreamReader(stream);
                systemPrompt = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load system prompt from embedded resources");
                systemPrompt = "You are a helpful AI assistant specialized in Survey Solutions questionnaire design.";
            }

            return systemPrompt;
        }
    }
}
