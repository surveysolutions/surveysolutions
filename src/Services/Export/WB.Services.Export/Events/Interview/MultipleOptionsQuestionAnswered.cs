using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class MultipleOptionsQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedValues { get; set; } = new decimal[0];
    }
}
