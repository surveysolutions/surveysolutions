using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Assistant.Settings;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireAssistant(
    IQuestionnaireContextProvider questionnaireContextProvider,
    ILogger<QuestionnaireAssistant> logger) 
    : IQuestionnaireAssistant
{
    IModelSettings modelSettings = new LlamaModelSettings();
    
    
    public async Task<AssistantResult> GetResponseAsync(AssistantRequest request)
    {
        if (modelSettings is OpenAIModelSettings && string.IsNullOrWhiteSpace(modelSettings.ApiKey))
            throw new ArgumentException("OpenAI API key is not configured.");
        
        var messages = request.Messages;
        // Reject any system messages from the client
        if (messages.Any(m => m.Role != null && m.Role.Trim().ToLower() == "system"))
        {
            throw new ArgumentException("System messages are not allowed from the client.");
        }

        if (messages.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                throw new ArgumentException("Either 'messages' or 'prompt' must be provided.");

            messages = new List<AssistantMessage>
            {
                new AssistantMessage("user", request.Prompt)
            };
        }
        
        var systemPrompt = await GetSystemPrompt();
        
        var questionnaireJson = questionnaireContextProvider.GetQuestionnaireContext(request.QuestionnaireId, request.EntityId);
        if (!string.IsNullOrWhiteSpace(questionnaireJson))
        {
            systemPrompt += "\n\nCurrent questionnaire context:\n" + questionnaireJson;
        }
        
        messages.Insert(0, new AssistantMessage("system", systemPrompt));

        
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
                    throw new ArgumentException(responseBody);
                }
                return new AssistantResult() { Answer = responseBody};
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw new ArgumentException("Error communicating with the AI model service. Try again later.");
        }
    }
    
    private async Task<string> GetSystemPrompt()
    {
        string systemPrompt;
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WB.Core.BoundedContexts.Designer.Assistant.Prompts.AssistantSystemPrompt.txt";
                
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
