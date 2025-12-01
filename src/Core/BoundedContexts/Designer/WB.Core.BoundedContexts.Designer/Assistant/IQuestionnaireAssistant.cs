using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public interface IQuestionnaireAssistant
{
    Task<AssistantResult> GetResponseAsync(AssistantRequest request);
}
