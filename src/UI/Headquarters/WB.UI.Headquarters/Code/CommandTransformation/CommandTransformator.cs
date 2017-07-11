﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    public class CommandTransformator
    {
        private static IAuthorizedUser authorizedUser => ServiceLocator.Current.GetInstance<IAuthorizedUser>();
        private static IQuestionnaireStorage questionnaireStorage => ServiceLocator.Current.GetInstance<IQuestionnaireStorage>();

        public ICommand TransformCommnadIfNeeded(ICommand command, Guid? responsibleId = null)
        {
            if (command is CreateInterviewControllerCommand)
            {
                command = this.GetCreateInterviewCommand((CreateInterviewControllerCommand) command);
            }

            var interviewCommand = command as InterviewCommand;
            if (interviewCommand != null)
            {
                interviewCommand.UserId = authorizedUser.Id;
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

        private CreateInterview GetCreateInterviewCommand(CreateInterviewControllerCommand command)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion);
            var questionnaire = questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var answers = command.AnswersToFeaturedQuestions
                .Select(x => ParseQuestionAnswer(x, questionnaire))
                .Select(x => new InterviewAnswer
                {
                    Identity = new Identity(x.Key, null),
                    Answer = x.Value
                })
                .ToList();

            Guid interviewId = Guid.NewGuid();
            var interviewKey = ServiceLocator.Current.GetInstance<IInterviewUniqueKeyGenerator>().Get();

            var resultCommand = new CreateInterview(interviewId,
                authorizedUser.Id,
                new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion),
                answers,
                DateTime.UtcNow,
                command.SupervisorId,
                null,
                interviewKey,
                null);

            return resultCommand;
        }

        public static KeyValuePair<Guid, AbstractAnswer> ParseQuestionAnswer(UntypedQuestionAnswer answer, IQuestionnaire questionnaire)
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
                        if (questionnaire.IsQuestionInteger(answer.Id))
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
                    if (questionnaire.IsQuestionYesNo(answer.Id))
                    {
                        CheckedYesNoAnswerOption[] answerAsIntArray = JsonArrayToStringArray(answer.Answer)
                            .Select(CheckedYesNoAnswerOption.Parse)
                            .ToArray();
                        answerValue = YesNoAnswer.FromCheckedYesNoAnswerOptions(answerAsIntArray);
                    }
                    else
                    {
                        int[] answerAsIntArray = JsonArrayToStringArray(answer.Answer).Select(x => x.Parse<int>()).ToArray();
                        answerValue = CategoricalFixedMultiOptionAnswer.FromInts(answerAsIntArray);
                    }
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