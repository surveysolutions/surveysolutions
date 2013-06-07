using System.Configuration;
using System.Text;
using Main.Core.Commands.Questionnaire;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Main.DenormalizerStorage;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage.RavenDB.RavenIndexes;
using Ncqrs.Restoring.EventStapshoot;
using Raven.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using System.Windows.Forms;

namespace LoadTestDataGenerator
{
    using System.Threading.Tasks;

    public partial class LoadTestDataGenerator : Form
    {
        protected readonly ICommandService CommandService;
        protected readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> SurveyStorage;
        protected readonly DocumentStore RavenStore;
        const string IndexForDelete = "AllEvents";
        const string IndexForStatistics = "EventsStatistics";
        string databaseName = "";

        private QuestionnaireDocument template;
        private IEnumerable<IQuestion> featuredQuestions;

        public LoadTestDataGenerator(ICommandService commandService,
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveyStorage,
            DocumentStore ravenStore)
        {
            this.RavenStore = ravenStore;
            this.CommandService = commandService;
            this.SurveyStorage = surveyStorage;
            InitializeComponent();

            this.databaseName = ConfigurationManager.AppSettings["Raven.DefaultDatabase"];
            defaultDatabaseName.Text = this.databaseName;

            this.UpdateEventsStatistics();
        }

        private void generate_Click(object sender, EventArgs e)
        {
            this.PrepareToStart();

            template = this.ReadTemplate(this.templatePath.Text);
            if (chkSetAnswers.Checked)
            {
                featuredQuestions = template.GetFeaturedQuestions();
            }

            this.Start();

            Task.Run(() =>
            {
                this.PrepareDatabase();

                if (this.clearDatabase.Checked)
                {
                    UpdateStatus("start cleaning database");
                    this.ClearDatabase();
                    UpdateStatus("database cleaned successfully");
                }

                this.UpdateEventsStatistics();

                if (this.generateSupervisorEvents.Checked)
                {
                    UpdateStatus("start generating supervisor's events");
                    this.GenerateSupervisorsEvents();
                    UpdateStatus("supervisor's events generated successfully");
                }

                this.UpdateEventsStatistics();

                if (this.generateCapiEvents.Checked)
                {
                    UpdateStatus("start generating CAPI's events");
                    this.GenerateCapiEvents();
                    UpdateStatus("CAPI's events generated successfully");
                }
            });
        }

        private void PrepareToStart()
        {
            generate.Enabled = false;
        }

        private void Start()
        {
            ctrlProgress.Maximum = (int.Parse(surveys_amount.Text) * (chkGenerateSnapshoots.Checked ? 5 : 4))
                                   + int.Parse(supervisorsCount.Text) + int.Parse(interviewersCount.Text)
                                   + (chkSetAnswers.Checked ? featuredQuestions.Count() : 0)
                                   + (chkHeadquarter.Checked ? 2 : 1);
        }

        private void Stop()
        {
            generate.InvokeIfRequired<Button>(x => x.Enabled = true);
            ctrlProgress.GetCurrentParent().InvokeIfRequired(x => ctrlProgress.Value = 0);
        }

        private void IncreaseProgress()
        {
            ctrlProgress.GetCurrentParent().InvokeIfRequired(x => ctrlProgress.Value += 1);
        }

        private void UpdateStatus(string status)
        {
            processStatus.Text = status;
        }

        private void UpdateEventsStatistics()
        {
            var statistics = new List<EventStatisticItem>();
            using (IDocumentSession session = this.RavenStore.OpenSession())
            {
                statistics.AddRange(session
                    .Query<EventStatisticItem>(IndexForStatistics)
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow()));
            }
            var sb = new StringBuilder();
            foreach (var statistic in statistics.OrderByDescending(x => x.Count))
            {
                sb.AppendFormat("{0,-7} {1}", statistic.Count, statistic.Type);
                sb.AppendLine();
            }
            eventsStatistics.Text = sb.ToString();
        }

