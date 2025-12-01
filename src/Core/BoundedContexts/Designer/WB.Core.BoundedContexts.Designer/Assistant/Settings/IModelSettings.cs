using Microsoft.Extensions.Configuration;

namespace WB.Core.BoundedContexts.Designer.Assistant.Settings;


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
