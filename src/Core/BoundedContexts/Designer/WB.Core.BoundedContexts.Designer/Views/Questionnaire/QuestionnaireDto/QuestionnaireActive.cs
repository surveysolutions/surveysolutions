using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public abstract class QuestionnaireActive
    {
        public Guid ResponsibleId { get; set; }

        protected QuestionnaireActive() {}

        protected QuestionnaireActive(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
        }
    }
}