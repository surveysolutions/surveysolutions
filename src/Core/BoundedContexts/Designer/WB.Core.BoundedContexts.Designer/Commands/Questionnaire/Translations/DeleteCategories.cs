using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations
{
    [Serializable]
    public class DeleteCategories : QuestionnaireCommand
    {
        public DeleteCategories(Guid questionnaireId, Guid responsibleId, Guid categoriesId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.CategoriesId = categoriesId;
        }

        public Guid CategoriesId { get; private set; }
    }
}
