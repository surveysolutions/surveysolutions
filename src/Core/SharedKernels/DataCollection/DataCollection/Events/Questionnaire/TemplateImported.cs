using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.Questionnaire
{
    public class TemplateImported : ILiteEvent
    {
        public QuestionnaireDocument Source { get; set; }
        public bool AllowCensusMode { get; set; }
        public long? Version { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
