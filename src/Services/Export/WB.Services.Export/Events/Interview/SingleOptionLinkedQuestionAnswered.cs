using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class SingleOptionLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedRosterVector { get; set; } = new decimal[0];
        
    }
}
