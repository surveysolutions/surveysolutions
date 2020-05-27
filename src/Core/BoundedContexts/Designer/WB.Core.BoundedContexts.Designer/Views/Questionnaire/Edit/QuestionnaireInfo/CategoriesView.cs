namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class CategoriesView
    {
        public CategoriesView(string categoriesId, string name)
        {
            CategoriesId = categoriesId;
            Name = name;
        }

        public string CategoriesId { get; set; }
        public string Name { get; set; }
    }
}
