using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation
{
    public class CommandTransformator
    {
        private static IGlobalInfoProvider globalInfo
        {
            get { return ServiceLocator.Current.GetInstance<IGlobalInfoProvider>(); }
        }

        public ICommand TransformCommnadIfNeeded(ICommand command)
        {
            TypeSwitch.Do(
                command,
                TypeSwitch.Case<CreateInterviewControllerCommand>(cmd => { command = this.GetCreateInterviewCommand(cmd); }));

            var interviewCommand = command as InterviewCommand;
            if (interviewCommand != null)
            {
                interviewCommand.UserId = globalInfo.GetCurrentUser().Id;
            }

            return command;
        }

        private CreateInterviewCommand GetCreateInterviewCommand(CreateInterviewControllerCommand command)
        {
            var answers = command.AnswersToFeaturedQuestions
                .Select(ParseQuestionAnswer)
                .ToDictionary(a => a.Key, a => a.Value);

            var resultCommand = new CreateInterviewCommand(command.InterviewId,
                                                           globalInfo.GetCurrentUser().Id,
                                                           command.QuestionnaireId,
                                                           answers,
                                                           DateTime.UtcNow,
                                                           command.SupervisorId, command.QuestionnaireVersion);
            return resultCommand;
        }

        private static KeyValuePair<Guid, object> ParseQuestionAnswer(UntypedQuestionAnswer answer)
        {
            string answerAsString = answer.Answer.ToString();
            object answerValue = null;

            switch (answer.Type)
            {
                case QuestionType.Text:
                    answerValue = answer.Answer;
                    break;
                case QuestionType.AutoPropagate:
                    answerValue = answerAsString.Parse<int>();
                    break;
                case QuestionType.Numeric:
                    try
                    {
                        if (answer.Settings != null && (bool) answer.Settings.IsInteger)
                            answerValue = answerAsString.Parse<int>();
                        else
                            answerValue = answerAsString.Parse<decimal>();
                    }
                    catch (OverflowException)
                    {
                        throw new OverflowException(string.Format("Values {0} is too big or too small", answer.Answer));
                    }
                    break;
                case QuestionType.DateTime:
                    answerValue = answer.Answer is DateTime ? answer.Answer : answerAsString.Parse<DateTime>();
                    break;
                case QuestionType.SingleOption:
                    answerValue = answerAsString.Parse<decimal>();
                    break;
                case QuestionType.MultyOption:
                    decimal[] answerAsDecimalArray = JsonArrayToStringArray(answer.Answer).Select(x=>x.Parse<decimal>()).ToArray();
                    answerValue = answerAsDecimalArray;
                    break;
            }

            if (answerValue == null)
            {
                throw new Exception("Error when parse question answer");    
            }

            return new KeyValuePair<Guid, object>(answer.Id, answerValue);
        }

        private static string[] JsonArrayToStringArray(object jsonArray)
        {
            return ((JArray)jsonArray).ToObject<string[]>();
        }
    }
}