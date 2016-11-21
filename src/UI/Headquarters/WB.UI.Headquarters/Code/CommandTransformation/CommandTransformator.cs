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
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Code.CommandTransformation
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

            var rejectCommand = command as RejectInterviewCommand;
            if (rejectCommand != null)
            {
                rejectCommand.RejectTime = DateTime.UtcNow;
            }

            var rejectToInterviewerCommand = command as RejectInterviewToInterviewerCommand;
            if (rejectToInterviewerCommand != null)
            {
                rejectToInterviewerCommand.RejectTime = DateTime.UtcNow;
            }

            var assignCommand = command as AssignInterviewerCommand;
            if (assignCommand != null)
            {
                assignCommand.AssignTime = DateTime.UtcNow;
            }

            var approveCommand = command as ApproveInterviewCommand;
            if (approveCommand != null)
            {
                approveCommand.ApproveTime = DateTime.UtcNow;
            }

            return command;
        }

        private CreateInterviewCommand GetCreateInterviewCommand(CreateInterviewControllerCommand command)
        {
            var answers = command.AnswersToFeaturedQuestions
                .Select(ParseQuestionAnswer)
                .ToDictionary(a => a.Key, a => a.Value);

            Guid interviewId = Guid.NewGuid();

            var resultCommand = new CreateInterviewCommand(interviewId,
                                                           globalInfo.GetCurrentUser().Id,
                                                           command.QuestionnaireId,
                                                           answers,
                                                           DateTime.UtcNow,
                                                           command.SupervisorId, command.QuestionnaireVersion);
            return resultCommand;
        }

        private static KeyValuePair<Guid, AbstractAnswer> ParseQuestionAnswer(UntypedQuestionAnswer answer)
        {
            string answerAsString = answer.Answer.ToString();
            AbstractAnswer answerValue = null;

            switch (answer.Type)
            {
                case QuestionType.Text:
                    answerValue = TextAnswer.FromString(answerAsString);
                    break;
                case QuestionType.Numeric:
                    try
                    {
                        if (answer.Settings != null && (bool) answer.Settings.IsInteger)
                            answerValue = NumericIntegerAnswer.FromInt(answerAsString.Parse<int>());
                        else
                            answerValue = NumericRealAnswer.FromDouble(answerAsString.Parse<double>());
                    }
                    catch (OverflowException)
                    {
                        throw new OverflowException($"Value '{answer.Answer}' is too big or too small.");
                    }
                    break;
                case QuestionType.DateTime:
                    answerValue = DateTimeAnswer.FromDateTime(answer.Answer as DateTime? ?? answerAsString.Parse<DateTime>());
                    break;
                case QuestionType.SingleOption:
                    answerValue = CategoricalFixedSingleOptionAnswer.FromInt(answerAsString.Parse<int>());
                    break;
                case QuestionType.MultyOption:
                    int[] answerAsIntArray = JsonArrayToStringArray(answer.Answer).Select(x=>x.Parse<int>()).ToArray();
                    answerValue = CategoricalFixedMultiOptionAnswer.FromInts(answerAsIntArray);
                    break;
                case QuestionType.GpsCoordinates:
                    var splitedCoordinates = answerAsString.Split('$');
                    if(splitedCoordinates.Length!=2)
                        throw new FormatException($"Value '{answerAsString}' is not in the correct format.");
                    var geoPosition = new GeoPosition(splitedCoordinates[0].Parse<double>(), splitedCoordinates[1].Parse<double>(), 0, 0, DateTime.Now);
                    answerValue = GpsAnswer.FromGeoPosition(geoPosition);
                    break;
            }

            if (answerValue == null)
            {
                throw new Exception("Error when parse question answer");
            }

            return new KeyValuePair<Guid, AbstractAnswer>(answer.Id, answerValue);
        }

        private static string[] JsonArrayToStringArray(object jsonArray)
        {
            return ((JArray)jsonArray).ToObject<string[]>();
        }
    }
}