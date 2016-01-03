using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
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

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewImportService : IInterviewImportService
    {
        const string RESPONSIBLECOLUMNNAME = "responsible";

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

        private readonly ConcurrentDictionary<string, UserView> usersCache = new ConcurrentDictionary<string, UserView>();

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            IUserViewFactory userViewFactory,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            IViewFactory<SampleUploadViewInputModel, SampleUploadView> sampleUploadViewFactory,
            SampleImportSettings sampleImportSettings)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.userViewFactory = userViewFactory;
            this.transactionManager = transactionManager;
            this.logger = logger;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
            this.sampleImportSettings = sampleImportSettings;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes, Guid? supervisorId, Guid headquartersId)
        {

            InterviewImportFileDescription fileDescription;
            
            try
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                fileDescription = this.GetDescriptionByFileWithInterviews(questionnaireIdentity, fileBytes);
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
                            this.logger.Error("Import interview.", ex);
                        }

                        this.Status.CreatedInterviewsCount += 1;
                        this.Status.ElapsedTime = DateTime.Now.Subtract(this.Status.StartedDateTime).TotalMilliseconds;
                        this.Status.TimePerInterview = this.Status.ElapsedTime/this.Status.CreatedInterviewsCount;
                        this.Status.EstimatedTime = this.Status.TimePerInterview*this.Status.TotalInterviewsCount;
                    });
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

            using (var csvReader = new CsvReader(new StreamReader(new MemoryStream(fileDescription.FileBytes))))
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

                var interviewMap = csvReader.Configuration.AutoMap(dynamicTypeOfImportedInterview);
                foreach (var prefilledGpsQuestion in fileDescription.PrefilledQuestions.Where(x => x.IsGps))
                {
                    var geoPositionReferenceMap = new CsvPropertyReferenceMap(
                        dynamicTypeOfImportedInterview.GetProperty(prefilledGpsQuestion.Variable),
                        csvReader.Configuration.AutoMap<GeoPosition>());

                    geoPositionReferenceMap.Prefix($"{prefilledGpsQuestion.Variable}{QuestionDataParser.COLUMNDELIMITER}");
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
                            var responsible = GetResponsibleByName(responsibleName);
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

        public InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes)
        {
            var questionnaireDocument = this.questionnaireDocumentRepository.AsVersioned()
                .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version).Questionnaire;

            var prefilledQuestions = questionnaireDocument.Find<IQuestion>(x => x.Featured);

            var interviewImportFileDescription = new InterviewImportFileDescription()
            {
                ColumnsByPrefilledQuestions = new List<InterviewImportColumn>(),
                QuestionnaireTitle = $"(ver. {questionnaireIdentity.Version}) {questionnaireDocument.Title}",
                FileBytes = fileBytes,
                PrefilledQuestions = prefilledQuestions
                        .Select(question => this.ToInterviewImportPrefilledQuestion(question, questionnaireDocument))
                        .ToList(),
            };
            
            using (var csvReader = new CsvReader(new StreamReader(new MemoryStream(fileBytes))))
            {
                csvReader.Configuration.Delimiter = this.Status.State.Delimiter;
                csvReader.Configuration.TrimHeaders = true;
                csvReader.Configuration.IsHeaderCaseSensitive = false;
                csvReader.Read();

                var columns = interviewImportFileDescription.FileColumns = csvReader.FieldHeaders.Select(header => header.Trim().ToLower()).ToArray();
                interviewImportFileDescription.HasResponsibleColumn = columns.Contains(RESPONSIBLECOLUMNNAME);

                foreach (var exportColumnByPrefilledQuestion in this.sampleUploadViewFactory.Load(
                    new SampleUploadViewInputModel(questionnaireIdentity.QuestionnaireId,
                        questionnaireIdentity.Version)).ColumnListToPreload)
                {
                    var prefilledQuestion = prefilledQuestions.FirstOrDefault(question => question.PublicKey == exportColumnByPrefilledQuestion.Id);
                    interviewImportFileDescription.ColumnsByPrefilledQuestions.Add(
                        new InterviewImportColumn
                        {
                            ColumnName = exportColumnByPrefilledQuestion.Caption,
                            IsRequired = (prefilledQuestion != null && prefilledQuestion.QuestionType != QuestionType.GpsCoordinates) ||
                                exportColumnByPrefilledQuestion.Caption.IsRequiredGeoPositionColumn(),
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
                IsGps = prefilledQuestion.QuestionType == QuestionType.GpsCoordinates,
                IsRosterSize = prefilledQuestion.QuestionType == QuestionType.Numeric &&
                               questionnaireDocument.Find<IGroup>(
                                   group => group.RosterSizeQuestionId == prefilledQuestion.PublicKey).Any()
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
                        throw new Exception($"Field Name: '{prefilledQuestionVariable.GetLatitideColumnName()}' not found");
                    }

                    if (!answerOnGpsQuestion.Longitude.HasValue)
                    {
                        throw new Exception($"Field Name: '{prefilledQuestionVariable.GetLongitugeColumnName()}' not found");
                    }

                    if (answerOnGpsQuestion.Latitude < -90 || answerOnGpsQuestion.Latitude > 90)
                    {
                        throw new Exception($"Field Name: '{prefilledQuestionVariable.GetLatitideColumnName()}', Field Value: '{answerOnGpsQuestion.Latitude}'");
                    }

                    if (answerOnGpsQuestion.Longitude < -180 || answerOnGpsQuestion.Longitude > 180)
                    {
                        throw new Exception($"Field Name: '{prefilledQuestionVariable.GetLongitugeColumnName()}', Field Value: '{answerOnGpsQuestion.Longitude}'");
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
                else if (prefilledQuestion.IsRosterSize)
                {
                    var answerOnRosterSizeQuestion = (int)importedInterview[prefilledQuestionVariable];
                    if (answerOnRosterSizeQuestion < 0 || answerOnRosterSizeQuestion > 40)
                    {
                        throw new Exception($"Field Name: '{prefilledQuestionVariable}', Field Value: '{answerOnRosterSizeQuestion}'");
                    }

                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId, answerOnRosterSizeQuestion);
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
                    return typeof (int);
                case QuestionType.GpsCoordinates:
                    return typeof(GeoPosition);
                default:
                    return typeof (string);
            }
        }

        public InterviewImportStatus Status { get; private set; } = new InterviewImportStatus();
    }
}