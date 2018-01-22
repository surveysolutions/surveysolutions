using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage;

namespace CoreTester
{
    public class CoreTestRunner
    {
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IPlainTransactionManagerProvider plainTransactionManager;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireBrowseViewFactory questionnairesBrowseFactory;
        private readonly INativeReadSideStorage<InterviewSummary> interviewSummaries;
        private readonly IEventStore eventStore;

        public CoreTestRunner(
            ICommandService commandService, 
            IQuestionnaireBrowseViewFactory questionnairesBrowseFactory,
            ILogger logger,
            ITransactionManagerProvider transactionManager, 
            INativeReadSideStorage<InterviewSummary> interviewSummaries, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireRepository, 
            IEventStore eventStore, 
            IQuestionnaireStorage questionnaireStorage, IPlainTransactionManagerProvider plainTransactionManager)
        {
            this.commandService = commandService;
            this.questionnairesBrowseFactory = questionnairesBrowseFactory;
            this.logger = logger;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManager = plainTransactionManager;
            this.eventStore = eventStore;
            this.questionnaireRepository = questionnaireRepository;
        }

        public int Run()
        {
            var questionnaireBrowseItems =  this.plainTransactionManager.GetPlainTransactionManager().ExecuteInQueryTransaction(() =>
                questionnairesBrowseFactory.Load(new QuestionnaireBrowseInputModel { PageSize = 1000 }));
            
            foreach (var questionnaireBrowseItem in questionnaireBrowseItems.Items)
            {
                var questionnaireIdentity = new QuestionnaireIdentity(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version);
                var interviewIdsToProcess = GetCompletedInterviewIdsByQuestionnaire(questionnaireIdentity).ToList();
                if (interviewIdsToProcess.Count <= 0) continue;

                var questionnaireRepositoryId = $"{questionnaireBrowseItem.QuestionnaireId.FormatGuid()}${questionnaireBrowseItem.Version}";
                QuestionnaireDocument questionnaire =this.plainTransactionManager.GetPlainTransactionManager().ExecuteInQueryTransaction(() =>
                    questionnaireRepository.GetById(questionnaireRepositoryId));

                // UpdateQuestionnaire(questionnaire);
                questionnaireStorage.StoreQuestionnaire(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version, questionnaire);

                foreach (var interviewId in interviewIdsToProcess)
                {
                    var newInterviewId = Guid.Parse("11111111111111111111111111111111");
                    var userId = Guid.Parse("22222222222222222222222222222222");

                    var committedEvents = this.eventStore.Read(newInterviewId, 0).ToList();

                    try
                    {
                        var createCommand = GetCreateInterviewCommand(committedEvents, interviewId, userId);
                        commandService.Execute(createCommand);
                        ApplyEventOnStatefulInterview(newInterviewId, committedEvents);
                    }
                    catch (InterviewException exception)
                    {
                        this.logger.Debug(
                            exception.ExceptionType == InterviewDomainExceptionType.ExpessionCalculationError
                                ? $"Expression calculation error for interview {interviewId}"
                                : $"Error during processing interview {interviewId}", exception);
                    }
                }
                questionnaireStorage.DeleteQuestionnaireDocument(questionnaireBrowseItem.QuestionnaireId, questionnaireBrowseItem.Version);
            }
            return 0;
        }

