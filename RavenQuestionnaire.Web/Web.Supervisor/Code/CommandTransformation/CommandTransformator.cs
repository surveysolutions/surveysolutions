using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace Web.Supervisor.Code.CommandTransformation
{
    public class CommandTransformator
    {
        public ICommand TransformCommnadIfNeeded(string type, ICommand command, IGlobalInfoProvider globalInfo)
        {
            switch (type)
            {
                case "CreateInterviewCommand":
                    return GetCreateInterviewCommand((CreateInterviewControllerCommand)command);
                    break;
                case "AnswerDateTimeQuestionCommand":
                    break;
                case "AnswerMultipleOptionsQuestionCommand":
                    break;
                case "AnswerNumericQuestionCommand":
                    break;
                case "AnswerSingleOptionQuestionCommand":
                    break;
                case "AnswerTextQuestionCommand":
                    break;
                case "AnswerGeoLocationQuestionCommand":
                    break;
                case "AssignInterviewerCommand":
                    var assignCommand = (AssignInterviewerCommand) command;
                    return new AssignInterviewerCommand(assignCommand.InterviewId, globalInfo.GetCurrentUser().Id, assignCommand.InterviewerId);
            }

            return command;
        }

        private CreateInterviewCommand GetCreateInterviewCommand(CreateInterviewControllerCommand command)
        {
            var answers = command.AnswersToFeaturedQuestions
                .Select(ParseQuestionAnswer)
                .ToDictionary(a => a.Key, a => a.Value);

            var resultCommand = new CreateInterviewCommand(command.InterviewId,
                                                           command.UserId,
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
                case QuestionType.Numeric:
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
                    return new KeyValuePair<Guid, object>(answer.Id, Guid.Parse(answer.Answer.ToString()));

                case QuestionType.MultyOption:
                    var answerAsDecimalArray = ((string[])answer.Answer).Select(decimal.Parse);
                    return new KeyValuePair<Guid, object>(answer.Id, answerAsDecimalArray);
                case QuestionType.GpsCoordinates:
                    return new KeyValuePair<Guid, object>(answer.Id, new GeoPosition(answer.Answer as string));
            }
            throw new Exception("Unknown question type");
        }
    }
}