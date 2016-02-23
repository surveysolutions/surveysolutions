using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private static readonly Dictionary<string, SampleCreationStatus> preLoadingStatuses = new Dictionary<string, SampleCreationStatus>();

        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private readonly IPlainTransactionManager transactionManager;
        private readonly SampleImportSettings sampleImportSettings;

        private static ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<SampleImportService>();

        public SampleImportService(
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IPlainTransactionManager transactionManager,
            SampleImportSettings sampleImportSettings, 
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.transactionManager = transactionManager;
            this.sampleImportSettings = sampleImportSettings;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        public void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            preLoadingStatuses[id] = new SampleCreationStatus(id);

            this.CreateInterviewInternal(
                questionnaireId,
                version,
                id,
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtosFromPanelData(data),
                responsibleHeadquarterId,
                responsibleSupervisorId);
        }

        public void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            preLoadingStatuses[id] = new SampleCreationStatus(id);

            this.CreateInterviewInternal(
                questionnaireId, 
                version, 
                id, 
                preloadedDataService => preloadedDataService.CreatePreloadedDataDtoFromSampleData(data), 
                responsibleHeadquarterId, 
                responsibleSupervisorId);
        }

        public SampleCreationStatus GetSampleCreationStatus(string id)
        {
            if (preLoadingStatuses.ContainsKey(id))
                return preLoadingStatuses[id];

            return new SampleCreationStatus() { StatusMessage = "Import not found or completed." };
        }

        void CreateInterviewInternal(Guid questionnaireId,
            long version,
            string id,
            Func<IPreloadedDataService, PreloadedDataRecord[]> preloadedDataDtoCreator,
            Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);

            QuestionnaireDocument bigTemplate;
            QuestionnaireExportStructure questionnaireExportStructure;
            QuestionnaireRosterStructure questionnaireRosterStructure;

            this.transactionManager.BeginTransaction();
            try
            {
                bigTemplate = this.plainQuestionnaireRepository.GetQuestionnaireDocument(questionnaireId, version);
                questionnaireExportStructure = this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireId, version));
                questionnaireRosterStructure = this.questionnaireProjectionsRepository.GetQuestionnaireRosterStructure(new QuestionnaireIdentity(questionnaireId, version));
            }
            finally
            {
                this.transactionManager.RollbackTransaction();
            }
            

            if (bigTemplate == null || questionnaireExportStructure == null || questionnaireRosterStructure == null)
            {
                result.SetErrorMessage("Questionnaire is absent");
                return;
            }

            var preloadedDataService =
                this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                    questionnaireRosterStructure, bigTemplate);

            result.SetStatusMessage("Data parsing");
            var interviewForCreate = preloadedDataDtoCreator(preloadedDataService);

            if (interviewForCreate == null)
            {
                result.SetErrorMessage("Data parsing error");
                return;
            }

            int errorCountOccuredOnInterviewsCreation = 0;
            bool interviewLimitReached = false;
            DateTime startTime = DateTime.Now;
            int totalInterviewsProcessed = 0;

            var commandInvoker = ServiceLocator.Current.GetInstance<ICommandService>();

//            foreach (var preloadedDataRecord in interviewForCreate)
//            {
//                try
//                { 
//                    this.CreatePreloadedInterview(preloadedDataRecord, version, responsibleHeadquarterId, responsibleSupervisorId, commandInvoker, bigTemplate);
//                }
//                catch(InterviewException interviewException)
//                {
//                    Logger.Error(interviewException.Message, interviewException);
//                
//                    if (interviewException.ExceptionType == InterviewDomainExceptionType.InterviewLimitReached)
//                    {
//                        interviewLimitReached = true;
//                        break;
//                    }
//                }
//                catch (Exception e)
//                {
//                    Logger.Error(e.Message, e);
//                
//                    Interlocked.Increment(ref errorCountOccuredOnInterviewsCreation);
//                }
//
//                Interlocked.Increment(ref totalInterviewsProcessed);
//                result.SetStatusMessage(string.Format(
//                        @"Processed {0} interview(s) out of {1}. Spent time: {2:d\.hh\:mm\:ss}. Total estimated time: {3:d\.hh\:mm\:ss}.",
//                        totalInterviewsProcessed,
//                        interviewForCreate.Length,
//                        DateTime.Now - startTime,
//                        CalculateEstimatedTime(totalInterviewsProcessed, interviewForCreate.Length, startTime, DateTime.Now)));
//            }

            var cancellationToken = new CancellationToken();

            Parallel.ForEach(interviewForCreate,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit
                },
                (preloadedDataRecord, loopState) => {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    { 
                        this.CreatePreloadedInterview(preloadedDataRecord, version, responsibleHeadquarterId, responsibleSupervisorId, commandInvoker, bigTemplate);
                    }
                    catch(InterviewException interviewException)
                    {
                        Logger.Error(interviewException.Message, interviewException);

                        if (interviewException.ExceptionType == InterviewDomainExceptionType.InterviewLimitReached)
                        {
                            interviewLimitReached = true;
                            loopState.Stop();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);

                        Interlocked.Increment(ref errorCountOccuredOnInterviewsCreation);
                    }

                    Interlocked.Increment(ref totalInterviewsProcessed);
                    result.SetStatusMessage(string.Format(
                        @"Processed {0} interview(s) out of {1}. Spent time: {2:d\.hh\:mm\:ss}. Total estimated time: {3:d\.hh\:mm\:ss}.",
                        totalInterviewsProcessed,
                        interviewForCreate.Length,
                        DateTime.Now - startTime,
                        CalculateEstimatedTime(totalInterviewsProcessed, interviewForCreate.Length, startTime, DateTime.Now)));
                });

            if (errorCountOccuredOnInterviewsCreation > 0)
            {
                if (interviewLimitReached)
                {
                    result.SetErrorMessage("You can't create more interviews because limit has been reached");
                }
                else
                {
                    result.SetErrorMessage(string.Format("Error{0} occurred during interview creation",
                        errorCountOccuredOnInterviewsCreation == 1 ? "" : "s"));
                }
            }
            else
            {
                result.CompleteProcess();
            }
        }

        private void CreatePreloadedInterview(PreloadedDataRecord preloadedDataRecord,
            long version, 
            Guid responsibleHeadquarterId,
            Guid? responsibleSupervisorId,
            ICommandService commandInvoker,
            QuestionnaireDocument questionnaireDocument)
        {
//            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
//            ThreadMarkerManager.MarkCurrentThreadAsNoTransactional();
//
//            try
//            {
                Guid responsibleId = preloadedDataRecord.SupervisorId ?? responsibleSupervisorId.Value;

                commandInvoker.Execute(
                    new CreateInterviewWithPreloadedData(Guid.NewGuid(), 
                        responsibleHeadquarterId,
                        questionnaireDocument.PublicKey, 
                        version,
                        preloadedDataRecord.PreloadedDataDto,
                        DateTime.UtcNow,
                        responsibleId));
//            }
//            finally
//            {
//                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
//                ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
//            }
        }

        private static TimeSpan CalculateEstimatedTime(int interviewIndex, int totalInterviews, DateTime startTime, DateTime currentTime)
        {
            double spentMilliseconds = (currentTime - startTime).TotalMilliseconds;
            double estimatedMilliseconds = spentMilliseconds / (interviewIndex + 1) * totalInterviews;

            return TimeSpan.FromMilliseconds(estimatedMilliseconds);
        }
    }
}
