using System;
using Main.Core.Documents;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CloneQuestionnaireCommand : CommandBase
    {
        public CloneQuestionnaireCommand()
        {
        }

        public CloneQuestionnaireCommand(Guid publicKey, string title, Guid createdBy, IQuestionnaireDocument doc)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.CreatedBy = createdBy;
            this.Title = title;
            this.Source = doc;
        }

        public string Title { get; set; }

        public Guid CreatedBy { get; set; }

        public Guid PublicKey { get; set; }

        public IQuestionnaireDocument Source { get; set; }
    }
}