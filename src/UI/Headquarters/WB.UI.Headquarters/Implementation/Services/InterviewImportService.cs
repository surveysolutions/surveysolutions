using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Threading;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewImportService : IInterviewImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IInterviewImportDataParsingService interviewImportDataParsingService;

        private IPlainTransactionManager plainTransactionManager => plainTransactionManagerProvider.GetPlainTransactionManager();
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public InterviewImportService(
            ICommandService commandService,
            ILogger logger,
            SampleImportSettings sampleImportSettings, 
            IPreloadedDataRepository preloadedDataRepository, 
            IInterviewImportDataParsingService interviewImportDataParsingService, 
            IQuestionnaireStorage questionnaireStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.commandService = commandService;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
            this.preloadedDataRepository = preloadedDataRepository;
            this.interviewImportDataParsingService = interviewImportDataParsingService;
            this.questionnaireStorage = questionnaireStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId,
            Guid? supervisorId, Guid headquartersId)
        {
            if(this.Status.IsInProgress == true)
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

            try
            {
                var bigTemplateObject = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                this.Status.QuestionnaireTitle = bigTemplateObject.Title;

                var interviewsToImport = this.interviewImportDataParsingService.GetInterviewsImportData(interviewImportProcessId, questionnaireIdentity);
                if (interviewsToImport == null)
                {
                    this.Status.State.Errors.Add(new InterviewImportError()
                    {
                        ErrorMessage = $"Datafile is incorrect"
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
                                    $"Error during import of interview with prefilled questions {FormatInterviewImportData(importedInterview)}. Resposible supervisor is missing"
                            });
                            return;
                        }
                        var responsibleSupervisorId = importedInterview.SupervisorId ?? supervisorId.Value;
                        try
                        {
                            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                            this.plainTransactionManager.ExecuteInPlainTransaction(
                                () => this.commandService.Execute(new CreateInterviewByPrefilledQuestions(
                                    interviewId: Guid.NewGuid(),
                                    responsibleId: headquartersId,
                                    questionnaireIdentity: questionnaireIdentity,
                                    supervisorId: responsibleSupervisorId,
                                    interviewerId: importedInterview.InterviewerId,
                                    answersTime: DateTime.UtcNow,
                                    answersOnPrefilledQuestions: importedInterview.Answers)));
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                $"Error during import of interview with prefilled questions {FormatInterviewImportData(importedInterview)}. " +
                                $"SupervisorId {responsibleSupervisorId}, " +
                                $"InterviewerId {importedInterview.InterviewerId}, " +
                                $"QuestionnaireId {questionnaireIdentity}, " +
                                $"HeadquartersId: {headquartersId}" +
                                $"Exception: {ex.Message}";

                            this.logger.Error(errorMessage, ex);

                            this.Status.State.Errors.Add(new InterviewImportError() {ErrorMessage = errorMessage});
                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }

                        Interlocked.Increment(ref createdInterviewsCount);
                        this.Status.CreatedInterviewsCount = createdInterviewsCount;
                        this.Status.ElapsedTime = elapsedTime.ElapsedMilliseconds;
                        this.Status.TimePerInterview = this.Status.ElapsedTime/this.Status.CreatedInterviewsCount;
                        this.Status.EstimatedTime = this.Status.TimePerInterview*this.Status.TotalInterviewsCount;
                    });

                this.logger.Info(
                    $"Imported {this.Status.TotalInterviewsCount:N0} of interviews. " +
                    $"Took {elapsedTime.Elapsed:c} to complete");
            }
            finally
            {
                this.Status.IsInProgress = false;
            }
        }

        private string FormatInterviewImportData(InterviewImportData importedInterview)
        {
            return string.Join(", ", importedInterview.Answers.Values.Where(x => x != null));
        }

        public bool HasResponsibleColumn(string sampleId)
        {
            using (var csvReader =new CsvReader(new StreamReader(new MemoryStream(this.preloadedDataRepository.GetBytesOfSampleData(sampleId)))))
            {
                this.ConfigureCsvReader(csvReader);

                csvReader.Read();

                var columns = csvReader.FieldHeaders.Select(header => header.Trim().ToLower()).ToArray();
                return columns.Contains(ServiceColumns.ResponsibleColumnName);
            }
        }

        private void ConfigureCsvReader(CsvReader csvReader)
        {
            csvReader.Configuration.Delimiter = this.Status.State.Delimiter;
            csvReader.Configuration.BufferSize = 32768;
            csvReader.Configuration.IgnoreHeaderWhiteSpace = true;
            csvReader.Configuration.IsHeaderCaseSensitive = false;
            csvReader.Configuration.SkipEmptyRecords = true;
            csvReader.Configuration.TrimFields = true;
            csvReader.Configuration.TrimHeaders = true;
            csvReader.Configuration.WillThrowOnMissingField = false;
        }
        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}