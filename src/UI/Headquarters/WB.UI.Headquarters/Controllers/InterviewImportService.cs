using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewImportService : IInterviewImportService
    {
        const string GPSLATITUDECOLUMNPOSTFIX = "_latitude";
        const string GPSLONGITUDECOLUMNPOSTFIX = "_longitude";
        const string GPSACCURACYCOLUMNPOSTFIX = "_accuracy";
        const string GPSALTITUDECOLUMNPOSTFIX = "_altitude";
        const string GPSTIMESTAMPCOLUMNPOSTFIX = "_timestamp";
        const string SUPERVISORCOLUMNNAME = "responsible";
        const string INTERVIEWERCOLUMNNAME = "interviewer";

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
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserViewFactory userViewFactory;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly ILogger logger;
        private readonly SampleImportSettings sampleImportSettings;

        private readonly ConcurrentDictionary<string, Guid?> usersCache = new ConcurrentDictionary<string, Guid?>();

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            SampleImportSettings sampleImportSettings)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.globalInfoProvider = globalInfoProvider;
            this.userViewFactory = userViewFactory;
            this.transactionManager = transactionManager;
            this.logger = logger;
            this.sampleImportSettings = sampleImportSettings;
        }

        public void ImportInterviews(InterviewImportFileDescription fileDescription, Guid? supervisorId)
        {
            var columnsWithTypes = GetFileColumnsWithTypes(fileDescription);
            var dynamicTypeOfImportedInterview = columnsWithTypes.ToDynamicType("interview");

            this.Status.QuestionnaireTitle = fileDescription.QuestionnaireTitle;
            this.Status.StartedDateTime = DateTime.Now;
            this.Status.CreatedInterviewsCount = 0;
            this.Status.ElapsedTime = 0;
            this.Status.EstimatedTime = 0;
            this.Status.State.Columns = fileDescription.FileColumns;
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
                                responsibleId: fileDescription.HeadquartersId,
                                questionnaireIdentity: fileDescription.QuestionnaireIdentity,
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
                csvReader.Configuration.AutoMap(dynamicTypeOfImportedInterview);
                csvReader.Configuration.Delimiter = this.Status.State.Delimiter;
                csvReader.Configuration.BufferSize = 32768;
                csvReader.Configuration.IgnoreHeaderWhiteSpace = true;
                csvReader.Configuration.IsHeaderCaseSensitive = false;
                csvReader.Configuration.SkipEmptyRecords = true;
                csvReader.Configuration.TrimFields = true;
                csvReader.Configuration.TrimHeaders = true;
                csvReader.Configuration.WillThrowOnMissingField = false;
                csvReader.Configuration.ThrowOnBadData = true;

                while (csvReader.Read())
                {
                    try
                    {
                        dynamic dynamicImportedInterview = csvReader.GetRecord(dynamicTypeOfImportedInterview);

                        Guid? supervisorId = null;
                        if (fileDescription.HasSupervisorColumn)
                        {
                            supervisorId = GetUserIdByName(dynamicImportedInterview.responsible);
                            if (!supervisorId.HasValue)
                                throw new Exception($"supervisor '{dynamicImportedInterview.responsible}' not found ");
                        }

                        Guid? interviewerId = null;
                        if (fileDescription.HasInterviewerColumn &&
                            !string.IsNullOrEmpty(dynamicImportedInterview.interviewer))
                        {
                            interviewerId = GetUserIdByName(dynamicImportedInterview.interviewer);
                            if (!interviewerId.HasValue)
                                throw new Exception($"interviewer '{dynamicImportedInterview.interviewer}' not found ");
                        }

                        var answersOnPrefilledQuestions = this.GetAnswersOnPrefilledQuestionsOrThrow(fileDescription.PrefilledQuestions,
                                TypeExtensions.ToDictionary(dynamicImportedInterview));

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
                            ErrorMessage = ex.Data.Contains("CsvHelper") ? ToUserFriendlyErrorMessage((string)ex.Data["CsvHelper"]) : ex.Message
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
            var columnsWithTypes = new Dictionary<string, Type>(fileDescription.ColumnsByPrefilledQuestions
                .Where(column => column.ExistsInFIle)
                .ToDictionary(column => column.ColumnName, column => column.ColumnType));

            if (fileDescription.HasSupervisorColumn)
            {
                columnsWithTypes.Add(SUPERVISORCOLUMNNAME, typeof (string));
            }
            if (fileDescription.HasInterviewerColumn)
            {
                columnsWithTypes.Add(INTERVIEWERCOLUMNNAME, typeof (string));
            }
            return columnsWithTypes;
        }

        public InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes)
        {
            var questionnaireDocument = this.questionnaireDocumentRepository.AsVersioned()
                .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version).Questionnaire;

            var interviewImportFileDescription = new InterviewImportFileDescription()
            {
                ColumnsByPrefilledQuestions = new List<InterviewImportColumn>(),
                QuestionnaireIdentity = questionnaireIdentity,
                QuestionnaireTitle = $"(ver. {questionnaireIdentity.Version}) {questionnaireDocument.Title}",
                FileBytes = fileBytes,
                HeadquartersId = this.globalInfoProvider.GetCurrentUser().Id,
                PrefilledQuestions = questionnaireDocument.Find<IQuestion>(x => x.Featured)
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
                interviewImportFileDescription.HasSupervisorColumn = columns.Contains(SUPERVISORCOLUMNNAME);
                interviewImportFileDescription.HasInterviewerColumn = columns.Contains(INTERVIEWERCOLUMNNAME);

                foreach (var prefilledQuestion in interviewImportFileDescription.PrefilledQuestions)
                {
                    if (prefilledQuestion.IsGps)
                    {
                        interviewImportFileDescription.ColumnsByPrefilledQuestions.AddRange(new[]
                        {
                            ToInterviewImportColumn($"{prefilledQuestion.Variable}{GPSLATITUDECOLUMNPOSTFIX}", columns, true, typeof (double)),
                            ToInterviewImportColumn($"{prefilledQuestion.Variable}{GPSLONGITUDECOLUMNPOSTFIX}", columns, true, typeof (double)),
                            ToInterviewImportColumn($"{prefilledQuestion.Variable}{GPSACCURACYCOLUMNPOSTFIX}", columns, false, typeof (double?)),
                            ToInterviewImportColumn($"{prefilledQuestion.Variable}{GPSALTITUDECOLUMNPOSTFIX}", columns, false, typeof (double)),
                            ToInterviewImportColumn($"{prefilledQuestion.Variable}{GPSTIMESTAMPCOLUMNPOSTFIX}", columns, false, typeof (DateTimeOffset?))
                        });
                    }
                    else
                    {
                        interviewImportFileDescription.ColumnsByPrefilledQuestions.Add(
                            ToInterviewImportColumn(prefilledQuestion.Variable, columns, true, prefilledQuestion.AnswerType));
                    }
                }
            }
            
            return interviewImportFileDescription;
        }

        private InterviewImportPrefilledQuestion ToInterviewImportPrefilledQuestion(IQuestion question,
            QuestionnaireDocument questionnaireDocument)
        {
            return new InterviewImportPrefilledQuestion
            {
                QuestionId = question.PublicKey,
                Variable = question.StataExportCaption.ToLower(),
                AnswerType = this.GetTypeOfAnswer(question),
                IsGps = question.QuestionType == QuestionType.GpsCoordinates,
                IsRosterSize = question.QuestionType == QuestionType.Numeric &&
                               questionnaireDocument.Find<IGroup>(
                                   group => group.RosterSizeQuestionId == question.PublicKey).Any()
            };
        }

        private static InterviewImportColumn ToInterviewImportColumn(string columnName, string[] fileColumns, bool isRequired, Type columnType)
        {
            return new InterviewImportColumn
            {
                ColumnName = columnName,
                ColumnType = columnType,
                IsRequired = isRequired,
                ExistsInFIle = fileColumns.Contains(columnName)
            };
        }

        private Guid? GetUserIdByName(string userName)
        {
            userName = userName.ToLower();
            if (!this.usersCache.Keys.Contains(userName))
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
                try
                {
                    var user = this.userViewFactory.Load(new UserViewInputModel(userName, null));
                    return this.usersCache.GetOrAdd(userName, user?.PublicKey);
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
                if (prefilledQuestion.IsGps)
                {
                    object oLatitude;
                    object oLongtitude;
                    object oAccuracy;
                    object oAltitude;
                    object oTimestamp;

                    importedInterview.TryGetValue($"{prefilledQuestion.Variable}{GPSLATITUDECOLUMNPOSTFIX}", out oLatitude);
                    importedInterview.TryGetValue($"{prefilledQuestion.Variable}{GPSLONGITUDECOLUMNPOSTFIX}", out oLongtitude);
                    importedInterview.TryGetValue($"{prefilledQuestion.Variable}{GPSACCURACYCOLUMNPOSTFIX}", out oAccuracy);
                    importedInterview.TryGetValue($"{prefilledQuestion.Variable}{GPSALTITUDECOLUMNPOSTFIX}", out oAltitude);
                    importedInterview.TryGetValue($"{prefilledQuestion.Variable}{GPSTIMESTAMPCOLUMNPOSTFIX}", out oTimestamp);

                    double? latitude = (double?) oLatitude;
                    double? longtitude = (double?) oLongtitude;

                    if (latitude.HasValue && (latitude.Value < -91 || latitude.Value > 90))
                    {
                        throw new Exception($"Field Name: '{prefilledQuestion.Variable}{GPSLATITUDECOLUMNPOSTFIX}', Field Value: '{latitude}'");
                    }

                    if (longtitude.HasValue && (longtitude.Value < -181 || longtitude.Value > 180))
                    {
                        throw new Exception($"Field Name: '{prefilledQuestion.Variable}{GPSLONGITUDECOLUMNPOSTFIX}', Field Value: '{longtitude}'");
                    }

                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId, new GeoPosition()
                    {
                        Latitude = latitude.GetValueOrDefault(),
                        Longitude = longtitude.GetValueOrDefault(),
                        Accuracy = ((double?)oAccuracy).GetValueOrDefault(),
                        Altitude = ((double?)oAltitude).GetValueOrDefault(),
                        Timestamp = ((DateTimeOffset?)oTimestamp).GetValueOrDefault()
                    });
                }
                else if (prefilledQuestion.IsRosterSize)
                {
                    var answerOnRosterSizeQuestion = (int)importedInterview[prefilledQuestion.Variable];
                    if (answerOnRosterSizeQuestion < 0 || answerOnRosterSizeQuestion > 40)
                    {
                        throw new Exception($"Field Name: '{prefilledQuestion.Variable}', Field Value: '{answerOnRosterSizeQuestion}'");
                    }

                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId, answerOnRosterSizeQuestion);
                }
                else
                {
                    answersOnPrefilledQuestions.Add(prefilledQuestion.QuestionId,
                        importedInterview[prefilledQuestion.Variable]);
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
                default:
                    return typeof (string);
            }
        }

        public InterviewImportStatus Status { get; } = new InterviewImportStatus()
        {
            State = new InterviewImportState
            {
                Errors = new List<InterviewImportError>()
            }
        };
    }
}