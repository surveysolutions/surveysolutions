using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class SharedPersonToQuestionnaireAdded : QuestionnaireActive
    {
        public ShareType ShareType { set; get; }
        public Guid PersonId { get; set; }
        public string Email { get; set; }
    }
}