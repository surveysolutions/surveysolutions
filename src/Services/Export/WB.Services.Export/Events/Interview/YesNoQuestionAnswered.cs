using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Events.Interview.Dtos;

namespace WB.Services.Export.Events.Interview
{
    public class YesNoQuestionAnswered : QuestionAnswered
    {
        public AnsweredYesNoOption[] AnsweredOptions { get; set; } = new AnsweredYesNoOption[0];
    }
}
