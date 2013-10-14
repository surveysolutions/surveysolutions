using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Newtonsoft.Json.Linq;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

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
                    command = GetCreateInterviewCommand((CreateInterviewControllerCommand) command);
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
            switch (answer.Type)
            {
                case QuestionType.Text:
                    return new KeyValuePair<Guid, object>(answer.Id, answer.Answer);

                case QuestionType.AutoPropagate:
                    return new KeyValuePair<Guid, object>(answer.Id, int.Parse(answer.Answer.ToString()));
                case QuestionType.Numeric:
                    if (answer.Settings != null)
                    {
                        if ((bool)answer.Settings.IsInteger)
                            return new KeyValuePair<Guid, object>(answer.Id, int.Parse(answer.Answer.ToString()));
                    }
                    return new KeyValuePair<Guid, object>(answer.Id, decimal.Parse(answer.Answer.ToString()));

                case QuestionType.DateTime:
                    if (answer.Answer is DateTime)
                    {
                        return new KeyValuePair<Guid, object>(answer.Id, (DateTime)answer.Answer);
                    }
                    DateTime resultDate;
                    if (DateTime.TryParse(answer.Answer.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out resultDate))
                        return new KeyValuePair<Guid, object>(answer.Id, resultDate);
                    break;

                case QuestionType.SingleOption:
                    return new KeyValuePair<Guid, object>(answer.Id, decimal.Parse(answer.Answer.ToString()));

                case QuestionType.MultyOption:
                    decimal[] answerAsDecimalArray = JsonArrayToStringArray(answer.Answer).Select(decimal.Parse).ToArray();
                    return new KeyValuePair<Guid, object>(answer.Id, answerAsDecimalArray);
                
            }

            throw new Exception("Unknown question type");
        }

        private static string[] JsonArrayToStringArray(object jsonArray)
        {
            return ((JArray) jsonArray).ToObject<string[]>();
        }
    }
}