using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IModelSettings modelSettings;
        private ILogger<AssistanceController> logger;
        //private readonly IQuestionnaireContextProvider questionnaireContextProvider;

        public AssistanceController(IConfiguration configuration, 
            //IQuestionnaireContextProvider questionnaireContextProvider,
            ILogger<AssistanceController> logger)
        {
            this.configuration = configuration;
            //this.questionnaireContextProvider = questionnaireContextProvider;
            this.logger = logger;
            
            // Switch between OpenAIModelSettings and LlamaModelSettings as needed
            // Example: this.modelSettings = new OpenAIModelSettings(configuration);
            this.modelSettings = new LlamaModelSettings();
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

            // Reject any system messages from the client
            if (messages.Any(m => m.Role != null && m.Role.Trim().ToLower() == "system"))
            {
                return BadRequest("System messages are not allowed from the client.");
            }
            
            messages.Insert(0, new Message { Role = "system", Content =
                "You are a helpful AI assistant specialized in Survey Solutions questionnaire design. "
                + "Only provide advice related to questionnaire structure, question types, validation rules, conditional logic, roster design, variable naming conventions, and best practices for survey creation. "
                + "Do not provide or generate any sensitive, private, or personally identifiable information. "
                + "Refuse any requests for legal, medical, financial, or other advice outside your domain. "
                + "Do not assist with or generate content that is harmful, unethical, or violates privacy, copyright, or Survey Solutions policies. "
                + "Never impersonate users, staff, or other entities. "
                + "Provide clear, actionable advice and examples when possible. "
                + "Keep responses concise but informative." });

            //Supply questionnaire context if provided
            
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
    }
    
    public interface IQuestionnaireContextProvider
    {
        string GetQuestionnaireContext(string questionnaireId);
    }
    public class QuestionnaireContextProvider : IQuestionnaireContextProvider
    {
        public string GetQuestionnaireContext(string questionnaireId)
        {
            //Class generation
            //could be too large for complex questionnaires
            //some referenced entities could be missing
            
            //var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            // var questionnaire = this.GetQuestionnaire(id).Source;
            // var supervisorVersion = version ?? this.engineVersionService.LatestSupportedVersion;
            // var package = generationPackageFactory.Generate(questionnaire);
            // var generated = this.expressionProcessorGenerator.GenerateProcessorStateClasses(package, supervisorVersion, inSingleFile: true);
            //
            // var resultBuilder = new StringBuilder();
            //
            // foreach (KeyValuePair<string, string> keyValuePair in generated)
            // {
            //     resultBuilder.AppendLine(string.Format("//{0}", keyValuePair.Key));
            //     resultBuilder.AppendLine(keyValuePair.Value);
            // }
            
            
            //Consider location of assistant call as a context to extract relevant parts of questionnaire
            //traverse questionnaire structure to the root and extract relevant parts
            //current code generation is lacking the description of entities like question texts
            
            // Placeholder implementation
            return $"Context for questionnaire {questionnaireId}";
            
        }
    }
}
