namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class OptionModel
    {
        public decimal Value { get; set; }
        public string Title { get; set; }
    }

    public class CascadingOptionModel : OptionModel
    {
        public decimal ParentValue { get; set; }
    }
}