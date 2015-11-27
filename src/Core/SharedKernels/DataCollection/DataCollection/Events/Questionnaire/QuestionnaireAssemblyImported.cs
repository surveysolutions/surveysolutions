using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class QuestionnaireAssemblyImported : IEvent
    {
        public long Version { get; set; }
    }
}
