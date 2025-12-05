using System.Threading;
using Microsoft.Extensions.Configuration;

namespace WB.Core.BoundedContexts.Designer.Assistant.Settings;


public interface IModelSettings
{
    string ModelName { get; set; }
    string ApiUrl { get; set; }
    string? ApiKey { get; set; }
}

public class AssistantModelSettings : IModelSettings
{
    public string ModelName { get; set; } 
    public string ApiUrl { get; set; } 
    public string? ApiKey { get; set; }

    public AssistantModelSettings(IConfiguration configuration)
    {
        ApiKey = configuration["AssistantApi:ApiKey"];
        ApiUrl = configuration["AssistantApi:ApiUrl"] ?? throw new System.ArgumentNullException("AssistantApi:ApiUrl");
        ModelName = configuration["AssistantApi:ModelName"]?? throw new System.ArgumentNullException("AssistantApi:ModelName");
    }
}
