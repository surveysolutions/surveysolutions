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
        private readonly ISamplePreloadingDataParsingService samplePreloadingDataParsingService;

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            SampleImportSettings sampleImportSettings, 
            IPreloadedDataRepository preloadedDataRepository, 
            ISamplePreloadingDataParsingService samplePreloadingDataParsingService)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.transactionManager = transactionManager;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
            this.preloadedDataRepository = preloadedDataRepository;
            this.samplePreloadingDataParsingService = samplePreloadingDataParsingService;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string sampleId, Guid? supervisorId, Guid headquartersId)
        {
            QuestionnaireDocumentVersioned bigTemplateObject =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.questionnaireDocumentRepository.AsVersioned()
                        .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));

            this.Status = new InterviewImportStatus
            {
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                SampleId = sampleId,
                QuestionnaireVersion = questionnaireIdentity.Version,
                QuestionnaireTitle = bigTemplateObject.Questionnaire.Title,
                StartedDateTime = DateTime.Now,
                CreatedInterviewsCount = 0,
                ElapsedTime = 0,
                EstimatedTime = 0,
                State = { Columns = new string[0] }
            };
            this.Status.State.Errors.Clear();
            this.Status.IsInProgress = true;
            this.Status.State.Errors = new List<InterviewImportError>();

            var dataToPreload = this.samplePreloadingDataParsingService.ParseSample(sampleId, questionnaireIdentity);

            this.Status.TotalInterviewsCount = dataToPreload.Length;
            try
            {

                int createdInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();
                
                Parallel.ForEach(dataToPreload,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    (importedInterview) =>
                    {
                        try
                        {
                            this.commandService.Execute(new CreateInterviewByPrefilledQuestions(
                                interviewId: Guid.NewGuid(),
                                responsibleId: headquartersId,
                                questionnaireIdentity: questionnaireIdentity,
                                supervisorId: importedInterview.SupervisorId ?? supervisorId.Value,
                                interviewerId: importedInterview.InterviewerId,
                                answersTime: DateTime.UtcNow,
                                answersOnPrefilledQuestions: importedInterview.Answers));
                        }
                        catch (Exception ex)
                        {
                            var answersOnPrefilledQuestions = string.Join(", ", importedInterview.Answers.Values.Where(x => x != null));
                            this.logger.Error(
                                $"Error during import of interview with prefilled questions {answersOnPrefilledQuestions}. " +
                                $"QuestionnaireId {questionnaireIdentity}, " +
                                $"HeadquartersId: {headquartersId}", ex);
                        }

                        Interlocked.Increment(ref createdInterviewsCount);
                        this.Status.CreatedInterviewsCount = createdInterviewsCount;
                        this.Status.ElapsedTime = elapsedTime.ElapsedMilliseconds;
                        this.Status.TimePerInterview = this.Status.ElapsedTime / this.Status.CreatedInterviewsCount;
                        this.Status.EstimatedTime = this.Status.TimePerInterview * this.Status.TotalInterviewsCount;
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