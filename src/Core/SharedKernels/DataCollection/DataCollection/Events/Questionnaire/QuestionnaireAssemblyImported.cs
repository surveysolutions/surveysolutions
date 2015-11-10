using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class QuestionnaireAssemblyImported : ILiteEvent
    {
        public long Version { get; set; }
    }
}
