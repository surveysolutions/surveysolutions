using System.Collections.Generic;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.Tests.Web.TestFactories
{
    public class CommandFactory
    {
        internal CreateInterviewControllerCommand CreateInterviewControllerCommand()
            => new CreateInterviewControllerCommand
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };
    }
}
