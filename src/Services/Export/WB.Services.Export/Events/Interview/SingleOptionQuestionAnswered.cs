using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class SingleOptionQuestionAnswered : QuestionAnswered
    {
        public decimal SelectedValue { get; set; }

    }
}
