using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories
{
    [Serializable]
    public class CopyCategories : QuestionnaireCommand
    {
        public CopyCategories(
            Guid targetQuestionnaireId,
            Guid responsibleId,
            Guid sourceQuestionnaireId,
            Guid sourceCategoriesId,
            Guid newCategoriesId,
            string name)
            : base(responsibleId: responsibleId, questionnaireId: targetQuestionnaireId)
        {
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceCategoriesId = sourceCategoriesId;
            this.NewCategoriesId = newCategoriesId;
            this.Name = name;
        }

        public Guid SourceQuestionnaireId { get; set; }
        public Guid SourceCategoriesId { get; set; }
        public Guid NewCategoriesId { get; set; }
        public string Name { get; set; }
    }
}
