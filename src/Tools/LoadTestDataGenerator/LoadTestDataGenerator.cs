using Main.Core.Commands.Questionnaire;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ncqrs.Eventing.Storage.RavenDB.RavenIndexes;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace LoadTestDataGenerator
{
    using System.Threading;

    using Main.Core;
    using Main.Core.View;
    using Main.Core.View.User;
    using System.Threading.Tasks;

    public partial class LoadTestDataGenerator : Form
    {
        private static readonly Random RandomObject = new Random((int)DateTime.Now.Ticks);

        protected readonly ICommandService CommandService;
        protected readonly IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> SurveyStorage;
        protected readonly DocumentStore RavenStore;
        private readonly IViewFactory<UserBrowseInputModel, UserBrowseView> userBrowseViewFactory;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        const string IndexForDelete = "AllEvents";
        const string IndexForStatistics = "EventsStatistics";

        private QuestionnaireDocument template;
        private IEnumerable<IQuestion> featuredQuestions;
        private Statistics statistics;
        private ProgressOperation currentOperation;
        private readonly IRavenReadSideRepositoryWriterRegistry writerRegistry;
        private readonly BatchedRavenDBEventStore eventStore;

        internal LoadTestDataGenerator(ICommandService commandService,
            IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> surveyStorage,
            DocumentStore ravenStore, IViewFactory<UserBrowseInputModel, UserBrowseView> userBrowseViewFactory, IViewFactory<UserViewInputModel, UserView> userViewFactory,
            IRavenReadSideRepositoryWriterRegistry writerRegistry
            , IStreamableEventStore eventStore)
        {
            this.RavenStore = ravenStore;
            this.userBrowseViewFactory = userBrowseViewFactory;
            this.userViewFactory = userViewFactory;
            this.writerRegistry = writerRegistry;
            this.eventStore = (BatchedRavenDBEventStore)eventStore;
            this.CommandService = commandService;
            this.SurveyStorage = surveyStorage;
            InitializeComponent();

            var databaseName = ConfigurationManager.AppSettings["Raven.DefaultDatabase"];
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = "<system>";
            }
            defaultDatabaseName.Text = databaseName;

            this.PrepareDatabase();
            this.UpdateEventsStatistics();
        }

        private void generate_Click(object sender, EventArgs e)
        {
            this.PrepareToStart();

            this.UpdateStatus("read template");
            template = this.ReadTemplate(this.templatePath.Text);

            this.Start();

            Task.Run(() =>
            {
                this.EnableCacheInAllRepositoryWriters();

                if (this.clearDatabase.Checked)
                {
                    this.UpdateStatus("clean events database");
                    this.ClearDatabase();
                }
                if (this.clearViews.Checked)
                {
                    this.UpdateStatus("clean views database");
                    this.ClearAllViews();
                }

                this.GenerateSupervisorsEvents();

                this.eventStore.StoreCache();

                if (statistics.hasCAPIevents)
                {
                    this.UpdateStatus("create CAPI's events");
                    this.GenerateCapiEvents();
                }

                this.eventStore.StoreCache();

                this.DisableCacheInAllRepositoryWriters();

                this.Stop();
            });
        }

        private void EnableCacheInAllRepositoryWriters()
        {
            foreach (IRavenReadSideRepositoryWriter writer in this.writerRegistry.GetAll())
            {
                writer.EnableCache();
            }
        }

        private void DisableCacheInAllRepositoryWriters()
        {
            foreach (IRavenReadSideRepositoryWriter writer in this.writerRegistry.GetAll())
            {
                writer.DisableCache();
            }
        }

        private void PrepareToStart()
        {
            generate.Enabled = false;
            lstLog.Items.Clear();
        }

        private void Start()
        {
            var surveys_count = int.Parse(surveys_amount.Text);

            statistics = new Statistics(
                hasHeadQuarter: chkHeadquarter.Checked,
                hasCAPIevents: chkGenerateCapiEvents.Checked,
                hasFeaturedQuestions: chkSetAnswers.Checked,
                hasSupervisorEvents: chkGenerateSupervisorEvents.Checked)
                             {
                                 SurveysCount = surveys_count,
                                 SupervisorEventsCount = surveys_count * 4
                                 /*assign to supervisor+unussigned status+assign to interviewer+initial status*/,
                                 InterviewersCount =
                                     int.Parse(interviewersCount.Text),
                                 SupervisorsCount =
                                     int.Parse(supervisorsCount.Text),
                                 FullCAPIEventsCount = surveys_count
                             };
            statistics.ElapsedTimeTick += statistics_ElapsedTimeTick;

            if (statistics.hasFeaturedQuestions)
            {
                featuredQuestions = template.GetFeaturedQuestions();
                if (featuredQuestions != null)
                {
                    statistics.FeaturedQuestionsCount = featuredQuestions.Count() * statistics.SurveysCount;
                }
            }

            ctrlProgress.Maximum = statistics.TotalCount;
        }

        void statistics_ElapsedTimeTick(TimeSpan time)
        {
            txtElapsedTime.GetCurrentParent()
                          .InvokeIfRequired(
                              x =>
                              txtElapsedTime.Text =
                              string.Format("Elapsed time: {0}", time.ToString(@"dd\.hh\:mm\:ss")));
        }
        
        private void Stop()
        {
            generate.InvokeIfRequired(x => x.Enabled = true);

            currentOperation = null;
            statistics.Dispose();

            statusStrip1.InvokeIfRequired(x => { ctrlProgress.Value = 0; });

            this.UpdateStatus("done");
        }
        
        private void UpdateProgress()
        {
            statistics.ProgressTotalIndex += 1;
            statusStrip1.InvokeIfRequired(
                x =>
                    {
                        ctrlProgress.Value = statistics.ProgressTotalIndex;
                        txtStatus.Text = string.Format(
                            "Total:  {0} of {1}", statistics.ProgressTotalIndex, statistics.TotalCount);
                    });

            this.UpdateLastStatement();
        }

        private void UpdateForecast(TimeSpan timeSpan)
        {
            statusStrip1.InvokeIfRequired(x => { timeForecast.Text = string.Format("Estimated time: {0}", timeSpan.ToString(@"hh\:mm\:ss")); });
        }

        internal class ProgressOperation
        {
            public string Text { get; set; }

            public int Value { get; set; }

            public int Count { get; set; }
        }

        internal class Statistics : IDisposable
        {
            public readonly bool hasHeadQuarter;
            public readonly bool hasCAPIevents;
            public readonly bool hasFeaturedQuestions;
            public  readonly bool hasSupervisorEvents;
            private readonly DateTime startTime;
            private Timer timer;

            public delegate void ElapsedTimerTickHandler(TimeSpan time);
            public event ElapsedTimerTickHandler ElapsedTimeTick;

            public Statistics(bool hasHeadQuarter, bool hasCAPIevents, bool hasFeaturedQuestions, bool hasSupervisorEvents)
            {
                this.hasCAPIevents = hasCAPIevents;
                this.hasFeaturedQuestions = hasFeaturedQuestions;
                this.hasHeadQuarter = hasHeadQuarter;
                this.hasSupervisorEvents = hasSupervisorEvents;
                this.TemplatesCount = 1;
                this.HeadquartersCount = 1;
                this.FeaturedQuestionsCount = 0;
                this.ProgressTotalIndex = 0;
                this.startTime = DateTime.Now;
                timer = new Timer(ElapsedTime, null, 0, 1000);
            }

            private void ElapsedTime(object state)
            {
                if (ElapsedTimeTick != null)
                {
                    ElapsedTimeTick(DateTime.Now.Subtract(this.startTime));
                }
            }

            public int SurveysCount { get; set; }
            public int InterviewersCount { get; set; }
            public int SupervisorsCount { get; set; }
            public int FeaturedQuestionsCount { get; set; }
            public int FullCAPIEventsCount { get; set; }
            public int TemplatesCount { get; private set; }
            public int HeadquartersCount { get; private set; }
            public int SupervisorEventsCount { get; set; }

            public int PartialCAPIEventsCount {
                get
                {
                    return this.FullCAPIEventsCount;
                }
            }

            public int TotalCount
            {
                get
                {
                    return this.InterviewersCount + this.SupervisorsCount + this.SurveysCount + this.TemplatesCount
                           + (this.hasSupervisorEvents ? this.SupervisorEventsCount : 0)
                           + (this.hasFeaturedQuestions ? this.FeaturedQuestionsCount : 0)
                           + (this.hasHeadQuarter ? this.HeadquartersCount : 0)
                           + (this.hasCAPIevents ? this.FullCAPIEventsCount + this.PartialCAPIEventsCount : 0);
                }
            }

            public DateTime StartTime
            {
                get
                {
                    return this.startTime;
                }
            }

            public int ProgressTotalIndex { get; set; }


            #region [dispose]

            private bool _disposed = false;

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (timer != null)
                            timer.Dispose();
                    }

                    timer = null;
                    _disposed = true;
                }
            }
            #endregion
        }

        private void UpdateStatus(string status, int count = 0)
        {
            if (currentOperation == null)
            {
                currentOperation = new ProgressOperation() { Count = count, Value = 0, Text = status };
            }
            else
            {
                currentOperation.Value = 0;
                currentOperation.Text = status;
                currentOperation.Count = count;
            }

            lstLog.InvokeIfRequired(
                x =>
                x.Items.Add(
                    currentOperation.Count == 0
                        ? currentOperation.Text
                        : string.Format(
                            "{0} {1} of {2}", currentOperation.Text, currentOperation.Value, currentOperation.Count)));

            UpdateEventsStatistics();
        }

        private void UpdateLastStatement()
        {
            currentOperation.Value += 1;
            lstLog.InvokeIfRequired(
                x =>
                x.Items[x.Items.Count - 1] =
                string.Format("{0} {1} of {2}", currentOperation.Text, currentOperation.Value, currentOperation.Count));
        }
        
        private void UpdateEventsStatistics()
        {
            Task.Run(
                () =>
                    {
                        var statistics = new List<EventStatisticItem>();
                        using (IDocumentSession session = this.RavenStore.OpenSession())
                        {
                            statistics.AddRange(
                                session.Query<EventStatisticItem>(IndexForStatistics)
                                       .Customize(customization => customization.WaitForNonStaleResultsAsOfNow()));
                        }
                        eventsStatistics.InvokeIfRequired(
                            _ =>
                            {
                                _.Items.Clear();
                                statistics.OrderByDescending(x => x.Count)
                                          .ToList()
                                          .ForEach(x => _.Items.Add(string.Format("{0,-7} {1}", x.Count, x.Type)));
                            });
                    });
        }

        private void PrepareDatabase()
        {
            this.RavenStore
                .DatabaseCommands
                .PutIndex(IndexForDelete,
                          new IndexDefinition
                              {
                                  Map =
                                      "from doc in docs let EventId = doc[\"@metadata\"][\"@id\"] select new { EventId };"
                              },
                          overwrite: true);
            this.RavenStore
          .DatabaseCommands
          .PutIndex(IndexForStatistics,
                    new IndexDefinition
                    {
                        Map = "from sEvent in docs.Events select new { Type = sEvent.EventType, Count = 1 };",
                        Reduce = "from result in results group result by result.Type into g select new { Type = g.Key, Count = g.Sum(x => x.Count) }"
                    },
                    overwrite: true);
        }

        private void GenerateCapiEvents()
        {
            if (!LoadTestDataGeneratorRegistry.ShouldUsePersistentReadLayer())
            {
                this.UpdateStatus("reduild read layer");
                NcqrsInit.EnsureReadLayerIsBuilt();
            }

            var surveyIds = this.GetSurveyIds();
            this.UpdateStatus("full capi events", statistics.FullCAPIEventsCount);
            var surveysCount = (int) (surveyIds.Count * 1.1);
            var startTime = DateTime.Now;
            var processedSurveys = 1;
            foreach (var surveyId in surveyIds)
            {
                FillAnswers(surveyId);
                var responsible = this.SurveyStorage.GetById(surveyId).Responsible;
                CompleteSurvey(surveyId, SurveyStatus.Complete, responsible);
                this.UpdateProgress();
                this.UpdateForecast(this.MakeTotalTimeForecast(startTime, processedSurveys, surveysCount));
                processedSurveys++;
            }
            var rand = RandomObject;
            this.UpdateStatus("partial capi events", statistics.PartialCAPIEventsCount);
            foreach (var surveyId in surveyIds)
            {
                if (rand.Next(10) != 5) continue;

                var responsible = this.SurveyStorage.GetById(surveyId).Responsible;
                this.CompleteSurvey(surveyId, SurveyStatus.Initial, responsible);
                this.FillAnswers(surveyId, true);
                this.CompleteSurvey(surveyId, SurveyStatus.Complete, responsible);
                this.UpdateProgress();
            }
        }
        private TimeSpan MakeTotalTimeForecast(DateTime startTime, int processedItems, int itemsCount)
        {
            var applicationWorkingTime = (DateTime.Now - this.statistics.StartTime).Ticks;
            var capiEventsSpentTime = (DateTime.Now - startTime).Ticks;
            var averageFillingSpeed = capiEventsSpentTime/processedItems;
            var result = TimeSpan.FromTicks(applicationWorkingTime + averageFillingSpeed*(itemsCount - processedItems));
            return result;
        }

        private void FillAnswers(Guid surveyId, bool partially = false)
        {
            var rand = RandomObject;
            var survey = this.SurveyStorage.GetById(surveyId);
            survey.ConnectChildsWithParent();
            var autoPropagateQuestoins = survey.GetQuestions().Where(x => x is IAutoPropagate).ToList();
            // to answer AutoPropagate questions
            foreach (var question in autoPropagateQuestoins)
            {
                if (partially && rand.Next(10) != 7)
                {
                    continue;
                }
                if (question.Enabled)
                {
                    this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey,
                                                                     this.GetDummyCompleteAnswers(question),
                                                                     GetDummyAnswer(question), null));
                }
            }
            var allQuestions = survey.GetQuestions().Where(x => !(x is IAutoPropagate)).ToList();
            foreach (var question in allQuestions)
            {
                if (partially && rand.Next(10) != 7)
                {
                    continue;
                }
                if (question.Enabled)
                {
                    this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey,
                                                                     GetDummyCompleteAnswers(question),
                                                                     GetDummyAnswer(question),
                                                                     question.PropagationPublicKey));
                }
            }
        }

        private void CompleteSurvey(Guid surveyId, SurveyStatus status, UserLight initiator)
        {
            this.CommandService.Execute(new ChangeStatusCommand()
            {
                CompleteQuestionnaireId = surveyId,
                Status = status,
                Responsible = initiator
            });
        }

        private List<Guid> GetSurveyIds()
        {
            var uniqueARIds = new List<UniqueEventsResults>();
            using (IDocumentSession session = this.RavenStore.OpenSession())
            {
                uniqueARIds.AddRange(session
                                         .Query<UniqueEventsResults, UniqueEventsIndex>()
                                         .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                                         .Take(int.MaxValue));
            }
            var surveyIds = new List<Guid>();
            foreach (var arId in uniqueARIds)
            {
                if (this.SurveyStorage.GetById(arId.EventSourceId) != null)
                {
                    surveyIds.Add(arId.EventSourceId);
                }
            }
            return surveyIds;
        }

        private void ClearDatabase()
        {

            int initialViewCount;
            using (IDocumentSession session = this.RavenStore.OpenSession())
            {
                // this will also materialize index if it is out of date or was just created
                initialViewCount = session
                    .Query<object>(IndexForDelete)
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            this.RavenStore
                .DatabaseCommands
                .DeleteByIndex(IndexForDelete, new IndexQuery());

            int resultViewCount;
            using (IDocumentSession session = this.RavenStore.OpenSession())
            {
                resultViewCount = session
                    .Query<object>(IndexForDelete)
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            if (resultViewCount > 0)
                throw new Exception(string.Format(
                    "Failed to delete all views. Initial view count: {0}, remaining view count: {1}.",
                    initialViewCount, resultViewCount));

        }

        private void ClearAllViews()
        {
            this.RavenStore
                .DatabaseCommands
                .EnsureDatabaseExists("Views");

            this.RavenStore
                .DatabaseCommands
                .ForDatabase("Views")
                .PutIndex(
                    "AllViews",
                    new IndexDefinition { Map = "from doc in docs let DocId = doc[\"@metadata\"][\"@id\"] select new {DocId};" },
                    overwrite: true);

            int initialViewCount;
            using (IDocumentSession session = this.RavenStore.OpenSession("Views"))
            {
                // this will also materialize index if it is out of date or was just created
                initialViewCount = session
                    .Query<object>("AllViews")
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            this.RavenStore
                .DatabaseCommands
                .ForDatabase("Views")
                .DeleteByIndex("AllViews", new IndexQuery());

            int resultViewCount;
            using (IDocumentSession session = this.RavenStore.OpenSession("Views"))
            {
                resultViewCount = session
                    .Query<object>("AllViews")
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            if (resultViewCount > 0)
                throw new Exception(string.Format(
                    "Failed to delete all views. Initial view count: {0}, remaining view count: {1}.",
                    initialViewCount, resultViewCount));
        }

        private void GenerateSupervisorsEvents()
        {
            UserDocument hq = this.GenerateHeadquarter();

            this.UpdateStatus("store template", statistics.TemplatesCount);
            this.StoreTemplate(template, hq);

            this.UpdateStatus("create supervisors", statistics.SupervisorsCount);
            var supervisors = this.GenerateSupervisors(int.Parse(this.supervisorsCount.Text));
            this.UpdateStatus("create interviewers", statistics.InterviewersCount);
            var interviewers = this.GenerateInterviewers(int.Parse(this.interviewersCount.Text), supervisors);
            this.UpdateStatus("create surveys", statistics.SurveysCount);
            var surveyIds = this.GenerateSurveys(int.Parse(this.surveys_amount.Text), template, hq).ToList();

            if (statistics.hasSupervisorEvents)
            {
                var startTime = DateTime.Now;
                var total = surveyIds.Count;
                var processed = 1;
                this.UpdateStatus(
                    "supervisor - interviewer - status", statistics.SupervisorEventsCount);
                foreach (var surveyId in surveyIds)
                {
                    var responsible = this.AssignSurveyToOneOfSupervisors(surveyId, supervisors, hq);
                    this.UpdateProgress();
                    this.MarkSurveysAsReadyForSyncWithStatus(SurveyStatus.Unassign, surveyId, responsible);
                    this.UpdateProgress();

                    var supervisorTeamMembers =
                        interviewers.Where(x => x.Supervisor.Id == responsible.PublicKey).ToList();
                    if (supervisorTeamMembers.Count > 0)
                    {
                        var interviwer = this.AssignSurveyToOneOfInterviewers(surveyId, supervisorTeamMembers);
                        this.UpdateProgress();
                        this.MarkSurveysAsReadyForSyncWithStatus(SurveyStatus.Initial, surveyId, interviwer);
                        this.UpdateProgress();
                    }
                    else
                    {
                        this.UpdateProgress();
                        this.UpdateProgress();
                    }
                    this.UpdateForecast(this.MakeTotalTimeForecast(startTime, processed, total));
                    processed++;
                }
            }

            if (featuredQuestions != null && statistics.hasFeaturedQuestions)
            {
                var startTime = DateTime.Now;
                var total = surveyIds.Count;
                var processed = 1;
                this.UpdateStatus("create featured questions", statistics.FeaturedQuestionsCount);
                foreach (var surveyId in surveyIds)
                {
                    this.FillFeaturedAnswers(surveyId, featuredQuestions, hq);
                    this.UpdateForecast(this.MakeTotalTimeForecast(startTime, processed, total));
                    processed++;
                }
            }
        }

        private QuestionnaireDocument ReadTemplate(string path)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var template = JsonConvert.DeserializeObject<QuestionnaireDocument>(File.OpenText(path).ReadToEnd(), settings);
            return template;
        }

        private UserDocument GenerateHeadquarter()
        {
            UserDocument hq;

            if (statistics.hasHeadQuarter)
            {
                this.UpdateStatus("create headquarter", statistics.HeadquartersCount);
                var hqView =
                    this.userViewFactory.Load(
                        new UserViewInputModel(txtHQName.Text, txtHQName.Text));
                if (hqView == null)
                {
                    hq = new UserDocument()
                    {
                        PublicKey = Guid.NewGuid(),
                        UserName = string.IsNullOrEmpty(txtHQName.Text) ? "hq" : txtHQName.Text,
                        Roles = new List<UserRoles>() { UserRoles.Headquarter }
                    };
                    CommandService.Execute(new CreateUserCommand(hq.PublicKey, hq.UserName, SimpleHash.ComputeHash(hq.UserName), hq.UserName + "@mail.org", hq.Roles.ToArray(), false, null));
                }
                else
                {
                    hq = new UserDocument() { PublicKey = hqView.PublicKey };
                }
                this.UpdateProgress();
            }
            else
            {
                var hqView =
                    this.userBrowseViewFactory.Load(
                        new UserBrowseInputModel(UserRoles.Headquarter));
                if (hqView.Items.Any())
                {
                    hq = new UserDocument() { PublicKey = hqView.Items.FirstOrDefault().Id };
                }
                else
                {
                    hq = new UserDocument()
                    {
                        PublicKey = Guid.NewGuid(),
                        UserName = "hq",
                        Roles = new List<UserRoles>() { UserRoles.Headquarter }
                    };
                    CommandService.Execute(new CreateUserCommand(hq.PublicKey, hq.UserName, SimpleHash.ComputeHash(hq.UserName), hq.UserName + "@mail.org", hq.Roles.ToArray(), false, null));
                }
            }

            return hq;
        }

        private void StoreTemplate(QuestionnaireDocument template, UserDocument initiator)
        {
            this.CommandService.Execute(
                new ImportQuestionnaireCommand(initiator == null ? Guid.Empty : initiator.PublicKey, template));
            this.UpdateProgress();
        }

        private List<UserDocument> GenerateSupervisors(int count)
        {
            var result = new List<UserDocument>();
            for (int i = 0; i < count; i++)
            {
                var supervisor = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = string.Format("supervisor_{0}_{1}", i, DateTime.Now.Ticks),
                    Roles = new List<UserRoles> { UserRoles.Supervisor }
                };
                CommandService.Execute(new CreateUserCommand(supervisor.PublicKey, supervisor.UserName, SimpleHash.ComputeHash(supervisor.UserName), supervisor.UserName + "@example.com", supervisor.Roles.ToArray(), false, null));
                result.Add(supervisor);
                this.UpdateProgress();
            }
            return result;
        }

        private List<UserDocument> GenerateInterviewers(int count, List<UserDocument> supervisors)
        {
            var rand = new Random();
            var result = new List<UserDocument>();
            var supervisorsCount = supervisors.Count;
            for (int i = 0; i < count; i++)
            {
                var supervisor = supervisors[rand.Next(supervisorsCount)];
                var interviewer = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = string.Format("interviewer_{0}_{1}", i, DateTime.Now.Ticks),
                    Supervisor = supervisor.ToUserLight(),
                    Roles = new List<UserRoles> { UserRoles.Operator }
                };
                CommandService.Execute(new CreateUserCommand(interviewer.PublicKey, interviewer.UserName, SimpleHash.ComputeHash(interviewer.UserName), interviewer.UserName + "@mail.org", interviewer.Roles.ToArray(), false, interviewer.Supervisor));
                result.Add(interviewer);
                this.UpdateProgress();
            }
            return result;
        }

        private IEnumerable<Guid> GenerateSurveys(int count, QuestionnaireDocument template, UserDocument initiator)
        {
            var startTime = DateTime.Now;
            var result = new List<Guid>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(id, template.PublicKey, initiator.ToUserLight()));
                result.Add(id);
                this.UpdateProgress();
                this.UpdateForecast(this.MakeTotalTimeForecast(startTime, i+1, count+1));
            }
            return result;
        }

        private void FillFeaturedAnswers(Guid surveyId, IEnumerable<IQuestion> featuredQuestions, UserDocument initiator)
        {
            foreach (var question in featuredQuestions)
            {
                this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey, GetDummyAnswers(question), GetDummyAnswer(question), null));
                this.UpdateProgress();
            }
        }

        private static string GetDummyAnswer(IQuestion q)
        {
            var rand = RandomObject;
            if (q is IMultyOptionsQuestion)
            {
                return null;
            }
            if (q is ISingleQuestion)
            {
                return null;
            }
            if (q is IDateTimeQuestion)
            {
                return (new DateTime(rand.Next(1940, 2003), rand.Next(1, 13), rand.Next(1, 29))).ToString(CultureInfo.InvariantCulture);
            }
            if (q is INumericQuestion)
            {
                return rand.Next(100).ToString(CultureInfo.InvariantCulture);
            }
            if (q is IAutoPropagate)
            {
                var question = (AutoPropagateCompleteQuestion)q;
                var maxValue = question.MaxValue;
                return rand.Next(maxValue).ToString(CultureInfo.InvariantCulture);
            }
            if (q is ITextCompleteQuestion)
            {
                return "value " + rand.Next();
            }
            return string.Empty;
        }

        private List<Guid> GetDummyAnswers(IQuestion q)
        {
            var random = RandomObject;
            if (q is IMultyOptionsQuestion)
            {
                var rand = random;
                var question = (MultyOptionsQuestion)q;
                var answersCount = question.Answers.Count;
                var selectedAnswersCount = rand.Next(1, answersCount + 1);
                var result = new List<Guid>();
                for (int i = 0; i < selectedAnswersCount; i++)
                {
                    var answer = question.Answers[rand.Next(0, answersCount)];
                    result.Add(answer.PublicKey);
                }
                return result.Distinct().ToList();
            }
            if (q is ISingleQuestion)
            {
                var question = (SingleQuestion)q;
                var answersCount = question.Answers.Count;
                return new List<Guid>()
                    {
                        question.Answers[random.Next(answersCount)].PublicKey
                    };
            }
            return new List<Guid>() { };
        }

        private List<Guid> GetDummyCompleteAnswers(IQuestion q)
        {
            var random = RandomObject;
            if (q is IMultyOptionsQuestion)
            {
                var rand = random;
                var question = (MultyOptionsCompleteQuestion)q;
                var answersCount = question.Answers.Count;
                var selectedAnswersCount = rand.Next(1, answersCount + 1);
                var result = new List<Guid>();
                for (int i = 0; i < selectedAnswersCount; i++)
                {
                    var answer = question.Answers[rand.Next(0, answersCount)];
                    result.Add(answer.PublicKey);
                }
                return result.Distinct().ToList();
            }
            if (q is ISingleQuestion)
            {
                var question = (SingleCompleteQuestion)q;
                var answersCount = question.Answers.Count;
                return new List<Guid>()
                    {
                        question.Answers[random.Next(answersCount)].PublicKey
                    };
            }
            return new List<Guid>() { Guid.NewGuid() };
        }

        private UserDocument AssignSurveyToOneOfSupervisors(Guid surveyId, List<UserDocument> supervisors, UserDocument hq)
        {
            var rand = RandomObject;
            var responsibleSupervisor = supervisors[rand.Next(supervisors.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleSupervisor.ToUserLight()));
            return responsibleSupervisor;
        }

        private UserDocument AssignSurveyToOneOfInterviewers(Guid surveyId, List<UserDocument> interviewers)
        {
            var rand = RandomObject;
            var responsibleInterviewer = interviewers[rand.Next(interviewers.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleInterviewer.ToUserLight()));
            return responsibleInterviewer;
        }

        private void MarkSurveysAsReadyForSyncWithStatus(
            SurveyStatus surveyStatus, Guid surveyId, UserDocument initiator)
        {
            this.CommandService.Execute(
                new ChangeStatusCommand()
                    {
                        CompleteQuestionnaireId = surveyId,
                        Status = surveyStatus,
                        Responsible = initiator.ToUserLight()
                    });
        }

        private void templatePath_Enter(object sender, EventArgs e)
        {
            var templateFileChooser = new OpenFileDialog
                {
                    InitialDirectory = "d:\\",
                    Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

            if (templateFileChooser.ShowDialog() == DialogResult.OK)
            {
                templatePath.Text = templateFileChooser.FileName;
            }
        }

        private void chkHeadquarter_CheckedChanged(object sender, EventArgs e)
        {
            txtHQName.Enabled = (sender as CheckBox).Checked;
        }

        private void LoadTestDataGenerator_Load(object sender, EventArgs e)
        {

        }
    }
}
