using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    [Obsolete]
    public class QuestionnaireAssemblyImported : IEvent
    {
        public long Version { get; set; }
    }
}
