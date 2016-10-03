using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public abstract class QuestionnaireEntity : QuestionnaireActive
    {
       public Guid EntityId { get; set; }
    }
}