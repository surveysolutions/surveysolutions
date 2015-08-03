namespace WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels
{
    public class TextListQuestionModel : BaseQuestionModel
    {
        public int? MaxAnswerCount { get; set; }

        public bool IsRosterSizeQuestion { get; set; }
    }
}