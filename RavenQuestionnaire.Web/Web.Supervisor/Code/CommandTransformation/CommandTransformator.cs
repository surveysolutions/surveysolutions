using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Newtonsoft.Json.Linq;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace Web.Supervisor.Code.CommandTransformation
{
    public class CommandTransformator
    {
        private static IGlobalInfoProvider globalInfo
        {
            get { return ServiceLocator.Current.GetInstance<IGlobalInfoProvider>(); }
        }

        public ICommand TransformCommnadIfNeeded(string type, ICommand command)
        {
            switch (type)
            {
                case "CreateInterviewCommand":
                    command = GetCreateInterviewCommand((CreateInterviewControllerCommand)command);
                    break;
                case "AnswerDateTimeQuestionCommand":
                case "AnswerMultipleOptionsQuestionCommand":
                case "AnswerNumericRealQuestionCommand":
                    break;
                case "AnswerNumericIntegerQuestionCommand":
                case "AnswerSingleOptionQuestionCommand":
                case "AnswerTextQuestionCommand":
                case "AnswerGeoLocationQuestionCommand":
                case "AssignInterviewerCommand":
                case "DeleteInterviewCommand":
                    break;
            }
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
                                                           command.SupervisorId);
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