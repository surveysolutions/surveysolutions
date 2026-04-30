namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class CategoriesView
    {
        public CategoriesView(string categoriesId, string name, string description = "")
        {
            CategoriesId = categoriesId;
            Name = name;
            Description = description;
        }

        public string CategoriesId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
