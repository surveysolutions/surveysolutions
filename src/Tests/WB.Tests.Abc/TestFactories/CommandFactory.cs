using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.Tests.Abc.TestFactories
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

        public AnswerMultipleOptionsLinkedQuestionCommand AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, RosterVector[] answer = null)
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

        public AnswerNumericIntegerQuestionCommand AnswerNumericIntegerQuestionCommand(Guid? interviewId = null, 
            Guid? userId = null, Guid? questionId = null, int answer = 0)
            => new AnswerNumericIntegerQuestionCommand(
                interviewId: interviewId ?? Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
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
            => new CreateInterviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.Now, Guid.NewGuid(), 1, Create.Entity.InterviewKey());

        public CreateInterviewControllerCommand CreateInterviewControllerCommand()
            => new CreateInterviewControllerCommand
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };

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
        
        public SynchronizeInterviewEventsCommand SynchronizeInterviewEventsCommand(
            Guid? interviewId = null,
            Guid? userId = null,
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            IEvent[] synchronizedEvents = null,
            InterviewStatus interviewStatus = InterviewStatus.Completed,
            bool createdOnClient = true,
            InterviewKey interviewKey = null
            )
        {
            return new SynchronizeInterviewEventsCommand(interviewId ?? Guid.NewGuid(), 
                userId ?? Guid.NewGuid(), 
                questionnaireId ?? Guid.NewGuid(), 
                questionnaireVersion ?? 15,
                synchronizedEvents ?? new IEvent[0], 
                interviewStatus, 
                createdOnClient,
                interviewKey ?? new InterviewKey(Guid.NewGuid().GetHashCode()));
        }

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

        public CreateInterviewWithPreloadedData CreateInterviewWithPreloadedData(PreloadedLevelDto[] data = null)
        {
            return new CreateInterviewWithPreloadedData(Guid.NewGuid(), 
                Guid.NewGuid(), Guid.NewGuid(), 1, new PreloadedDataDto(data ?? new PreloadedLevelDto[0]), DateTime.Now, Guid.NewGuid(), null, null, null);
        }

        public SynchronizeInterviewCommand Synchronize(Guid userId, InterviewSynchronizationDto synchronizationDto)
        {
            return new SynchronizeInterviewCommand(
                interviewId: synchronizationDto.Id,
                userId: userId,
                featuredQuestionsMeta: new AnsweredQuestionSynchronizationDto[0],
                createdOnClient: synchronizationDto.CreatedOnClient,
                initialStatus: synchronizationDto.Status,
                sycnhronizedInterview: synchronizationDto);
        }

        public CreateInterviewFromSnapshotCommand CreateInterviewFromSnapshot(Guid userId, InterviewSynchronizationDto synchronizationDto)
        {
            return new CreateInterviewFromSnapshotCommand(
                interviewId: synchronizationDto.Id,
                userId: userId,
                sycnhronizedInterview: synchronizationDto);
        }

        public CreateInterviewOnClientCommand CreateInterviewOnClientCommand(Guid? interviewId = null,
            Guid? userId = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            DateTime? answersTime = null, 
            Guid? supervisorId = null,
            InterviewKey interviewKey = null,
            int? assignmentId = null,
            IReadOnlyDictionary<Guid, AbstractAnswer> answersToIdentifyingQuestions = null)
        {
            return new CreateInterviewOnClientCommand(interviewId ?? Guid.NewGuid(),
                userId ?? Guid.NewGuid(), 
                questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1), 
                answersTime ?? DateTime.UtcNow,
                supervisorId ?? Guid.NewGuid(),
                interviewKey, 
                assignmentId, 
                answersToIdentifyingQuestions ?? new Dictionary<Guid, AbstractAnswer>());
        }
    }
}