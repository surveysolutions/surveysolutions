namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class TextListQuestionModel : BaseQuestionModel
    {
        public int? MaxAnswerCount { get; set; }

        public bool IsRosterSizeQuestion { get; set; }
    }
}