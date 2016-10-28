using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.Tests.Unit.TestFactories
{
    internal class CommandFactory
    {
        public AnswerDateTimeQuestionCommand AnswerDateTimeQuestionCommand(Guid interviewId, Guid userId, DateTime answer = default(DateTime))
            => new AnswerDateTimeQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);

        public AnswerGeoLocationQuestionCommand AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, double latitude = 0,
            double longitude = 0, double accuracy = 0, double altitude = 0, DateTimeOffset timestamp = default(DateTimeOffset))
            => new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                longitude: longitude,
                latitude: latitude,
                accuracy: accuracy,
                altitude: altitude,
                timestamp: timestamp);

        public AnswerMultipleOptionsLinkedQuestionCommand AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, decimal[][] answer = null)
            => new AnswerMultipleOptionsLinkedQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedRosterVectors: answer);

        public AnswerMultipleOptionsQuestionCommand AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, int[] answer = null)
            => new AnswerMultipleOptionsQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedValues: answer);

        public AnswerNumericIntegerQuestionCommand AnswerNumericIntegerQuestionCommand(Guid interviewId, Guid userId, Guid? questionId = null, int answer = 0)
            => new AnswerNumericIntegerQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);

        public AnswerNumericRealQuestionCommand AnswerNumericRealQuestionCommand(Guid interviewId, Guid userId, double answer = 0)
            => new AnswerNumericRealQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);

        public AnswerPictureQuestionCommand AnswerPictureQuestionCommand(Guid interviewId, Guid userId, string answer = "")
            => new AnswerPictureQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                pictureFileName: answer);

        public AnswerQRBarcodeQuestionCommand AnswerQRBarcodeQuestionCommand(Guid interviewId, Guid userId, string answer = "")
            => new AnswerQRBarcodeQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);

        public AnswerSingleOptionLinkedQuestionCommand AnswerSingleOptionLinkedQuestionCommand(Guid interviewId, Guid userId, decimal[] answer = null)
            => new AnswerSingleOptionLinkedQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedRosterVector: answer);

        public AnswerSingleOptionQuestionCommand AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, decimal answer = 0)
            => new AnswerSingleOptionQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedValue: answer);

        public AnswerTextListQuestionCommand AnswerTextListQuestionCommand(Guid interviewId, Guid userId, Tuple<decimal, string>[] answer = null)
            => new AnswerTextListQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answers: answer);

        public AnswerTextQuestionCommand AnswerTextQuestionCommand(Guid interviewId, Guid userId, string answer = "")
            => new AnswerTextQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);

        public AnswerYesNoQuestion AnswerYesNoQuestion(Guid? userId = null,
            Guid? questionId = null, RosterVector rosterVector = null, AnsweredYesNoOption[] answeredOptions = null,
            DateTime? answerTime = null)
            => new AnswerYesNoQuestion(
                interviewId: Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answerTime: answerTime ?? DateTime.UtcNow,
                answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] { });

        public AnswerYesNoQuestion AnswerYesNoQuestion(Guid interviewId, Guid userId, IEnumerable<AnsweredYesNoOption> answer = default(IEnumerable<AnsweredYesNoOption>))
            => new AnswerYesNoQuestion(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answeredOptions: answer);

        public CloneQuestionnaire CloneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity = null,
            Guid? questionnaireId = null, long? questionnaireVersion = null,
            string newTitle = null,
            long? newQuestionnaireVersion = null)
            => new CloneQuestionnaire(
                questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                questionnaireIdentity?.Version ?? questionnaireVersion ?? 777,
                newTitle ?? "New Title of Cloned Copy",
                newQuestionnaireVersion??42,
                Guid.NewGuid());

        public CreateInterviewCommand CreateInterviewCommand()
            => new CreateInterviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.Now, Guid.NewGuid(), 1);

        public CreateInterviewControllerCommand CreateInterviewControllerCommand()
            => new CreateInterviewControllerCommand
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };

        public CreateUserCommand CreateUserCommand(UserRoles role = UserRoles.Operator, string userName = "name", Guid? supervisorId = null)
            => new CreateUserCommand(Guid.NewGuid(), userName, "pass", "e@g.com", new[] { role }, false, false, Create.Entity.UserLight(supervisorId), "", "");

        public ImportFromDesigner ImportFromDesigner(Guid? questionnaireId = null, string title = "Questionnaire X",
            Guid? responsibleId = null, string base64StringOfAssembly = "<base64>assembly</base64> :)",
            long questionnaireContentVersion = 1)
            => new ImportFromDesigner(
                responsibleId ?? Guid.NewGuid(),
                new QuestionnaireDocument
                {
                    PublicKey = questionnaireId ?? Guid.NewGuid(),
                    Title = title,
                },
                false,
                base64StringOfAssembly,
                questionnaireContentVersion,
                2);

        public LinkUserToDevice LinkUserToDeviceCommand(Guid userId, string deviceId)
            => new LinkUserToDevice(userId, deviceId);

        public SynchronizeInterviewEventsCommand SynchronizeInterviewEventsCommand()
            => new SynchronizeInterviewEventsCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, new IEvent[0], InterviewStatus.Completed, true);

        public UnarchiveUserCommand UnarchiveUserCommand(Guid userId)
            => new UnarchiveUserCommand(userId);

        public DeleteQuestionnaire DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
        {
            return new DeleteQuestionnaire(questionnaireId, questionnaireVersion, responsibleId);
        }

        public DisableQuestionnaire DisableQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
        {
            return new DisableQuestionnaire(questionnaireId, questionnaireVersion, responsibleId);
        }

        public SwitchTranslation SwitchTranslation(string language = null)
        {
            return new SwitchTranslation(Guid.Empty, language, Guid.NewGuid());
        }
    }
}