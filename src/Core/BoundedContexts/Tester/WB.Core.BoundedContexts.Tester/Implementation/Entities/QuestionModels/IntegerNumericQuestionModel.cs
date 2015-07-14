namespace WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels
{
    public class IntegerNumericQuestionModel : BaseQuestionModel
    {
        public bool IsRosterSizeQuestion { get; set; }

        public int? MaxValue { get; set; }
    }
}