        private void PrepareDatabase()
        {
            this.RavenStore
                .DatabaseCommands
                .EnsureDatabaseExists(this.databaseName);

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
            var surveyIds = this.GetSurveyIds();
            foreach (var surveyId in surveyIds)
            {
                FillAnswers(surveyId);
                var responsible = this.SurveyStorage.GetById(surveyId).Responsible;
                CompleteSurvey(surveyId, SurveyStatus.Complete, responsible);
                this.CreateSnapshoot(surveyId);
            }

            var rand = RandomObject;

            foreach (var surveyId in surveyIds)
            {
                if (rand.Next(10) != 5) continue;

                var responsible = this.SurveyStorage.GetById(surveyId).Responsible;
                this.CompleteSurvey(surveyId, SurveyStatus.Initial, responsible);
                this.FillAnswers(surveyId, true);
                this.CompleteSurvey(surveyId, SurveyStatus.Complete, responsible);
                this.CreateSnapshoot(surveyId);
            }
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
                this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey, this.GetDummyAnswers(question), GetDummyAnswer(question), null));
            }
            var allQuestions = survey.GetQuestions().Where(x => !(x is IAutoPropagate)).ToList();
            foreach (var question in allQuestions)
            {
                if (partially && rand.Next(10) != 7)
                {
                    continue;
                }
                this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey, GetDummyAnswers(question), GetDummyAnswer(question), question.PropagationPublicKey));
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

        private void GenerateSupervisorsEvents()
        {
            UserDocument hq = null;
            if (chkHeadquarter.Checked && !string.IsNullOrEmpty(txtHQName.Text))
            {
                hq = this.GenerateHeadquarter();
            }

            this.StoreTemplate(template, hq);

            var supervisors = this.GenerateSupervisors(int.Parse(this.supervisorsCount.Text));
            var interviewers = this.GenerateInterviewers(int.Parse(this.interviewersCount.Text), supervisors);
            var surveyIds = this.GenerateSurveys(int.Parse(this.surveys_amount.Text), template, hq).ToList();
            foreach (var surveyId in surveyIds)
            {
                if (featuredQuestions != null)
                {
                    this.FillFeaturedAnswers(surveyId, featuredQuestions, hq);
                }

                var responsible = this.AssignSurveyToOneOfSupervisors(surveyId, supervisors, hq);
                var supervisorTeamMembers = interviewers.Where(x => x.Supervisor.Id == responsible.PublicKey).ToList();
                if (supervisorTeamMembers.Count > 0)
                {
                    this.AssignSurveyToOneOfInterviewers(surveyId, supervisorTeamMembers);
                    this.MarkSurveysAsReadyForSyncWithStatus(SurveyStatus.Initial, surveyId, responsible);
                }
            }
            if (chkGenerateSnapshoots.Checked)
            {
                foreach (var surveyId in surveyIds)
                {
                    this.CreateSnapshoot(surveyId);
                }
            }

            this.Stop();
        }

        private QuestionnaireDocument ReadTemplate(string path)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var template = JsonConvert.DeserializeObject<QuestionnaireDocument>(File.OpenText(path).ReadToEnd(), settings);
            return template;
        }

        private UserDocument GenerateHeadquarter()
        {
            var hq = new UserDocument()
                {
                    PublicKey = Guid.NewGuid(),
                    UserName = txtHQName.Text,
                    Roles = new List<UserRoles>() { UserRoles.Headquarter }
                };
            CommandService.Execute(new CreateUserCommand(hq.PublicKey, hq.UserName, SimpleHash.ComputeHash(hq.UserName), hq.UserName + "@mail.org", hq.Roles.ToArray(), false, null));
            this.IncreaseProgress();

            return hq;
        }

        private void StoreTemplate(QuestionnaireDocument template, UserDocument initiator)
        {
            this.CommandService.Execute(
                new ImportQuestionnaireCommand(initiator == null ? Guid.Empty : initiator.PublicKey, template));
            this.IncreaseProgress();
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
                CommandService.Execute(new CreateUserCommand(supervisor.PublicKey, supervisor.UserName, SimpleHash.ComputeHash(supervisor.UserName), supervisor.UserName + "@worldbank.org", supervisor.Roles.ToArray(), false, null));
                result.Add(supervisor);
                this.IncreaseProgress();
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
                this.IncreaseProgress();
            }
            return result;
        }

        private IEnumerable<Guid> GenerateSurveys(int count, QuestionnaireDocument template, UserDocument initiator)
        {
            var result = new List<Guid>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(id, template.PublicKey, initiator.ToUserLight()));
                result.Add(id);
                this.IncreaseProgress();
            }
            return result;
        }

        private void FillFeaturedAnswers(Guid surveyId, IEnumerable<IQuestion> featuredQuestions, UserDocument initiator)
        {
            foreach (var question in featuredQuestions)
            {
                this.CommandService.Execute(new SetAnswerCommand(surveyId, question.PublicKey, GetDummyAnswers(question), GetDummyAnswer(question), null));
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

        private static Random RandomObject = new Random((int)DateTime.Now.Ticks);

        private UserDocument AssignSurveyToOneOfSupervisors(Guid surveyId, List<UserDocument> supervisors, UserDocument hq)
        {
            var rand = RandomObject;
            var responsibleSupervisor = supervisors[rand.Next(supervisors.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleSupervisor.ToUserLight()));
            this.IncreaseProgress();
            return responsibleSupervisor;
        }

        private void AssignSurveyToOneOfInterviewers(Guid surveyId, List<UserDocument> interviewers)
        {
            var rand = RandomObject;
            var responsibleInterviewer = interviewers[rand.Next(interviewers.Count)];
            this.CommandService.Execute(new ChangeAssignmentCommand(surveyId, responsibleInterviewer.ToUserLight()));
            this.IncreaseProgress();
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
            this.IncreaseProgress();
        }

        private void CreateSnapshoot(Guid surveyId)
        {
            this.CommandService.Execute(new CreateSnapshotForAR(surveyId, typeof(CompleteQuestionnaireAR)));
            this.IncreaseProgress();
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
