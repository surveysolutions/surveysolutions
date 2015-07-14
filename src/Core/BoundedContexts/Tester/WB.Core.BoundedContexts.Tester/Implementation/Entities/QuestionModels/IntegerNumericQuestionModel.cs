namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class IntegerNumericQuestionModel : BaseQuestionModel
    {
        public bool IsRosterSizeQuestion { get; set; }

        public int? MaxValue { get; set; }
    }
}