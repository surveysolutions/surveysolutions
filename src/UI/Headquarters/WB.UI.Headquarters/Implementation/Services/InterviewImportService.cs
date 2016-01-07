using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
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
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using GuidExtensions = WB.Core.GenericSubdomains.Portable.GuidExtensions;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewImportService : IInterviewImportService
    {
        const string RESPONSIBLECOLUMNNAME = "responsible";

        private static class GeoPositionExtensions
        {
            private static bool IsLatitude(string exportColumnName)
            {
                return exportColumnName.IndexOf(nameof (GeoPosition.Latitude), StringComparison.OrdinalIgnoreCase) > -1;
            }

            private static bool IsLongtitude(string exportColumnName)
            {
                return exportColumnName.IndexOf(nameof(GeoPosition.Longitude), StringComparison.OrdinalIgnoreCase) > -1;
            }

            public static bool IsRequiredGeoPositionColumn(string exportColumnName)
            {
                return IsLatitude(exportColumnName) || IsLongtitude(exportColumnName);
            }

            public static string GetLatitideColumnName(string prefilledQuestionVariable)
            {
                return $"{prefilledQuestionVariable}{QuestionDataParser.ColumnDelimiter}{nameof(GeoPosition.Latitude)}"
                    .ToLower();
            }

            public static string GetLongitugeColumnName(string prefilledQuestionVariable)
            {
                return $"{prefilledQuestionVariable}{QuestionDataParser.ColumnDelimiter}{nameof(GeoPosition.Longitude)}"
                    .ToLower();
            }
        }

        private class GeoPosition
        {
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public double? Accuracy { get; set; }
            public double? Altitude { get; set; }
            public DateTimeOffset? Timestamp { get; set; }

        }

        private class ImportedInterview
        {
            public Guid? SupervisorId { get; set; }
            public Guid? InterviewerId { get; set; }
            public Dictionary<Guid, object> AnswersOnPrefilledQuestions { get; set; }
        }

        private class FileInterviews
        {
            public List<ImportedInterview> Imported { get; set; }
            public List<InterviewImportError> WithErrors { get; set; }
        }

        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository;
        private readonly ICommandService commandService;
        private readonly IUserViewFactory userViewFactory;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly ILogger logger;
        private readonly IViewFactory<SampleUploadViewInputModel, SampleUploadView> sampleUploadViewFactory;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly IPreloadedDataRepository preloadedDataRepository;

        private readonly ConcurrentDictionary<string, UserView> usersCache = new ConcurrentDictionary<string, UserView>();

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            IUserViewFactory userViewFactory,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            IViewFactory<SampleUploadViewInputModel, SampleUploadView> sampleUploadViewFactory,
            SampleImportSettings sampleImportSettings, IPreloadedDataRepository preloadedDataRepository)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.userViewFactory = userViewFactory;
            this.transactionManager = transactionManager;
            this.logger = logger;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
            this.sampleImportSettings = sampleImportSettings;
            this.preloadedDataRepository = preloadedDataRepository;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string sampleId, Guid? supervisorId, Guid headquartersId)
        {

            InterviewImportFileDescription fileDescription;
            
            try
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                fileDescription = this.GetDescriptionByFileWithInterviews(questionnaireIdentity, sampleId);
            }
            finally
            {
                this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
            }
            
            var columnsWithTypes = GetFileColumnsWithTypes(fileDescription);
            var dynamicTypeOfImportedInterview = columnsWithTypes.ToDynamicType("interview");

            this.Status = new InterviewImportStatus
            {
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                SampleId = sampleId, 
                QuestionnaireVersion = questionnaireIdentity.Version,
                QuestionnaireTitle = fileDescription.QuestionnaireTitle,
                StartedDateTime = DateTime.Now,
                CreatedInterviewsCount = 0,
                ElapsedTime = 0,
                EstimatedTime = 0,
                State = {Columns = fileDescription.FileColumns}
            };
            this.Status.State.Errors.Clear();
            this.Status.IsInProgress = true;

            var fileInterviews = this.ReadInterviewsFromFile(dynamicTypeOfImportedInterview, fileDescription);
            this.Status.TotalInterviewsCount = fileInterviews.Imported.Count + fileInterviews.WithErrors.Count;
            this.Status.State.Errors = fileInterviews.WithErrors;

            try
            {

                int createdInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();
                
                Parallel.ForEach(fileInterviews.Imported,
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
                                answersOnPrefilledQuestions: importedInterview.AnswersOnPrefilledQuestions));
                        }
                        catch (Exception ex)
                        {
                            var answersOnPrefilledQuestions = string.Join(", ", importedInterview.AnswersOnPrefilledQuestions.Values.Where(x => x != null));
                            this.logger.Error(
                                $"Error during import of interview with prefilled questions {answersOnPrefilledQuestions}. " +
                                $"QuestionnaireId {questionnaireIdentity}, " +
                                $"HeadquartersId: {headquartersId}", ex);
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

        private FileInterviews ReadInterviewsFromFile(Type dynamicTypeOfImportedInterview,
            InterviewImportFileDescription fileDescription)
        {
            var fileInterviews = new FileInterviews
            {
                Imported = new List<ImportedInterview>(),
                WithErrors = new List<InterviewImportError>()
            };

            using (var csvReader = new CsvReader(new StreamReader(new MemoryStream(this.preloadedDataRepository.GetBytesOfSampleData(fileDescription.SampleId)))))
            {
                this.ConfigureCsvReader(csvReader);

                var interviewMap = csvReader.Configuration.AutoMap(dynamicTypeOfImportedInterview);
                foreach (var prefilledGpsQuestion in fileDescription.PrefilledQuestions.Where(x => x.IsGps))
                {
                    var geoPositionReferenceMap = new CsvPropertyReferenceMap(
                        dynamicTypeOfImportedInterview.GetProperty(prefilledGpsQuestion.Variable),
                        csvReader.Configuration.AutoMap<GeoPosition>());

                    geoPositionReferenceMap.Prefix($"{prefilledGpsQuestion.Variable}{QuestionDataParser.ColumnDelimiter}");
                    interviewMap.ReferenceMaps.Add(geoPositionReferenceMap);
                }
                csvReader.Configuration.RegisterClassMap(interviewMap);

                while (csvReader.Read())
                {
                    try
                    {
                        dynamic dynamicImportedInterview = csvReader.GetRecord(dynamicTypeOfImportedInterview);

                        Guid? supervisorId = null;
                        Guid? interviewerId = null;

                        if (fileDescription.HasResponsibleColumn)
                        {
                            string responsibleName = dynamicImportedInterview.responsible;
                            var responsible = this.GetResponsibleByName(responsibleName);
                            if (responsible == null)
                            {
                                throw new Exception($"responsible '{responsibleName}' not found");
                            }

                            if (responsible.Supervisor == null)
                            {
                                supervisorId = responsible.PublicKey;
                            }
                            else
                            {
                                interviewerId = responsible.PublicKey;
                                supervisorId = responsible.Supervisor.Id;
                            }
                        }

                        var answersOnPrefilledQuestions = this.GetAnswersOnPrefilledQuestionsOrThrow(
                            fileDescription.PrefilledQuestions, TypeExtensions.ToDictionary(dynamicImportedInterview));

                        fileInterviews.Imported.Add(new ImportedInterview
                        {
                            SupervisorId = supervisorId,
                            InterviewerId = interviewerId,
                            AnswersOnPrefilledQuestions = answersOnPrefilledQuestions
                        });

                    }
                    catch (Exception ex)
                    {
                        fileInterviews.WithErrors.Add(new InterviewImportError
                        {
                            RawData = csvReader.CurrentRecord,
                            ErrorMessage = ex.Data.Contains("CsvHelper")
                                ? ToUserFriendlyErrorMessage((string) ex.Data["CsvHelper"])
                                : ex.Message
                        });
                    }
                }
            }

            return fileInterviews;
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

        private static string ToUserFriendlyErrorMessage(string csvExceptionAsString)
        {
            return csvExceptionAsString.Substring(csvExceptionAsString.IndexOf("Field Name", StringComparison.Ordinal)).Replace("\r\n", ", ");
        }

        private static Dictionary<string, Type> GetFileColumnsWithTypes(InterviewImportFileDescription fileDescription)
        {
            var columnsWithTypes = new Dictionary<string, Type>(fileDescription.PrefilledQuestions
                .ToDictionary(column => column.Variable, column => column.AnswerType));

            if (fileDescription.HasResponsibleColumn)
            {
                columnsWithTypes.Add(RESPONSIBLECOLUMNNAME, typeof (string));
            }
            return columnsWithTypes;
        }

        public InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, string sampleId)
        {
            var questionnaireDocument = this.questionnaireDocumentRepository.AsVersioned()
                .Get(GuidExtensions.FormatGuid(questionnaireIdentity.QuestionnaireId), questionnaireIdentity.Version).Questionnaire;

            var interviewImportFileDescription = new InterviewImportFileDescription()
            {
                ColumnsByPrefilledQuestions = new List<InterviewImportColumn>(),
                QuestionnaireTitle = $"(ver. {questionnaireIdentity.Version}) {questionnaireDocument.Title}",
                SampleId = sampleId
            };
            
            using (var csvReader = new CsvReader(new StreamReader(new MemoryStream(this.preloadedDataRepository.GetBytesOfSampleData(sampleId)))))
            {
                this.ConfigureCsvReader(csvReader);
                
                csvReader.Read();

                var columns = interviewImportFileDescription.FileColumns = csvReader.FieldHeaders.Select(header => header.Trim().ToLower()).ToArray();

                var presentPrefilledQuestion =
                    questionnaireDocument.Find<IQuestion>(
                        x => x.Featured && columns.Contains(x.StataExportCaption.Trim().ToLower())).ToArray();

                interviewImportFileDescription.PrefilledQuestions = presentPrefilledQuestion
                    .Select(question => this.ToInterviewImportPrefilledQuestion(question, questionnaireDocument))
                    .ToList();

                interviewImportFileDescription.HasResponsibleColumn = columns.Contains(RESPONSIBLECOLUMNNAME);

                var exportColumnsByPrefilledQuestions = this.sampleUploadViewFactory.Load(
                    new SampleUploadViewInputModel(questionnaireIdentity.QuestionnaireId,
                        questionnaireIdentity.Version)).ColumnListToPreload;

                foreach (var exportColumnByPrefilledQuestion in exportColumnsByPrefilledQuestions)
                {
                    var prefilledQuestion = presentPrefilledQuestion.FirstOrDefault(question => question.PublicKey == exportColumnByPrefilledQuestion.Id);
                    interviewImportFileDescription.ColumnsByPrefilledQuestions.Add(
                        new InterviewImportColumn
                        {
                            ColumnName = exportColumnByPrefilledQuestion.Caption,
                            IsRequired = (prefilledQuestion != null && prefilledQuestion.QuestionType != QuestionType.GpsCoordinates) ||
                                GeoPositionExtensions.IsRequiredGeoPositionColumn(exportColumnByPrefilledQuestion.Caption),
                            ExistsInFIle = columns.Contains(exportColumnByPrefilledQuestion.Caption.ToLower())
                        });
                }
            }
            
            return interviewImportFileDescription;
        }

        private InterviewImportPrefilledQuestion ToInterviewImportPrefilledQuestion(IQuestion prefilledQuestion,
            QuestionnaireDocument questionnaireDocument)
        {
            return new InterviewImportPrefilledQuestion
            {
                QuestionId = prefilledQuestion.PublicKey,
                Variable = prefilledQuestion.StataExportCaption.ToLower(),
                AnswerType = this.GetTypeOfAnswer(prefilledQuestion),
                IsGps = prefilledQuestion.QuestionType == QuestionType.GpsCoordinates
            };
        }

        private UserView GetResponsibleByName(string userName)
        {
            userName = userName.ToLower();
            if (!this.usersCache.Keys.Contains(userName))
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                try
                {
                    var user = this.userViewFactory.Load(new UserViewInputModel(userName, null));
                    return this.usersCache.GetOrAdd(userName, user);
                }
                finally
                {
                    this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
                }
            }

            return this.usersCache[userName];
        }

        private Dictionary<Guid, object> GetAnswersOnPrefilledQuestionsOrThrow(List<InterviewImportPrefilledQuestion> prefilledQuestions, 
            IDictionary<string, object> importedInterview)
        {
            var answersOnPrefilledQuestions = new Dictionary<Guid, object>();
            foreach (var prefilledQuestion in prefilledQuestions)
            {
                var prefilledQuestionVariable = prefilledQuestion.Variable;
                if (prefilledQuestion.IsGps)
                {
                    var answerOnGpsQuestion = (GeoPosition)importedInterview[prefilledQuestionVariable];

                    if (!answerOnGpsQuestion.Latitude.HasValue)
                    {
                        throw new Exception($"Field Name: '{GeoPositionExtensions.GetLatitideColumnName(prefilledQuestionVariable)}' not found");
                    }

                    if (!answerOnGpsQuestion.Longitude.HasValue)
                    {
                        throw new Exception($"Field Name: '{GeoPositionExtensions.GetLongitugeColumnName(prefilledQuestionVariable)}' not found");
                    }

                    if (answerOnGpsQuestion.Latitude < -90 || answerOnGpsQuestion.Latitude > 90)
                    {
                        throw new Exception($"Field Name: '{GeoPositionExtensions.GetLatitideColumnName(prefilledQuestionVariable)}', Field Value: '{answerOnGpsQuestion.Latitude}'");
                    }

                    if (answerOnGpsQuestion.Longitude < -180 || answerOnGpsQuestion.Longitude > 180)
                    {
                        throw new Exception($"Field Name: '{GeoPositionExtensions.GetLongitugeColumnName(prefilledQuestionVariable)}', Field Value: '{answerOnGpsQuestion.Longitude}'");
                    }

                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId,
                        new Main.Core.Entities.SubEntities.GeoPosition()
                        {
                            Latitude = answerOnGpsQuestion.Latitude.GetValueOrDefault(),
                            Longitude = answerOnGpsQuestion.Longitude.GetValueOrDefault(),
                            Altitude = answerOnGpsQuestion.Altitude.GetValueOrDefault(),
                            Accuracy = answerOnGpsQuestion.Accuracy.GetValueOrDefault(),
                            Timestamp = answerOnGpsQuestion.Timestamp.GetValueOrDefault()
                        });
                }
                else
                {
                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId,
                        importedInterview[prefilledQuestionVariable]);
                }
            }

            return answersOnPrefilledQuestions;
        }

        private Type GetTypeOfAnswer(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.DateTime:
                    return typeof (DateTime);
                case QuestionType.Numeric:
                    return ((INumericQuestion) question).IsInteger ? typeof (int) : typeof (decimal);
                case QuestionType.SingleOption:
                    return typeof (decimal);
                case QuestionType.GpsCoordinates:
                    return typeof(GeoPosition);
                default:
                    return typeof (string);
            }
        }

        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}