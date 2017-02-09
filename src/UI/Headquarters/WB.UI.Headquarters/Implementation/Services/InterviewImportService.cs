using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewImportService : IInterviewImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IInterviewImportDataParsingService interviewImportDataParsingService;

        private readonly object lockStart = new object();

        private IPlainTransactionManager plainTransactionManager => plainTransactionManagerProvider.GetPlainTransactionManager();
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public InterviewImportService(
            ICommandService commandService,
            ILogger logger,
            SampleImportSettings sampleImportSettings, 
            IInterviewImportDataParsingService interviewImportDataParsingService, 
            IQuestionnaireStorage questionnaireStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.commandService = commandService;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
            this.interviewImportDataParsingService = interviewImportDataParsingService;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, bool isPanel, Guid? supervisorId, Guid headquartersId)
        {
            lock (lockStart)
            {
                if (this.Status.IsInProgress == true)
                    return;

                this.Status = new InterviewImportStatus
                {
                    QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                    InterviewImportProcessId = interviewImportProcessId,
                    QuestionnaireVersion = questionnaireIdentity.Version,
                    StartedDateTime = DateTime.Now,
                    CreatedInterviewsCount = 0,
                    ElapsedTime = 0,
                    EstimatedTime = 0,
                    State = {Columns = new string[0], Errors = new List<InterviewImportError>()},
                    IsInProgress = true
                };
            }

            try
            {
                var bigTemplateObject = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                this.Status.QuestionnaireTitle = bigTemplateObject.Title;

                var interviewsToImport = isPanel ? 
                    this.interviewImportDataParsingService.GetInterviewsImportDataForPanel(interviewImportProcessId, questionnaireIdentity):
                    this.interviewImportDataParsingService.GetInterviewsImportDataForSample(interviewImportProcessId, questionnaireIdentity);

                if (interviewsToImport == null)
                {
                    this.Status.State.Errors.Add(new InterviewImportError
                    {
                        ErrorMessage = Interviews.ImportInterviews_IncorrectDatafile
                    });

                    return;
                }

                this.Status.TotalInterviewsCount = interviewsToImport.Length;

                int createdInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();

                Parallel.ForEach(interviewsToImport,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    (importedInterview) =>
                    {
                        if (!supervisorId.HasValue && !importedInterview.SupervisorId.HasValue)
                        {
                            this.Status.State.Errors.Add(new InterviewImportError()
                            {
                                ErrorMessage =
                                    string.Format(Interviews.ImportInterviews_FailedToImportInterview_NoSupervisor, this.FormatInterviewImportData(importedInterview))
                            });
                            return;
                        }
                        var responsibleSupervisorId = importedInterview.SupervisorId ?? supervisorId.Value;
                        try
                        {
                            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                            this.plainTransactionManager.ExecuteInPlainTransaction(
                                () => this.commandService.Execute(
                                    new CreateInterviewWithPreloadedData(
                                        Guid.NewGuid(),
                                        headquartersId,
                                        questionnaireIdentity.QuestionnaireId,
                                        questionnaireIdentity.Version,
                                        supervisorId: responsibleSupervisorId,
                                        interviewerId: importedInterview.InterviewerId,
                                        answersTime: DateTime.UtcNow,
                                        preloadedDataDto: importedInterview.PreloadedData)));
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                string.Format(Interviews.ImportInterviews_GenericError, this.FormatInterviewImportData(importedInterview), responsibleSupervisorId, importedInterview.InterviewerId, questionnaireIdentity, headquartersId, ex.Message);

                            this.logger.Error(errorMessage, ex);

                            this.Status.State.Errors.Add(new InterviewImportError {ErrorMessage = errorMessage});
                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }
                        
                        this.Status.CreatedInterviewsCount = Interlocked.Increment(ref createdInterviewsCount);
                        this.Status.ElapsedTime = elapsedTime.ElapsedMilliseconds;
                        this.Status.TimePerInterview = this.Status.ElapsedTime/this.Status.CreatedInterviewsCount;
                        this.Status.EstimatedTime = this.Status.TimePerInterview*this.Status.TotalInterviewsCount;
                    });

                this.logger.Info($"Imported {this.Status.TotalInterviewsCount:N0} of interviews. Took {elapsedTime.Elapsed:c} to complete");
            }
            finally
            {
                this.Status.IsInProgress = false;
            }
        }

        private string FormatInterviewImportData(InterviewImportData importedInterview)
        {
            return string.Join(", ", importedInterview.PreloadedData.Data[0].Answers.Values.Where(x => x != null));
        }
        
        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}