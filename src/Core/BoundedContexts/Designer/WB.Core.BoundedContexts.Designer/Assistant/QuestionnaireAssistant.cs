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
    
    public async Task<AssistantResult> GetResponseAsync(AssistantRequest request, IModelSettings modelSettings)
    {
        var allLoadedGroups = new List<string>();
        var conversationMessages = new List<AssistantMessage>(request.Messages);
        
        // Reject any system messages from the client
        if (conversationMessages.Any(m => m.Role != null && m.Role.Trim().ToLower() == "system"))
            throw new ArgumentException("System messages are not allowed from the client.");

        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Either 'messages' or 'prompt' must be provided.");


        const int maxAttempts = 30;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            logger.LogInformation($"AI Assistant attempt {attempt}/{maxAttempts}. Loaded groups: {string.Join(", ", allLoadedGroups)}");
            
            var result = await MakeAiRequestAsync(modelSettings, request.QuestionnaireId, request.EntityId, request.Prompt,
                conversationMessages, allLoadedGroups);
            
            if (result.Final)
            {
                logger.LogInformation($"AI Assistant completed in {attempt} attempt(s)");
                return result;
            }
            
            if (result.LoadGroups.Any())
            {
                var newGroups = result.LoadGroups.Where(g => !allLoadedGroups.Contains(g)).ToList();
                if (!newGroups.Any())
                {
                    logger.LogWarning("AI requested groups that are already loaded. Breaking the loop.");
                    result.Final = true;
                    return result;
                }
                
                allLoadedGroups.AddRange(newGroups);
                
                conversationMessages.Add(new AssistantMessage("assistant", result.Message));
                conversationMessages.Add(new AssistantMessage("user", 
                    $"Here are the requested groups: {string.Join(", ", newGroups)}. Please continue."));
                
                logger.LogInformation($"Loading additional groups: {string.Join(", ", newGroups)}");
            }
            else
            {
                logger.LogWarning("AI returned final=false but no loadGroups specified. Breaking the loop.");
                result.Final = true;
                return result;
            }
        }
        
        logger.LogError($"AI Assistant exceeded maximum attempts ({maxAttempts})");
        throw new InvalidOperationException($"Failed to get final response after {maxAttempts} attempts");
    }
    
    private async Task<AssistantResult> MakeAiRequestAsync(
        IModelSettings modelSettings,
        Guid questionnaireId,
        Guid entityId,
        string userPrompt,
        List<AssistantMessage> conversationMessages,
        List<string> loadedGroups)
    {
        var systemPrompt = await GetSystemPrompt();
        var messages = new List<AssistantMessage> { new AssistantMessage("system", systemPrompt) };
        messages.AddRange(conversationMessages);
        
        var questionnaireJson = questionnaireContextProvider.GetQuestionnaireContext(
            questionnaireId, 
            entityId, 
            loadedGroups);

        //conversationMessages.Add(new AssistantMessage("user", userPrompt));

        var userPromptWithQuestionnaire = userPrompt + "\n\nCurrent questionnaire context:\n" + questionnaireJson;
        messages.Add(new AssistantMessage("user", userPromptWithQuestionnaire));

        var payloadObj = new {
            model = modelSettings.ModelName,
            messages = messages,
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
                
                // Parse the AI response and extract the assistant's message
                var aiResponse = ParseAiResponse(responseBody);
                
                return new AssistantResult() 
                { 
                    Answer = responseBody,
                    Final = aiResponse?.Final ?? true,
                    LoadGroups = aiResponse?.LoadGroups ?? new List<string>(),
                    Expression = aiResponse?.Expression,
                    Message = aiResponse?.Message ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw new ArgumentException("Error communicating with the AI model service. Try again later.");
        }
    }
    
    private AssistantAiResponse? ParseAiResponse(string responseBody)
    {
        try
        {
            // Parse the OpenAI/LLaMA API response format
            using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
            
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || 
                choices.GetArrayLength() == 0)
            {
                logger.LogWarning("AI response doesn't contain 'choices' array");
                return null;
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var content))
            {
                logger.LogWarning("AI response doesn't contain message content");
                return null;
            }

            var messageContent = content.GetString();
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                logger.LogWarning("AI response message content is empty");
                return null;
            }

            // Try to extract JSON from the message content
            var jsonContent = ExtractJsonFromMessage(messageContent);
            
            // Try to parse the structured response
            try
            {
                var aiResponse = System.Text.Json.JsonSerializer.Deserialize<AssistantAiResponse>(
                    jsonContent, 
                    new System.Text.Json.JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                if (aiResponse != null)
                {
                    logger.LogInformation($"Successfully parsed structured AI response. Final: {aiResponse.Final}");
                    return aiResponse;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // AI didn't return proper JSON structure, treat as unstructured response
                logger.LogWarning("AI returned unstructured response (not JSON). Treating as final response.");
            }
            
            // Fallback: AI didn't follow the JSON format, return a final response with the message
            return new AssistantAiResponse
            {
                Final = true,
                LoadGroups = new List<string>(),
                Expression = null,
                Message = messageContent
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse AI response");
            return null;
        }
    }

    private string ExtractJsonFromMessage(string message)
    {
        var trimmed = message.Trim();
        
        // Check for ```json ... ``` or ``` ... ```
        if (trimmed.Contains("```"))
        {
            var lines = trimmed.Split('\n');
            var jsonLines = new List<string>();
            var inCodeBlock = false;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }
                
                if (inCodeBlock)
                {
                    jsonLines.Add(line);
                }
            }
            
            if (jsonLines.Any())
            {
                var extracted = string.Join('\n', jsonLines).Trim();
                logger.LogDebug($"Extracted JSON from markdown code block: {extracted.Substring(0, Math.Min(100, extracted.Length))}...");
                return extracted;
            }
        }
        
        // Try to find JSON object boundaries
        var startIndex = trimmed.IndexOf('{');
        if (startIndex >= 0)
        {
            // Find matching closing brace
            var braceCount = 0;
            var endIndex = -1;
            
            for (int i = startIndex; i < trimmed.Length; i++)
            {
                if (trimmed[i] == '{') braceCount++;
                if (trimmed[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        endIndex = i;
                        break;
                    }
                }
            }
            
            if (endIndex > startIndex)
            {
                var extracted = trimmed.Substring(startIndex, endIndex - startIndex + 1);
                logger.LogDebug($"Extracted JSON from text: {extracted.Substring(0, Math.Min(100, extracted.Length))}...");
                return extracted;
            }
        }
        
        // No JSON found, return as-is
        logger.LogDebug("No JSON structure found in message, returning as-is");
        return trimmed;
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
