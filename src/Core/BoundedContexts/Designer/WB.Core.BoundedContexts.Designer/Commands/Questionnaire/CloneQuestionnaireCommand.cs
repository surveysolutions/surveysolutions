namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

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