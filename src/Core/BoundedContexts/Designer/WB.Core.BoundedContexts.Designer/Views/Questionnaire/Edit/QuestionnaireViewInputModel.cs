using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireViewInputModel
    {
        public QuestionnaireViewInputModel(Guid id)
        {
            this.QuestionnaireId = id;
        }
       
        public Guid QuestionnaireId { get; private set; }
    }
}