        private static ICommand GetCreateInterviewCommand(List<CommittedEvent> committedEvents, Guid interviewId, Guid userId)
        {
            var supervisorAssigned =
                committedEvents.First(x => x.Payload is SupervisorAssigned).Payload as SupervisorAssigned;
            var interviewerAssigned =
                committedEvents.FirstOrDefault(x => x.Payload is InterviewerAssigned)?.Payload as InterviewerAssigned;
            var interviewKey =
                committedEvents.LastOrDefault(x => x.Payload is InterviewKeyAssigned)?.Payload as InterviewKeyAssigned;

            var interviewCreated = committedEvents.FirstOrDefault(x => x.Payload is InterviewCreated)?.Payload as InterviewCreated;
            ICommand createCommand = null;
            if (interviewCreated != null)
            {
                createCommand = new CreateInterview(interviewId, userId,
                    new QuestionnaireIdentity(interviewCreated.QuestionnaireId, interviewCreated.QuestionnaireVersion),
                    new List<InterviewAnswer>(),
                    interviewCreated.CreationTime ?? DateTime.UtcNow,
                    supervisorAssigned.SupervisorId,
                    interviewerAssigned?.InterviewerId,
                    interviewKey?.Key,
                    interviewCreated.AssignmentId
                );
            }
            else
            {
                if (committedEvents.FirstOrDefault(x => x.Payload is InterviewOnClientCreated)?.Payload is
                    InterviewOnClientCreated interviewOnClientCreated)
                {
                    createCommand = new CreateInterview(interviewId, userId,
                        new QuestionnaireIdentity(interviewOnClientCreated.QuestionnaireId,
                            interviewOnClientCreated.QuestionnaireVersion),
                        new List<InterviewAnswer>(),
                        supervisorAssigned.AssignTime ?? DateTime.UtcNow,
                        supervisorAssigned.SupervisorId,
                        interviewerAssigned?.InterviewerId,
                        interviewKey?.Key,
                        interviewOnClientCreated.AssignmentId);
                }
                else if (committedEvents.FirstOrDefault(x => x.Payload is InterviewFromPreloadedDataCreated)?.Payload is
                    InterviewFromPreloadedDataCreated interviewFromPreloadedDataCreated)
                {
                    createCommand = new CreateInterview(interviewId, userId,
                        new QuestionnaireIdentity(interviewFromPreloadedDataCreated.QuestionnaireId,
                            interviewFromPreloadedDataCreated.QuestionnaireVersion),
                        new List<InterviewAnswer>(),
                        supervisorAssigned.AssignTime ?? DateTime.UtcNow,
                        supervisorAssigned.SupervisorId,
                        interviewerAssigned?.InterviewerId,
                        interviewKey?.Key,
                        interviewFromPreloadedDataCreated.AssignmentId);
                }
            }

            return createCommand;
        }

        private void ApplyEventOnStatefulInterview(Guid interviewId, IEnumerable<CommittedEvent> committedEvents)
        {
            foreach (var committedEvent in committedEvents)
            {
                ICommand command = null;
                if (committedEvent.Payload is QuestionAnswered)
                {
                    command = ConvertEventToCommand(interviewId, committedEvent);
                }

                if (command == null)
                    continue;
                commandService.Execute(command);
            }
        }

