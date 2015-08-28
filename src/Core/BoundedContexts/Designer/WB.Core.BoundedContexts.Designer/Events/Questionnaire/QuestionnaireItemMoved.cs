namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;
    public class QuestionnaireItemMoved : QuestionnaireActiveEvent
    {
        public Guid? GroupKey { get; set; }

        public Guid PublicKey { get; set; }

        [Obsolete]
        public Guid QuestionnaireId { get; set; }

        public int TargetIndex { get; set; }
    }
}