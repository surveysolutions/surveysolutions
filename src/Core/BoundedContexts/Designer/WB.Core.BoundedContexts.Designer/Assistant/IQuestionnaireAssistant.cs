using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Assistant.Settings;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public interface IQuestionnaireAssistant
{
    Task<AssistantResult> GetResponseAsync(AssistantRequest request, IModelSettings modelSettings);
}
