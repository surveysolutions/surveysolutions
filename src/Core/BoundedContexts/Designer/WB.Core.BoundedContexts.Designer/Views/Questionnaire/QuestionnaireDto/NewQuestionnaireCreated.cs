using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class NewQuestionnaireCreated : IEvent
    {
        public DateTime CreationDate { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }
    }
}