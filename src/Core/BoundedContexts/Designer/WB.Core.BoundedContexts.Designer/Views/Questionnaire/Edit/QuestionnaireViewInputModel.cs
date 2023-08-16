using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireViewInputModel
    {
        public QuestionnaireViewInputModel(Guid id, Guid? compileQuestionnaireId = null)
        {
            this.QuestionnaireId = id;
            this.CompileQuestionnaireId = compileQuestionnaireId;
        }
       
        public Guid QuestionnaireId { get; private set; }
        public Guid? CompileQuestionnaireId  { get; private set; }
    }
}
