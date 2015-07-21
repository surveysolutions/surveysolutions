using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    using System;

    public class SharedPersonToQuestionnaireAdded : QuestionnaireActiveEvent
    {
        public ShareType ShareType { set; get; }
        public Guid PersonId { get; set; }
        public string Email { get; set; }
    }
}