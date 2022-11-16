using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

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
                answer: answer);

        public AnswerGeoLocationQuestionCommand AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, double latitude = 0,
            double longitude = 0, double accuracy = 0, double altitude = 0, DateTimeOffset timestamp = default(DateTimeOffset))
            => new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
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
                selectedRosterVectors: answer);

        public AnswerMultipleOptionsQuestionCommand AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, int[] answer = null, Guid? questionId = null)
            => new AnswerMultipleOptionsQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: new decimal[0],
                selectedValues: answer);

        [DebuggerStepThrough]
        public AnswerNumericIntegerQuestionCommand AnswerNumericIntegerQuestionCommand(Guid? interviewId = null, 
            Guid? userId = null, Guid? questionId = null, int answer = 0, decimal[] rosterVector = null)
            => new AnswerNumericIntegerQuestionCommand(
                interviewId: interviewId ?? Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: rosterVector ?? new decimal[0],
                answer: answer);

        public AnswerNumericRealQuestionCommand AnswerNumericRealQuestionCommand(Guid interviewId, Guid userId, double answer = 0)
            => new AnswerNumericRealQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answer: answer);

        public AnswerPictureQuestionCommand AnswerPictureQuestionCommand(Guid interviewId, Guid userId, string answer = "")
            => new AnswerPictureQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                pictureFileName: answer);

        public AnswerQRBarcodeQuestionCommand AnswerQRBarcodeQuestionCommand(Guid interviewId, Guid userId, string answer = "")
            => new AnswerQRBarcodeQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answer: answer);

        public AnswerSingleOptionLinkedQuestionCommand AnswerSingleOptionLinkedQuestionCommand(Guid interviewId, Guid userId, decimal[] answer = null)
            => new AnswerSingleOptionLinkedQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                selectedRosterVector: answer);

        public AnswerSingleOptionQuestionCommand AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, int answer = 0, Guid? questionId = null)
            => new AnswerSingleOptionQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: new decimal[0],
                selectedValue: answer);

        public AnswerTextListQuestionCommand AnswerTextListQuestionCommand(Guid interviewId, Guid userId, Tuple<decimal, string>[] answer = null)
            => new AnswerTextListQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answers: answer);

        public AnswerTextQuestionCommand AnswerTextQuestionCommand(Guid interviewId, 
            Guid userId, 
            Guid? questionId = null,
            string answer = "")
            => new AnswerTextQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: new decimal[0],
                answer: answer);

        public AnswerYesNoQuestion AnswerYesNoQuestion(Guid? userId = null,
            Guid? questionId = null, RosterVector rosterVector = null, AnsweredYesNoOption[] answeredOptions = null)
            => new AnswerYesNoQuestion(
                interviewId: Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] { });

        public AnswerYesNoQuestion AnswerYesNoQuestion(Guid interviewId, Guid userId, IEnumerable<AnsweredYesNoOption> answer = default(IEnumerable<AnsweredYesNoOption>))
            => new AnswerYesNoQuestion(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answeredOptions: answer);

        public CloneQuestionnaire CloneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity = null,
            Guid? questionnaireId = null, long? questionnaireVersion = null,
            string newTitle = null,
            long? newQuestionnaireVersion = null,
            string comment = null)
            => new CloneQuestionnaire(
                questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                questionnaireIdentity?.Version ?? questionnaireVersion ?? 777,
                newTitle ?? "New Title of Cloned Copy",
                newQuestionnaireVersion??42,
                Guid.NewGuid(),
                comment: comment);

        public ImportFromDesigner ImportFromDesigner(Guid? questionnaireId = null, string title = "Questionnaire X",
            Guid? responsibleId = null, string base64StringOfAssembly = "<base64>assembly</base64> :)",
            long questionnaireContentVersion = 1,
            string variable = "questionnaire",
            string comment = null)
            => new ImportFromDesigner(
                responsibleId ?? Guid.NewGuid(),
                new QuestionnaireDocument
                {
                    PublicKey = questionnaireId ?? Guid.NewGuid(),
                    Title = title,
                    VariableName = variable
                },
                false,
                base64StringOfAssembly,
                questionnaireContentVersion,
                2,
                comment: comment);
        
        public SynchronizeInterviewEventsCommand SynchronizeInterviewEventsCommand(
            Guid? interviewId = null,
            Guid? userId = null,
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            IEvent[] synchronizedEvents = null,
            InterviewStatus interviewStatus = InterviewStatus.Completed,
            bool createdOnClient = true,
            InterviewKey interviewKey = null,
            Guid? newSupervisorId = null)
        {
            var qId = questionnaireId ?? Guid.NewGuid();
            var uId = userId ?? Guid.NewGuid();
            var qVersion = questionnaireVersion ?? 15;
            return new SynchronizeInterviewEventsCommand(interviewId ?? Guid.NewGuid(), 
                uId, 
                qId, 
                qVersion,
                synchronizedEvents?.Select(Create.Event.AggregateRootEvent).ToArray() ?? new AggregateRootEvent[1] { Create.Event.AggregateRootEvent(Create.Event.InterviewCreated(qId, qVersion)) }, 
                interviewStatus, 
                createdOnClient,
                interviewKey ?? new InterviewKey(Guid.NewGuid().GetHashCode()),
                newSupervisorId);
        }

        public AnswerGeographyQuestionCommand AnswerGeographyQuestionCommand(Guid interviewId, Guid questionId, Guid? userId = null)
            => new AnswerGeographyQuestionCommand(
                interviewId: interviewId,
                userId: userId ?? Guid.NewGuid(),
                questionId: questionId,
                rosterVector: new decimal[0],
                geometry: "",
                mapName:"",
                area:0,
                coordinates:"",
                length:0,
                distanceToEditor:0,
                numberOfPoints:0);

        public DeleteQuestionnaire DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
        {
            return new DeleteQuestionnaire(questionnaireId, questionnaireVersion, responsibleId);
        }

        public DisableQuestionnaire DisableQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
        {
            return new DisableQuestionnaire(questionnaireId, questionnaireVersion, responsibleId);
        }

        public SwitchTranslation SwitchTranslation(string language = null, Guid? interviewId = null)
        {
            return new SwitchTranslation(interviewId ?? Guid.Empty, language, Guid.NewGuid());
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
                synchronizedInterview: synchronizationDto);
        }

        public CreateInterview CreateInterview(Guid interviewId,
            Guid userId,
            Guid questionnaireId,
            long version,
            List<InterviewAnswer> answers,
            Guid supervisorId,
            Guid? interviewerId = null,
            InterviewKey interviewKey = null,
            int? assignmentId = null,
            List<string> protectedAnswers = null,
            InterviewMode interviewMode = InterviewMode.CAPI)
        {
            return new CreateInterview(
                interviewId, 
                userId, 
                Create.Entity.QuestionnaireIdentity(questionnaireId, version), 
                answers, 
                protectedAnswers ?? new List<string>(),
                supervisorId, 
                interviewerId, 
                interviewKey, 
                assignmentId,
                false,
                interviewMode);
        }

        public CreateInterview CreateInterview(Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? responsibleSupervisorId = null,
            List<InterviewAnswer> answersToFeaturedQuestions = null,
            Guid? userId = null,
            List<string> protectedAnswers = null)
        {
            return this.CreateInterview(
                Guid.NewGuid(),
                userId ?? Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1,
                answersToFeaturedQuestions,
                responsibleSupervisorId ?? Guid.NewGuid(),
                null,
                Create.Entity.InterviewKey(),
                null,
                protectedAnswers);
        }

        public CreateInterview CreateInterview(
            Guid? interviewId = null,
            Guid? userId = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            Guid? supervisorId = null,
            InterviewKey interviewKey = null,
            int? assignmentId = null,
            List<InterviewAnswer> answers = null,
            List<string> protectedAnswers = null,
            InterviewMode interviewMode = InterviewMode.CAPI
            )
        {
            return this.CreateInterview(interviewId ?? Guid.NewGuid(),
                userId ?? Guid.NewGuid(), 
                questionnaireIdentity?.QuestionnaireId ?? Guid.NewGuid(),
                questionnaireIdentity?.Version ?? 1,
                answers ?? new List<InterviewAnswer>(),
                supervisorId ?? Guid.NewGuid(),
                userId,
                interviewKey, 
                assignmentId,
                protectedAnswers,
                interviewMode);
        }

        public AssignResponsibleCommand AssignResponsibleCommand(Guid? interviewId = null, 
            Guid? userId = null, 
            Guid? interviewerId = null, 
            Guid? supervisorId = null)
        {
            return new AssignResponsibleCommand(interviewId ?? Guid.NewGuid(),
                userId ?? Guid.NewGuid(),
                interviewerId,
                supervisorId ?? Guid.NewGuid());
        }

        public ResumeInterviewCommand ResumeInterview(Guid interviewId, DateTimeOffset? originDate = null)
        {
            return new ResumeInterviewCommand(interviewId, Guid.NewGuid(), AgentDeviceType.Unknown)
            {
                OriginDate = originDate ?? DateTimeOffset.Now
            };
        }

        public PauseInterviewCommand PauseInterview(Guid interviewId, DateTimeOffset? originDate = null)
        {
            return new PauseInterviewCommand(interviewId, Guid.NewGuid())
            {
                OriginDate = originDate ?? DateTimeOffset.Now
            };
        }

        public CreateTemporaryInterviewCommand CreateTemporaryInterview(Guid? interviewId= null, Guid? userId = null, QuestionnaireIdentity questionnaireId = null)
        {
            return new CreateTemporaryInterviewCommand(interviewId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), questionnaireId ?? Create.Entity.QuestionnaireIdentity());
        }

        public ResolveCommentAnswerCommand ResolveCommentAnswer(Guid? interviewId = null, Identity entityId = null)
        {
            var stubIdentity = Create.Identity();
            return new ResolveCommentAnswerCommand(interviewId ?? Guid.NewGuid(),
                Guid.NewGuid(),
                entityId?.Id ?? stubIdentity.Id,
                entityId?.RosterVector ?? stubIdentity.RosterVector);
        }

        public CreateAssignment CreateAssignment(
            Guid? assignmentId = null,
            int? id = null,
            Guid? userId = null,
            QuestionnaireIdentity questionnaireId = null,
            Guid? responsibleId = null,
            int? quantity = null,
            bool? audioRecording = null,
            string email = null,
            string password = null,
            bool? webMode = null,
            List<InterviewAnswer> answers = null,
            List<string> protectedVariables = null,
            string comment = null)
        {
            return new CreateAssignment(
                assignmentId ?? Guid.NewGuid(), 
                id ?? 11,
                userId ?? Guid.NewGuid(), 
                questionnaireId ?? Create.Entity.QuestionnaireIdentity(),
                responsibleId ?? Guid.NewGuid(),
                quantity,
                audioRecording ?? false,
                email,
                password,
                webMode,
                answers,
                protectedVariables,
                comment);
        }

        public ArchiveAssignment ArchiveAssignment(Guid? assignmentId = null, Guid? userId = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new ArchiveAssignment(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public UnarchiveAssignment UnarchiveAssignment(Guid? assignmentId = null, Guid? userId = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new UnarchiveAssignment(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public DeleteAssignment DeleteAssignment(Guid? assignmentId = null, Guid? userId = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new DeleteAssignment(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public MarkAssignmentAsReceivedByTablet MarkAssignmentAsReceivedByTablet(Guid? assignmentId = null, Guid? userId = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new MarkAssignmentAsReceivedByTablet(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public ReassignAssignment ReassignAssignment(Guid? assignmentId = null, Guid? userId = null, Guid? responsibleId = null, string comment = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new ReassignAssignment(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), responsibleId ?? Guid.NewGuid(), comment, questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public UpdateAssignmentAudioRecording UpdateAssignmentAudioRecording(Guid? assignmentId = null, Guid? userId = null, bool? audioRecording = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new UpdateAssignmentAudioRecording(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), audioRecording ?? false, questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public UpdateAssignmentWebMode UpdateAssignmentWebMode(Guid? assignmentId = null, Guid? userId = null, bool? webMode = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new UpdateAssignmentWebMode(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), webMode ?? false, questionnaireIdentity ?? new QuestionnaireIdentity());
        }
        public UpdateAssignmentQuantity UpdateAssignmentQuantity(Guid? assignmentId = null, Guid? userId = null, int? quantity = null, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new UpdateAssignmentQuantity(assignmentId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), quantity, questionnaireIdentity ?? new QuestionnaireIdentity());
        }

        public UpgradeAssignmentCommand UpgradeAssignment()
        {
            return new UpgradeAssignmentCommand(Id.g1, Id.g2, new QuestionnaireIdentity());
        }

        public CreateCalendarEventCommand CreateCalendarEventCommand(
            Guid? calendarEventId = null,
            Guid? userId = null,
            Guid? interviewId = null,
            string interviewKey = "",
            int assignmentId = 0,
            QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new CreateCalendarEventCommand(
                calendarEventId ?? Guid.NewGuid(),
                userId?? Guid.NewGuid(),
                DateTimeOffset.Now, 
                "America/New_York",
                interviewId,
                interviewKey,
                assignmentId,
                "",
                questionnaireIdentity ?? new QuestionnaireIdentity()
                );
        }
        
        public UpdateCalendarEventCommand UpdateCalendarEventCommand(
            Guid? publicKey = null,
            Guid? userId = null,
            Guid? interviewId = null,
            string interviewKey = "",
            int assignmentId = 0,
            QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new UpdateCalendarEventCommand(
                publicKey ?? Guid.NewGuid(),
                userId?? Guid.NewGuid(),
                DateTimeOffset.Now, 
                "America/New_York",
                "",
                questionnaireIdentity ?? new QuestionnaireIdentity()
            );
        }
    }
}
