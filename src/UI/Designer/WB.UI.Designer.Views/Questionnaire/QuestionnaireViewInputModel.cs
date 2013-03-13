using System;

namespace WB.UI.Designer.Views.Questionnaire
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