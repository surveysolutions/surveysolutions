using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewImportService : IInterviewImportService
    {
        const string RESPONSIBLECOLUMNNAME = "responsible";

        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository;
        private readonly ICommandService commandService;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IInterviewImportDataParsingService interviewImportDataParsingService;

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            SampleImportSettings sampleImportSettings, 
            IPreloadedDataRepository preloadedDataRepository, 
            IInterviewImportDataParsingService interviewImportDataParsingService)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.transactionManager = transactionManager;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
            this.preloadedDataRepository = preloadedDataRepository;
            this.interviewImportDataParsingService = interviewImportDataParsingService;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId,
            Guid? supervisorId, Guid headquartersId)
        {
            QuestionnaireDocumentVersioned bigTemplateObject =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.questionnaireDocumentRepository.AsVersioned()
                        .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));

            this.Status = new InterviewImportStatus
            {
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                InterviewImportProcessId = interviewImportProcessId,
                QuestionnaireVersion = questionnaireIdentity.Version,
                QuestionnaireTitle = bigTemplateObject.Questionnaire.Title,
                StartedDateTime = DateTime.Now,
                CreatedInterviewsCount = 0,
                ElapsedTime = 0,
                EstimatedTime = 0,
                State = {Columns = new string[0], Errors = new List<InterviewImportError>()}
            };
            this.Status.IsInProgress = true;

            var interviewsToImport = this.interviewImportDataParsingService.GetInterviewsImportData(interviewImportProcessId,
                questionnaireIdentity);

            this.Status.TotalInterviewsCount = interviewsToImport.Length;
            try
            {

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
                            this.commandService.Execute(new CreateInterviewByPrefilledQuestions(
                                interviewId: Guid.NewGuid(),
                                responsibleId: headquartersId,
                                questionnaireIdentity: questionnaireIdentity,
                                supervisorId: responsibleSupervisorId,
                                interviewerId: importedInterview.InterviewerId,
                                answersTime: DateTime.UtcNow,
                                answersOnPrefilledQuestions: importedInterview.Answers));
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
            return string.Join(", ",
                importedInterview.Answers.Values.Where(x => x != null));
        }

        public bool HasResponsibleColumn(string sampleId)
        {
            using (var csvReader =new CsvReader(new StreamReader(new MemoryStream(this.preloadedDataRepository.GetBytesOfSampleData(sampleId)))))
            {
                this.ConfigureCsvReader(csvReader);

                csvReader.Read();

                var columns = csvReader.FieldHeaders.Select(header => header.Trim().ToLower()).ToArray();
                return columns.Contains(RESPONSIBLECOLUMNNAME);
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
            csvReader.Configuration.PrefixReferenceHeaders = true;
            csvReader.Configuration.UseNewObjectForNullReferenceProperties = false;
        }
        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}