using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories
{
    [Serializable]
    public class AddOrUpdateCategories : QuestionnaireCommand
    {
        public AddOrUpdateCategories(
            Guid questionnaireId,
            Guid responsibleId,
            Guid categoriesId,
            string name,
            Guid? oldCategoriesId,
            string description = "")
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.CategoriesId = categoriesId;
            this.Name = name;
            this.OldCategoriesId = oldCategoriesId;
            this.Description = description;
        }

        public Guid? OldCategoriesId { get; set; }
        public Guid CategoriesId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
