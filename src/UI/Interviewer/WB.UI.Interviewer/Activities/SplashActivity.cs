using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Views;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using PCLStorage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.SharedPreferences;
using WB.UI.Interviewer.ViewModel.Dashboard;
using WB.UI.Interviewer.ViewModel.Login;
using Environment = System.Environment;

#warning we must keep "Sqo" namespace on siaqodb as long as at least one 5.1.0-5.3.* version exist
#warning do not remove "Sqo" namespace
#warning if after all the warning you intend to remove the namespace anyway, please remove NuGet packages SiaqoDB and SiaqoDbProtable also
using Sqo;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.XamarinAndroid;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var splashAnimation = this.FindViewById<ImageView>(Resource.Id.splash_animation);
            ((AnimationDrawable)splashAnimation.Drawable).Start();
        }

        protected override async void TriggerFirstNavigate()
        {
            await this.BackwardCompatibilityAsync();

            await Mvx.Resolve<IViewModelNavigationService>().NavigateToAsync<LoginViewModel>();
        }

        private async Task BackwardCompatibilityAsync()
        {
            var settings = Mvx.Resolve<IAsyncPlainStorage<ApplicationSettingsView>>();
            if (settings.FirstOrDefault() != null) return;

            await RestoreApplicationSettingsAsync();
            await RestoreInterviewerAsync();
            await Task.Run(this.RestoreInterviewsAsync);
            await Task.Run(this.RestoreQuestionnairesAsync);
            await Task.Run(this.RestoreQuestionnaireModelsAndDocumentsAsync);
            await Task.Run(this.RestoreEventStreamsAsync);
            await Task.Run(this.RestoreInterviewDetailsAsync);
            await Task.Run(this.RestoreInterviewImagesAsync);
        }

        private async Task RestoreInterviewImagesAsync()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var fileSystemAccessor = Mvx.Resolve<IFileSystemAccessor>();
            IPlainInterviewFileStorage oldImageFileStorage = new PlainInterviewFileStorage(fileSystemAccessor, basePath);
            IPlainInterviewFileStorage newImageFileStorage = Mvx.Resolve<IPlainInterviewFileStorage>();

            var interviews = this.GetSqlLiteEntities<QuestionnaireDTO>("Projections");
            foreach (var interview in interviews)
            {
                var binaryDataDescriptors = oldImageFileStorage.GetBinaryFilesForInterview(Guid.Parse(interview.Id));
                foreach (var descriptor in binaryDataDescriptors)
                {
                    await newImageFileStorage.StoreInterviewBinaryDataAsync(descriptor.InterviewId, descriptor.FileName,
                        descriptor.GetData());
                }
            }
        }

        private async Task RestoreInterviewDetailsAsync()
        {
            var commandService = Mvx.Resolve<ICommandService>();
            var serializer = Mvx.Resolve<ISerializer>();

            var interviewersRepository = Mvx.Resolve<IAsyncPlainStorage<InterviewerIdentity>>();

            var pathToInterviewDetails =
                PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SyncCache");

            var interviewDetailsFolder = await FileSystem.Current.GetFolderFromPathAsync(pathToInterviewDetails);

            if (interviewDetailsFolder == null) return;

            var interviewer = interviewersRepository.FirstOrDefault();

            if (interviewer == null) return;

            foreach (var interviewDetailsFile in await interviewDetailsFolder.GetFilesAsync())
            {
                var interviewDetailsText = await interviewDetailsFile.ReadAllTextAsync();
                var interviewSynchronizationDto =
                    serializer.Deserialize<InterviewSynchronizationDto>(interviewDetailsText);

                await commandService.ExecuteAsync(new SynchronizeInterviewCommand(
                    interviewId: Guid.Parse(interviewDetailsFile.Name),
                    userId: interviewer.UserId,
                    sycnhronizedInterview: interviewSynchronizationDto));
            }
        }

        private async Task RestoreInterviewerAsync()
        {
            var interviewersRepository = Mvx.Resolve<IAsyncPlainStorage<InterviewerIdentity>>();

            var oldInterviewer = await GetInterviewerIdentityFromOldDatabase();
            if (oldInterviewer != null)
            {
                await interviewersRepository.StoreAsync(oldInterviewer);
            }
            else
            {
                var veryOldInterviewersRepository = this.GetSqlLiteEntities<LoginDTO>("Projections");

                var firstUserFromOldStorage = veryOldInterviewersRepository?.FirstOrDefault();

                if (firstUserFromOldStorage != null)
                {
                    await interviewersRepository.StoreAsync(new InterviewerIdentity()
                    {
                        Id = firstUserFromOldStorage.Id,
                        UserId = Guid.Parse(firstUserFromOldStorage.Id),
                        Password = firstUserFromOldStorage.Password,
                        Name = firstUserFromOldStorage.Login,
                        SupervisorId = Guid.Parse(firstUserFromOldStorage.Supervisor)
                    });
                }
            }
        }

        private static async Task<InterviewerIdentity> GetInterviewerIdentityFromOldDatabase()
        {
            InterviewerIdentity oldInterviewer;

#warning we must keep this code as long as at least one 5.1.0-5.3.* version exist
#warning do not remove this code
            SiaqodbConfigurator.EncryptedDatabase = false;

            using (var oldInterviewersRepository = new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")))
            {
                oldInterviewer = await oldInterviewersRepository.Query<InterviewerIdentity>().FirstOrDefaultAsync();
            }

            return oldInterviewer;
        }

        private async Task RestoreInterviewsAsync()
        {
            var serializer = Mvx.Resolve<ISerializer>();
            var interviewViewRepository = Mvx.Resolve<IAsyncPlainStorage<InterviewView>>();

            var interviews = this.GetSqlLiteEntities<QuestionnaireDTO>("Projections");

            await interviewViewRepository.StoreAsync(interviews.Select(x => new InterviewView
            {
                Id = x.Id,
                InterviewId = Guid.Parse(x.Id),
                ResponsibleId = Guid.Parse(x.Responsible),
                InterviewerAssignedDateTime = x.InterviewerAssignedDateTime ?? x.CreatedDateTime,
                CompletedDateTime = x.CompletedDateTime,
                StartedDateTime = x.StartedDateTime,
                RejectedDateTime = x.RejectedDateTime,
                Census = x.CreatedOnClient ?? false,
                QuestionnaireId = new QuestionnaireIdentity(Guid.Parse(x.Survey), x.SurveyVersion).ToString(),
                LastInterviewerOrSupervisorComment = x.Comments,
                Status = (InterviewStatus)x.Status,
                AnswersOnPrefilledQuestions = serializer.Deserialize<FeaturedItem[]>(x.Properties).Select(y => new InterviewAnswerOnPrefilledQuestionView
                {
                    QuestionId = y.PublicKey,
                    QuestionText = y.Title,
                    Answer = y.Value
                }).ToArray(),
                GpsLocation = new InterviewGpsLocationView
                {
                    PrefilledQuestionId = string.IsNullOrEmpty(x.GpsLocationQuestionId) ? (Guid?)null : Guid.Parse(x.GpsLocationQuestionId),
                    Coordinates = x.GpsLocationLatitude.HasValue && x.GpsLocationLongitude.HasValue ? new InterviewGpsCoordinatesView
                    {
                        Latitude = x.GpsLocationLatitude.Value,
                        Longitude = x.GpsLocationLongitude.Value
                    } : null
                },
                CanBeDeleted = x.JustInitilized ?? false
            }));
        }

        private async Task RestoreEventStreamsAsync()
        {
            var pathToEventStreams = PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "EventStore");
            var eventViewRepository = Mvx.Resolve<IInterviewerEventStorage>();

            var eventStreamsFolder = await FileSystem.Current.GetFolderFromPathAsync(pathToEventStreams);

            if (eventStreamsFolder == null) return;

            foreach (var eventStreamFile in await eventStreamsFolder.GetFilesAsync())
            {
                IEnumerable<StoredEvent> eventStream = this.GetSqlLiteEntities<StoredEvent>(eventStreamFile.Name, "EventStore");

                var eventViews = eventStream.OrderBy(x => x.Sequence).Select((x, index) => new EventView
                {
                    EventSourceId = Guid.Parse(eventStreamFile.Name),
                    DateTimeUtc = new DateTime(x.TimeStamp),
                    EventSequence = index + 1,
                    JsonEvent = x.Data,
                    EventId = Guid.Parse(x.EventId)
                });
                eventViewRepository.MigrateOldEvents(eventViews);
            }
        }

        private async Task RestoreQuestionnairesAsync()
        {
            var questionnaireViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireView>>();

            var questionnaires = this.GetSqlLiteEntities<SurveyDto>("Projections");

            await questionnaireViewRepository.StoreAsync(questionnaires.Select(x => new QuestionnaireView
            {
                Id = new QuestionnaireIdentity(Guid.Parse(x.QuestionnaireId), x.QuestionnaireVersion).ToString(),
                Identity = new QuestionnaireIdentity(Guid.Parse(x.QuestionnaireId), x.QuestionnaireVersion),
                Title = x.SurveyTitle,
                Census = x.AllowCensusMode
            }));
        }

        private async Task RestoreQuestionnaireModelsAndDocumentsAsync()
        {
            var serializer = Mvx.Resolve<ISerializer>();
            var questionnaireModelBuilder = Mvx.Resolve<IQuestionnaireModelBuilder>();

            var questionnaires = this.GetSqlLiteEntities<PlainStorageRow>("PlainStore");

            var questionnaireDocumentsAndModels = questionnaires.Select(
                x =>
                    new
                    {
                        QuestionnaireEntityTypeName = x.Id.Split('$')[0],
                        QuestionnaireId = x.Id = x.Id.Split('$')[1] + "$" + x.Id.Split('$')[2],
                        Entity = serializer.Deserialize<object>(x.SerializedData)
                    }).ToList();

            var questionnaireModels =
                questionnaireDocumentsAndModels.Where(x => x.QuestionnaireEntityTypeName == "QuestionnaireModel")
                    .Select(x => new { QuestionnaireId = x.QuestionnaireId, Model = (QuestionnaireModel)x.Entity })
                    .ToList();
            var questionnaireDocuments =
                questionnaireDocumentsAndModels.Where(x => x.QuestionnaireEntityTypeName == "QuestionnaireDocument")
                    .Select(x => new { QuestionnaireId = x.QuestionnaireId, Document = (QuestionnaireDocument)x.Entity })
                    .ToList();

            foreach (var questionnaireDocument in questionnaireDocuments)
            {
                var questionnaireModel = questionnaireModels.FirstOrDefault(x => x.QuestionnaireId == questionnaireDocument.QuestionnaireId)?.Model ??
                    questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocument.Document);

                await this.FixCompleteScreenAndStoreQuestionnaireAsync(questionnaireDocument.QuestionnaireId, questionnaireModel, questionnaireDocument.Document);
            }
        }

        [Obsolete]
        public class PlainStorageRow
        {
            [PrimaryKey]
            public string Id { get; set; }

            public string SerializedData { get; set; }
        }

        public async Task FixCompleteScreenAndStoreQuestionnaireAsync(string questionnaireId, QuestionnaireModel questionnaireModel, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireDocumentViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireDocumentView>>();
            var questionnaireModelViewRepository = Mvx.Resolve<IAsyncPlainStorage<QuestionnaireModelView>>();

            if (questionnaireModel?.GroupsHierarchy != null && !questionnaireModel.GroupsHierarchy.Any())
            {
                var lastGroupInHierarchy = questionnaireModel.GroupsHierarchy.Last();

                if (lastGroupInHierarchy.Title == UIResources.Interview_Complete_Screen_Title &&
                    !lastGroupInHierarchy.Children.Any())
                {
                    questionnaireModel.GroupsHierarchy.Remove(lastGroupInHierarchy);

                    var groupInQuestionnaireDocument =
                    questionnaireDocument.Children.OfType<IGroup>().FirstOrDefault(g => g.PublicKey == lastGroupInHierarchy.Id);

                    if (groupInQuestionnaireDocument != null)
                    {
                        questionnaireDocument.Children.Remove(groupInQuestionnaireDocument);
                    }
                }
            }

            await questionnaireModelViewRepository.StoreAsync(new QuestionnaireModelView
            {
                Id = questionnaireId,
                Model = questionnaireModel
            });

            await questionnaireDocumentViewRepository.StoreAsync(new QuestionnaireDocumentView
            {
                Id = questionnaireId,
                Document = questionnaireDocument
            });
        }


        private static async Task RestoreApplicationSettingsAsync()
        {
            var settings = Mvx.Resolve<IInterviewerSettings>();

            var endpoint = GetAppSettings(SettingsNames.Endpoint, string.Empty);
            var httpResponseTimeoutInSec = GetAppSettings(SettingsNames.HttpResponseTimeout, string.Empty);
            var communicationBufferSize = GetAppSettings(SettingsNames.BufferSize, string.Empty);
            var gpsResponseTimeoutInSec = GetAppSettings(SettingsNames.GpsReceiveTimeoutSec, string.Empty);

            if (!string.IsNullOrEmpty(endpoint))
                await settings.SetEndpointAsync(endpoint);

            if (!string.IsNullOrEmpty(communicationBufferSize))
                await settings.SetCommunicationBufferSize(int.Parse(communicationBufferSize));

            if (!string.IsNullOrEmpty(gpsResponseTimeoutInSec))
                await settings.SetGpsResponseTimeoutAsync(int.Parse(gpsResponseTimeoutInSec));

            if (!string.IsNullOrEmpty(httpResponseTimeoutInSec))
                await settings.SetHttpResponseTimeoutAsync(int.Parse(httpResponseTimeoutInSec));
        }

        private static string GetAppSettings(string settingName, string defaultValue)
        {
            var newPreference = PreferenceManager.GetDefaultSharedPreferences(Application.Context).GetString(settingName, defaultValue);
            return !string.IsNullOrEmpty(newPreference)
                ? newPreference
                : Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private).GetString(settingName, defaultValue);
        }

        private IEnumerable<TView> GetSqlLiteEntities<TView>(string dbName, string subFolder = "") where TView : class, new()
        {
            var databasePath = PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), subFolder, dbName);
            using (var connection = new SQLiteConnection(new SQLitePlatformAndroid(), databasePath))
            {
                connection.CreateTable<TView>();
                return connection.Table<TView>().ToList();
            }
        }
    }
}