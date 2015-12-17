using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewImportService : IInterviewImportService
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository;
        private readonly ICommandService commandService;
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserViewFactory userViewFactory;
        private readonly IArchiveUtils archiver;
        private readonly SampleImportSettings sampleImportSettings;

        private readonly ConcurrentDictionary<string, Guid> supervisorCache = new ConcurrentDictionary<string, Guid>();

        public InterviewImportService(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentRepository,
            ICommandService commandService,
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            IArchiveUtils archiver,
            SampleImportSettings sampleImportSettings)
        {
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.commandService = commandService;
            this.globalInfoProvider = globalInfoProvider;
            this.userViewFactory = userViewFactory;
            this.archiver = archiver;
            this.sampleImportSettings = sampleImportSettings;
        }

        public void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, Stream zipOrCsvFileStream)
        {
            if (this.Status.IsInProgress)
                throw new Exception("Import interviews is in progress. Wait until current operation is finished.");

            if (this.archiver.IsZipStream(zipOrCsvFileStream))
            {
                var unzippedFiles = this.archiver.UnzipStream(zipOrCsvFileStream).ToList();

                zipOrCsvFileStream = unzippedFiles.FirstOrDefault()?.FileStream;
                if (zipOrCsvFileStream == null)
                    throw new Exception("Zip file does not contains file with interviews.");
            }

            this.Status.IsInProgress = true;
            this.Status.StartedDateTime = DateTime.Now;

            var questionnaireDocument = this.questionnaireDocumentRepository.AsVersioned()
                .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version).Questionnaire;

            this.Status.QuestionnaireTitle = $"(ver. {questionnaireIdentity.Version}) {questionnaireDocument.Title}";

            var prefilledQuestions = questionnaireDocument.Find<IQuestion>(x => x.Featured).ToList();

            var createInterviewCommands = this.ReadInterviewsFromCsv(questionnaireIdentity, zipOrCsvFileStream, prefilledQuestions);
            this.Status.TotalInterviewsCount = createInterviewCommands.Count;

            Task.Run(() =>
            {
                try
                {
                    Parallel.ForEach(createInterviewCommands,
                        new ParallelOptions {MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit},
                        (createInterviewCommand) =>
                        {

                            this.commandService.Execute(createInterviewCommand);
                            this.Status.CreatedInterviewsCount += 1;

                            this.Status.ElapsedTime =
                                DateTime.Now.Subtract(this.Status.StartedDateTime).TotalMilliseconds;
                            this.Status.TimePerInterview = this.Status.ElapsedTime/this.Status.CreatedInterviewsCount;
                            this.Status.EstimatedTime = this.Status.TimePerInterview*this.Status.TotalInterviewsCount;
                        });
                }
                finally
                {
                    this.Status.IsInProgress = false;
                }
            });
        }

        private List<CreateInterviewWithPreloadedData> ReadInterviewsFromCsv(QuestionnaireIdentity questionnaireIdentity, Stream csvFileStream,
            List<IQuestion> prefilledQuestions)
        {
            var csvRow = this.GetInterviewCsvRowProperties(prefilledQuestions);

            var dynamicTypeOfAnswersOnPrefilledQuestions = csvRow.ToDynamicType();
            var headquartersId = this.globalInfoProvider.GetCurrentUser().Id;

            List<CreateInterviewWithPreloadedData> interviewViews;
            using (var csvReader = new CsvReader(new StreamReader(csvFileStream)))
            {
                csvReader.Configuration.AutoMap(dynamicTypeOfAnswersOnPrefilledQuestions);
                csvReader.Configuration.Delimiter = "\t";
                csvReader.Configuration.IgnoreReadingExceptions = true;
                csvReader.Configuration.ReadingExceptionCallback = (exception, reader) => { };

                interviewViews = csvReader.GetRecords(dynamicTypeOfAnswersOnPrefilledQuestions)
                    .Select((dynamic importedInterview) => new CreateInterviewWithPreloadedData(
                        interviewId: Guid.NewGuid(),
                        userId: headquartersId,
                        questionnaireId: questionnaireIdentity.QuestionnaireId,
                        version: questionnaireIdentity.Version,
                        supervisorId: GetSupervisorIdByName(importedInterview.supervisor),
                        answersTime: DateTime.UtcNow,
                        preloadedDataDto: new PreloadedDataDto(new []
                        {
                            new PreloadedLevelDto(RosterVector.Empty, this.GetAnswersOnPrefilledQuestions(prefilledQuestions, TypeExtensions.ToDictionary(importedInterview)))
                        })))
                    .ToList();
            }
            return interviewViews;
        }

        private Dictionary<string, Type> GetInterviewCsvRowProperties(List<IQuestion> prefilledQuestions)
        {
            var csvRow = new Dictionary<string, Type>
            {
                ["supervisor"] = typeof (string)
            };
            foreach (var prefilledQuestion in prefilledQuestions)
            {
                if (prefilledQuestion.QuestionType == QuestionType.GpsCoordinates)
                {
                    csvRow[prefilledQuestion.StataExportCaption + "_Latitude"] = typeof (double);
                    csvRow[prefilledQuestion.StataExportCaption + "_Longitude"] = typeof (double);
                    csvRow[prefilledQuestion.StataExportCaption + "_Accuracy"] = typeof (double);
                    csvRow[prefilledQuestion.StataExportCaption + "_Altitude"] = typeof (double);
                    csvRow[prefilledQuestion.StataExportCaption + "_Timestamp"] = typeof (DateTimeOffset);
                }
                else
                {
                    csvRow[prefilledQuestion.StataExportCaption] = this.GetTypeOfAnswer(prefilledQuestion);
                }
            }
            return csvRow;
        }

        private Guid GetSupervisorIdByName(string supervisorName)
        {
            if (!this.supervisorCache.Keys.Contains(supervisorName))
            {
                var supervisor = this.userViewFactory.Load(new UserViewInputModel(supervisorName, null));
                if (supervisor != null)
                    return this.supervisorCache.GetOrAdd(supervisorName, supervisor.PublicKey);
            }

            return this.supervisorCache[supervisorName];
        }

        private Dictionary<Guid, object> GetAnswersOnPrefilledQuestions(List<IQuestion> prefilledQuestions, IDictionary<string, object> interviewView)
        {
            var answersOnPrefilledQuestions = new Dictionary<Guid, object>();
            foreach (var prefilledQuestion in prefilledQuestions)
            {
                if (interviewView.ContainsKey(prefilledQuestion.StataExportCaption))
                {
                    answersOnPrefilledQuestions.Add(prefilledQuestion.PublicKey, interviewView[prefilledQuestion.StataExportCaption]);
                }
                else
                {
                    switch (prefilledQuestion.QuestionType)
                    {
                        case QuestionType.GpsCoordinates:

                            var _Latitude = prefilledQuestion.StataExportCaption + "_Latitude";
                            var _Longitude = prefilledQuestion.StataExportCaption + "_Longitude";
                            var _Accuracy = prefilledQuestion.StataExportCaption + "_Accuracy";
                            var _Altitude = prefilledQuestion.StataExportCaption + "_Altitude";
                            var _Timestamp = prefilledQuestion.StataExportCaption + "_Timestamp";

                            var Latitude = interviewView.Keys.Contains(_Latitude) ? interviewView[_Latitude] : null;
                            var Longitude = interviewView.Keys.Contains(_Longitude) ? interviewView[_Longitude] : null;
                            var Accuracy = interviewView.Keys.Contains(_Accuracy) ? interviewView[_Accuracy] : null;
                            var Altitude = interviewView.Keys.Contains(_Altitude) ? interviewView[_Altitude] : null;
                            var Timestamp = interviewView.Keys.Contains(_Timestamp) ? interviewView[_Timestamp] : null;


                            if (Latitude != null || Longitude != null || Accuracy != null || Altitude != null || Timestamp != null)
                            {
                                answersOnPrefilledQuestions.Add(prefilledQuestion.PublicKey,
                                    new GeoPosition((double)Latitude, (double)Longitude, (double)Accuracy,
                                        (double)Altitude, (DateTimeOffset)Timestamp));
                            }

                            break;
                    }
                }
            }

            return answersOnPrefilledQuestions;
        }

        private Type GetTypeOfAnswer(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.DateTime:
                    return typeof(DateTime);
                case QuestionType.Numeric:
                    return ((INumericQuestion)question).IsInteger ? typeof(int) : typeof(decimal);
                case QuestionType.SingleOption:
                    return typeof(int);
                default:
                    return typeof(string);
            }
        }

        public void ImportInterviewsWithRosters(QuestionnaireIdentity questionnaireIdentity, Stream zipFileStream)
        {
            throw new NotImplementedException();
        }

        public InterviewImportStatus Status => this.status;
        public readonly InterviewImportStatus status = new InterviewImportStatus();
    }
}