        private static ICommand ConvertEventToCommand(Guid interviewId, CommittedEvent committedEvent)
        {
            var userId = Guid.NewGuid();
            switch (committedEvent.Payload)
            {
                case AnswerRemoved answerRemoved:
                    return new RemoveAnswerCommand(interviewId, userId, new Identity(answerRemoved.QuestionId, answerRemoved.RosterVector), answerRemoved.RemoveTimeUtc);
                case AreaQuestionAnswered areaQuestion:
                    return new AnswerAreaQuestionCommand(interviewId, userId, 
                        areaQuestion.QuestionId, areaQuestion.RosterVector, areaQuestion.AnswerTimeUtc, areaQuestion.Geometry,
                        areaQuestion.MapName, areaQuestion.AreaSize, areaQuestion.Coordinates, areaQuestion.Length, areaQuestion.DistanceToEditor);
                case AudioQuestionAnswered audioQuestion:
                    return new AnswerAudioQuestionCommand(interviewId, userId, audioQuestion.QuestionId, audioQuestion.RosterVector, audioQuestion.AnswerTimeUtc,
                        audioQuestion.FileName, audioQuestion.Length);
                case DateTimeQuestionAnswered dateTimeQuestion:
                    return new AnswerDateTimeQuestionCommand(interviewId, userId, dateTimeQuestion.QuestionId, dateTimeQuestion.RosterVector, 
                        dateTimeQuestion.AnswerTimeUtc, dateTimeQuestion.Answer);
                case GeoLocationQuestionAnswered geoLocation:
                    return new AnswerGeoLocationQuestionCommand(interviewId, userId, geoLocation.QuestionId, geoLocation.RosterVector, geoLocation.AnswerTimeUtc, 
                        geoLocation.Latitude,geoLocation.Longitude,geoLocation.Accuracy,geoLocation.Altitude, geoLocation.Timestamp);
                case MultipleOptionsLinkedQuestionAnswered multipleOptionsLinked:
                    return new AnswerMultipleOptionsLinkedQuestionCommand(interviewId, userId,multipleOptionsLinked.QuestionId, multipleOptionsLinked.RosterVector,
                        multipleOptionsLinked.AnswerTimeUtc, multipleOptionsLinked.SelectedRosterVectors.Select(x => new RosterVector(x)).ToArray());
                case MultipleOptionsQuestionAnswered multipleOptions:
                    return new AnswerMultipleOptionsQuestionCommand(interviewId, userId, multipleOptions.QuestionId, multipleOptions.RosterVector,
                        multipleOptions.AnswerTimeUtc, multipleOptions.SelectedValues.Select(Convert.ToInt32).ToArray());
                case NumericIntegerQuestionAnswered numericInteger:
                    return new AnswerNumericIntegerQuestionCommand(interviewId, userId, numericInteger.QuestionId, numericInteger.RosterVector,
                        numericInteger.AnswerTimeUtc, numericInteger.Answer);
                case NumericRealQuestionAnswered numericReal:
                    return new AnswerNumericRealQuestionCommand(interviewId, userId, numericReal.QuestionId, numericReal.RosterVector,
                        numericReal.AnswerTimeUtc, Convert.ToDouble(numericReal.Answer));
                case PictureQuestionAnswered picture:
                    return new AnswerPictureQuestionCommand(interviewId, userId, picture.QuestionId, picture.RosterVector,
                        picture.AnswerTimeUtc, picture.PictureFileName);
                case QRBarcodeQuestionAnswered qrBarcode:
                    return new AnswerQRBarcodeQuestionCommand(interviewId, userId, qrBarcode.QuestionId, qrBarcode.RosterVector,
                        qrBarcode.AnswerTimeUtc, qrBarcode.Answer);
                case SingleOptionLinkedQuestionAnswered singleOptionLinked:
                    return new AnswerSingleOptionLinkedQuestionCommand(interviewId, userId, singleOptionLinked.QuestionId, singleOptionLinked.RosterVector,
                        singleOptionLinked.AnswerTimeUtc, singleOptionLinked.SelectedRosterVector);
                case SingleOptionQuestionAnswered singleOption:
                    return new AnswerSingleOptionQuestionCommand(interviewId, userId, singleOption.QuestionId, singleOption.RosterVector, singleOption.AnswerTimeUtc,
                        Convert.ToInt32(singleOption.SelectedValue));
                case TextListQuestionAnswered textList:
                    return new AnswerTextListQuestionCommand(interviewId, userId, textList.QuestionId, textList.RosterVector,
                        textList.AnswerTimeUtc, textList.Answers);
                case TextQuestionAnswered text:
                    return new AnswerTextQuestionCommand(interviewId, userId, text.QuestionId, text.RosterVector, text.AnswerTimeUtc, text.Answer);
                case YesNoQuestionAnswered yesNo:
                    return new AnswerYesNoQuestion(interviewId, userId, yesNo.QuestionId, yesNo.RosterVector, yesNo.AnswerTimeUtc, yesNo.AnsweredOptions);
            }

            return null;
        }

        public IEnumerable<Guid> GetCompletedInterviewIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var skipInterviewsCount = 0;
            var batchInterviews = new List<Guid>();

            var stopwatch = Stopwatch.StartNew();

            do
            {
                var questionnaireIdentityAsString = questionnaireIdentity.ToString();
                batchInterviews = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _.Where(x => x.QuestionnaireIdentity == questionnaireIdentityAsString && x.WasCompleted)
                        .Select(x => x.InterviewId)
                        .Skip(skipInterviewsCount)
                        .Take(1000)
                        .ToList()));

                skipInterviewsCount += batchInterviews.Count;
                this.logger.Debug($"Received {skipInterviewsCount:n0} interview ids.");

                foreach (var interviewId in batchInterviews)
                    yield return interviewId;

            } while (batchInterviews.Count > 0);

            stopwatch.Stop();

            this.logger.Info($"Received {skipInterviewsCount:N0} interviewIds to start export. " +
                             $"Took {stopwatch.Elapsed:g} to complete.");
        }
    }
}