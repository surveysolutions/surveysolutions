using System;

namespace Main.Core.Events.Questionnaire
{
    public class QuestionnaireItemMoved : QuestionnaireActiveEvent
    {

        public Guid? GroupKey { get; set; }
        public Guid PublicKey { get; set; }

        [Obsolete]
        public Guid QuestionnaireId { get; set; }

        public int TargetIndex { get; set; }
    }
}