using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CAPI.Android.Core.Model;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.User;
using Main.Core.Utility;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Tools.CapiDataGenerator;
using WB.UI.Shared.Web;

namespace CapiDataGenerator
{
    using WB.Core.Infrastructure.Raven.Implementation.ReadSide;

    public class MainPageModel : MvxViewModel
    {
        private const int MaxAllowedRosterCount = 40;
        private static readonly decimal[] EmptyPropagationVector = new decimal[] { };

        private ICommandService commandService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ICommandService>();
            }
        }

        private IOpenFileService openFileService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IOpenFileService>();
            }
        }

        private IBackup backupService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IBackup>();
            }
        }

        private IViewFactory<UserListViewInputModel, UserListView> userListViewFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IViewFactory<UserListViewInputModel, UserListView>>();
            }
        }

        private IRavenReadSideRepositoryWriterRegistry writerRegistry
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRavenReadSideRepositoryWriterRegistry>();
            }
        }

        private IChangeLogManipulator changeLogManipulator
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IChangeLogManipulator>();
            }
        }

        private IPlainQuestionnaireRepository questionnaireRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>();
            }
        }

        static readonly Random _rand = new Random();
        readonly Timer _timer = new Timer(1000);
        private DateTime _startTime;
        private UserLight _headquarterUser;

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

        public MainPageModel()
        {
            this.GenerateCommand = new MvxCommand(this.Generate, () => this.CanGenerate);
            this.OpenTemplateCommand = new MvxCommand(this.OpenTemplate, () => this.CanGenerate);

            
            _timer.Elapsed += _timer_Elapsed;
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            //create hq if none was found needed
            var hq = userListViewFactory.Load(new UserListViewInputModel() { Role = UserRoles.Headquarter }).Items.FirstOrDefault();
            if (hq == null)
            {
                Guid hqId = Guid.Parse("DF120CFD-B624-4E3F-BED8-7BE9033CCBC6");
                string userName = "hq";
                commandService.Execute(new CreateUserCommand(hqId, userName, SimpleHash.ComputeHash("Headquarter1"), "hq@example.com", new UserRoles[] { UserRoles.Headquarter }, false, false, null));

                this._headquarterUser = new UserLight(hqId, userName);
                this.HeadquarterName = this._headquarterUser.Name;
            }
            else
            {
                this._headquarterUser = new UserLight(hq.UserId, hq.UserName);
                this.HeadquarterName = this._headquarterUser.Name;
            }

            //create supervisor if none was found needed
            this.SupervisorList = new ObservableCollection<UserListItem>(userListViewFactory.Load(new UserListViewInputModel() 
                { Role = UserRoles.Supervisor }).Items);
            
            if (this.SupervisorList.Count == 0)
            {
                Guid superId = Guid.Parse("1A94734B-DEAD-462D-98F1-C8F44136C4E4");
                string userName = "supervisor";
                string userEmail = "s@example.com";
                commandService.Execute(new CreateUserCommand(superId, userName, SimpleHash.ComputeHash("Supervisor1"), userEmail, new UserRoles[] { UserRoles.Supervisor}, false, false, null));


                //var emptySupervisor = new UserListItem(Guid.Empty, "Please, create new supervisor", null, DateTime.Now, false, false, null);
                var createdSupervisor = new UserListItem(superId, userName, userEmail, DateTime.Now, false, false, null);
                this.SupervisorList = new ObservableCollection<UserListItem>() { createdSupervisor };
                this.SelectedSupervisor = createdSupervisor;
            }
            
            this.WorkingModeList = new ObservableCollection<string>(Enum.GetNames(typeof(GenerationMode)));
            this.SelectedWorkingMode = AppSettings.Instance.CurrentMode;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.ElapsedTime = DateTime.Now.Subtract(this._startTime);
        }

        private void OpenTemplate()
        {
            TemplatePath = openFileService.OpenFileDialog();
        }

        public MvxCommand GenerateCommand { get; set; }
        public MvxCommand OpenTemplateCommand { get; set; }

        private string _headquarterName = string.Empty;
        public string HeadquarterName
        {
            get { return _headquarterName; }
            set
            {
                _headquarterName = value;
                RaisePropertyChanged(() => HeadquarterName);
            }
        }

        private string _templatePath = string.Empty;
        public string TemplatePath
        {
            get { return _templatePath; }
            set
            {
                _templatePath = value;
                RaisePropertyChanged(() => TemplatePath);
            }
        }

        private string _interviewersCount = string.Empty;
        public string InterviewersCount
        {
            get { return _interviewersCount; }
            set
            {
                _interviewersCount = value;
                RaisePropertyChanged(() => InterviewersCount);
            }
        }

        private string _questionnairesCount = string.Empty;
        public string QuestionnairesCount {
            get
            {
                return _questionnairesCount;
            }
            set
            {
                _questionnairesCount = value;
                RaisePropertyChanged(() => QuestionnairesCount);
            }
        }

        private string _answersCount = "10";
        public string AnswersCount {
            get
            {
                return _answersCount;
            }
            set
            {
                _answersCount = value;
                RaisePropertyChanged(() => AnswersCount);
            }
        }

        private string _commentsCount = "5";
        public string CommentsCount
        {
            get
            {
                return _commentsCount;
            }
            set
            {
                _commentsCount = value;
                RaisePropertyChanged(() => CommentsCount);
            }
        }

        private string _statusesCount = "20";
        public string StatusesCount
        {
            get
            {
                return _statusesCount;
            }
            set
            {
                _statusesCount = value;
                RaisePropertyChanged(() => StatusesCount);
            }
        }

        private int _totalCount = 1;
        public int TotalCount {
            get
            {
                return _totalCount;
            }
            set
            {
                _totalCount = value;
                RaisePropertyChanged(() => TotalCount);
            }
        }

        private int _progress = 0;
        public int Progress {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }
        
        private UserListItem _selectedSupervisor = null;
        public UserListItem SelectedSupervisor
        {
            get
            {
                return _selectedSupervisor;
            }
            set
            {
                _selectedSupervisor = value;
                RaisePropertyChanged(() => SelectedSupervisor);
            }
        }

        private GenerationMode? selectedWorkingMode = null;
        public GenerationMode? SelectedWorkingMode
        {
            get
            {
                return selectedWorkingMode;
            }
            set
            {
                selectedWorkingMode = value;
                RaisePropertyChanged(() => SelectedWorkingMode);
            }
        }

        private ObservableCollection<string> workingModeList = new ObservableCollection<string>();
        public ObservableCollection<string> WorkingModeList
        {
            get
            {
                return workingModeList;
            }
            set
            {
                workingModeList = value;
                RaisePropertyChanged(() => WorkingModeList);
            }
        }

        private ObservableCollection<LogMessage> _logMessages = new ObservableCollection<LogMessage>();
        public ObservableCollection<LogMessage> LogMessages
        {
            get
            {
                return _logMessages;
            }
            set
            {
                _logMessages = value;
                RaisePropertyChanged(() => LogMessages);
            }
        }

        private ObservableCollection<string> _interviewersList = new ObservableCollection<string>();
        public ObservableCollection<string> InterviewersList
        {
            get
            {
                return _interviewersList;
            }
            set
            {
                _interviewersList = value;
                RaisePropertyChanged(() => InterviewersList);
            }
        }

        private ObservableCollection<UserListItem> _supervisorList = new ObservableCollection<UserListItem>();
        public ObservableCollection<UserListItem> SupervisorList
        {
            get
            {
                return _supervisorList;
            }
            set
            {
                _supervisorList = value;
                RaisePropertyChanged(() => SupervisorList);
            }
        }

        private bool _canGenerate = true;
        public bool CanGenerate {
            get
            {
                return _canGenerate;
            }
            set
            {
                _canGenerate = value;
                RaisePropertyChanged(() => CanGenerate);
            }
        }

        private TimeSpan _elapsedTime;

        public TimeSpan ElapsedTime
        {
            get
            {
                return _elapsedTime;
            }
            set
            {
                _elapsedTime = value;
                RaisePropertyChanged(() => ElapsedTime);
            }
        }

        public void Generate()
        {
            if (string.IsNullOrEmpty(TemplatePath) || SelectedSupervisor == null ||
                this._headquarterUser == null || SelectedWorkingMode == null)
                return;

            LogMessages.Clear();

            Task.Run(() =>
            {
                CanGenerate = false;
                Log("start");

                _startTime = DateTime.Now;
                _timer.Start();

                QuestionnaireDocument questionnaireDocument = null;

                try
                {
                    questionnaireDocument = ReadTemplate(this.TemplatePath, false);
                    questionnaireDocument.Title = "(generated) " + questionnaireDocument.Title;
                }
                catch (Exception)
                {
                    try
                    {
                        questionnaireDocument = ReadTemplate(this.TemplatePath, true);
                        questionnaireDocument.Title = "(generated) " + questionnaireDocument.Title;
                    }
                    catch (Exception)
                    {

                        Log("bad template (do not forget to unzip it)");
                    }
                }

                if (questionnaireDocument != null)
                {
                    int interviewsCount = 0;
                    int interviewersCount = 0;
                    int answersCount = 0;
                    int commentsCount = 0;
                    int statusesCount = 0;
                    int.TryParse(QuestionnairesCount, out interviewsCount);
                    int.TryParse(InterviewersCount, out interviewersCount);
                    int.TryParse(AnswersCount, out answersCount);
                    int.TryParse(CommentsCount, out commentsCount);
                    int.TryParse(StatusesCount, out statusesCount);

                    if (statusesCount > 100)
                    {
                        statusesCount = 100;
                        StatusesCount = statusesCount.ToString();
                    }
                    
                    //AppSettings.Instance.CurrentMode = this.SelectedWorkingMode.Value;

                    var notFeaturedQuestions = questionnaireDocument.GetAllQuestions().Where(x => !x.Featured).ToList();
                    var questionsCount = notFeaturedQuestions.Count();

                    answersCount = (int) (questionsCount*((double) answersCount/100));
                    commentsCount = (int) (questionsCount*((double) commentsCount/100));
                    statusesCount = (int) (interviewersCount*interviewsCount*((double) statusesCount/100));

                    TotalCount = interviewersCount + interviewersCount*interviewsCount*(answersCount + commentsCount + 2) + statusesCount;

                    this.EnableCacheInAllRepositoryWriters();
                    try
                    {
                        var users = CreateInterviewers(interviewersCount);
                        var questionnaire = new Questionnaire(questionnaireDocument);
                        var state = new State(questionnaire.GetFixedRosterGroups());

                        this.ImportTemplate(questionnaireDocument);

                        var interviews = this.CreateInterviews(questionnaireDocument, interviewsCount, users, questionnaire, state);

                        CreateAnswers(answersCount, interviews, notFeaturedQuestions, questionnaire, questionnaireDocument, state);
                        CreateComments(commentsCount, interviews, notFeaturedQuestions, questionnaire);
                        ChangeStatuses(statusesCount, interviews);

                        if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitCapiAndSupervisor 
                            || AppSettings.Instance.CurrentMode == GenerationMode.DataSplitOnCapiCreatedAndSupervisor)
                        {
                            Log("create backup");
                            string backupPath = backupService.Backup();
                            string backupFileFullPath = backupPath + ".zip";
                            Log("backup was created ", Path.GetFileName(backupFileFullPath), backupFileFullPath);
                        }

                        Log("end");
                    }
                    catch (Exception e)
                    {
                        this.Log(e.Message);
                    }
                    finally
                    {
                        this.DisableCacheInAllRepositoryWriters();
                    }
                }

                _timer.Stop();
                Progress = 0;
                CanGenerate = true;
                
            });
        }

        private void ImportTemplate(IQuestionnaireDocument template)
        {
            this.Log("import template");
            this.commandService.Execute(new ImportFromDesigner(this._headquarterUser.Id, template));

            //incorrect. should be saved on denormalizer
            this.questionnaireRepository.StoreQuestionnaire(template.PublicKey, 1, template as QuestionnaireDocument);

            //this.commandService.Execute(new RegisterPlainQuestionnaire(template.PublicKey, 1));
        }

        private void ChangeStatuses(int statusesCount, Dictionary<Guid, Guid> interviews)
        {
            for (int z = 0; z < statusesCount; z++)
            {
                var interview = interviews.ElementAt(z);

                commandService.Execute(new CompleteInterviewCommand(interview.Key, interview.Value, "auto complete comment"));

                if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitCapiAndSupervisor ||
                    AppSettings.Instance.CurrentMode == GenerationMode.DataSplitOnCapiCreatedAndSupervisor)
                    changeLogManipulator.CloseDraftRecord(interview.Key);

                if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterApproved || 
                    AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterRejected ||
                    AppSettings.Instance.CurrentMode == GenerationMode.DataSplitSupervisorHeadquarter )
                {
                    commandService.Execute(new ApproveInterviewCommand(interview.Key, interview.Value, "auto approve comment"));
                }

                if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterApproved)
                {
                    commandService.Execute(new HqApproveInterviewCommand(interview.Key, interview.Value, "auto hq approve comment"));
                }
                else if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterRejected)
                {
                    commandService.Execute(new HqRejectInterviewCommand(interview.Key, interview.Value, "auto hq reject comment"));
                }

                UpdateProgress();
                LogStatus("set complete status", z, statusesCount);
            }
        }

        private void CreateComments(int commentsCount, Dictionary<Guid, Guid> interviews, IEnumerable<IQuestion> questions, IQuestionnaire questionnaire)
        {
            for (int j = 0; j < interviews.Count; j++)
            {
                var interview = interviews.ElementAt(j);
                for (int z = 0; z < commentsCount; z++)
                {
                    var question = questions.ElementAt(_rand.Next(questions.Count()));

                    int rosterLevel = questionnaire.GetRosterLevelForQuestion(question.PublicKey);

                    decimal[] rosterVector = Enumerable.Repeat((decimal) 0, rosterLevel).ToArray();

                    try
                    {
                        commandService.Execute(new CommentAnswerCommand(interviewId: interview.Key,
                            userId: interview.Value,
                            questionId: question.PublicKey,
                            rosterVector: rosterVector,
                            commentTime: DateTime.UtcNow,
                            comment: "auto comment"));
                    }
                    catch {}

                    UpdateProgress();
                    LogStatus("set comments", (j*commentsCount) + z, interviews.Count*commentsCount);
                }
            }
        }


        private List<UserLight> CreateInterviewers(int usersCount)
        {
            var users = new List<UserLight>();
            for (int userIndex = 0; userIndex < usersCount; userIndex++)
            {
                var uId = Guid.NewGuid();
                var userName = string.Format("i{0}_{1}", userIndex, DateTime.Now.ToString("MMddHHmm", CultureInfo.InvariantCulture));
                users.Add(new UserLight(uId, userName));
                commandService.Execute(new CreateUserCommand(publicKey: uId, userName: userName,
                    password: SimpleHash.ComputeHash(userName),
                    email: string.Concat(userName, "@example.com"), roles: new[] { UserRoles.Operator }, isLockedBySupervisor: false, isLockedByHQ: false,
                    supervsor: new UserLight(SelectedSupervisor.UserId, SelectedSupervisor.UserName)));
                InvokeOnMainThread(() => InterviewersList.Add(userName));
                UpdateProgress();
                LogStatus("create users", userIndex, usersCount);
            }

            return users;
        }

        private Dictionary<Guid, Guid> CreateInterviews(IQuestionnaireDocument template, int interviewCount, List<UserLight> users,
            IQuestionnaire questionnaire, State state)
        {
            var featuredQuestions = template.GetFeaturedQuestions();

            var interviews = new Dictionary<Guid, Guid>();
            for (int i = 0; i < users.Count; i++)
            {
                var interviewer = users[i];
                for (int j = 0; j < interviewCount; j++)
                {
                    LogStatus("create interviews", (i * interviewCount) + j, interviewCount * users.Count);

                    Guid interviewId = Guid.NewGuid();

                    if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitOnCapiCreatedAndSupervisor)
                    {
                        //version of template should be resolved correctly
                        var questionnaireVersion = 1;
 
                        commandService.Execute(new CreateInterviewOnClientCommand(interviewId: interviewId,
                            questionnaireId: template.PublicKey,
                            supervisorId: SelectedSupervisor.UserId,
                            userId: interviewer.Id,
                            answersTime: DateTime.UtcNow,
                            questionnaireVersion: questionnaireVersion));

                        //featured questions should also be filled

                        changeLogManipulator.CreateOrReopenDraftRecord(interviewId);
                    }
                    else
                    {
                        commandService.Execute(new CreateInterviewCommand(interviewId: interviewId,
                            questionnaireId: template.PublicKey,
                            supervisorId: SelectedSupervisor.UserId,
                            userId: interviewer.Id,
                            answersTime: DateTime.UtcNow,
                            answersToFeaturedQuestions: this.GetAnswersByFeaturedQuestions(featuredQuestions)));

                        commandService.Execute(new AssignInterviewerCommand(interviewId: interviewId, userId: SelectedSupervisor.UserId, interviewerId: interviewer.Id));

                    }

                                        
                    interviews.Add(interviewId, interviewer.Id);

                    UpdateProgress();
                }
            }

            if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitCapiAndSupervisor)
            {
                for (int i = 0; i < interviews.Count; i++)
                {
                    LogStatus("creating synchronize interview events", i, interviews.Count);

                    var interviewData = interviews.ElementAt(i);

                    commandService.Execute(new SynchronizeInterviewCommand(
                        interviewId: interviewData.Key,
                        userId: interviewData.Value,
                        sycnhronizedInterview: new InterviewSynchronizationDto(
                            id: interviewData.Key,
                            status: InterviewStatus.InterviewerAssigned,
                            userId: interviewData.Value,
                            questionnaireId: template.PublicKey,
                            questionnaireVersion: 1,
                            answers: new AnsweredQuestionSynchronizationDto[0],
                            disabledGroups: new HashSet<InterviewItemId>(),
                            disabledQuestions: new HashSet<InterviewItemId>(),
                            validAnsweredQuestions: new HashSet<InterviewItemId>(),
                            invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                            propagatedGroupInstanceCounts: null,
                            rosterGroupInstances: GetRosterGroupInstancesForSynchronization(questionnaire, state),
                            wasCompleted: false)));

                    changeLogManipulator.CreateOrReopenDraftRecord(interviewData.Key);
                    UpdateProgress();
                }
            }

            return interviews;
        }

        private static Dictionary<InterviewItemId, RosterSynchronizationDto[]> GetRosterGroupInstancesForSynchronization(IQuestionnaire questionnaire, State state)
        {
            return Enumerable
                .Concat(
                    state.FixedTitlesRosters.Select(rosterId => new { RosterId = rosterId, InstanceCount = questionnaire.GetFixedRosterTitles(rosterId).Count() }),
                    state.NumericRosterInstanceCounts.Select(pair => new { RosterId = pair.Key, InstanceCount = pair.Value }))
                .ToDictionary(
                    x => new InterviewItemId(x.RosterId),
                    x => Enumerable
                        .Range(0, x.InstanceCount)
                        .Select(instanceIndex => new RosterSynchronizationDto(x.RosterId, EmptyPropagationVector, instanceIndex, null, null))
                        .ToArray());
        }

        private Dictionary<Guid, object> GetAnswersByFeaturedQuestions(IEnumerable<IQuestion> featuredQuestions)
        {
            return featuredQuestions.ToDictionary(featuredQuestion => featuredQuestion.PublicKey, GetAnswerByQuestion);
        }

        private static object GetAnswerByQuestion(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    if (question.Answers.Count > 0)
                    {
                        try
                        {
                            return decimal.Parse(question.Answers[_rand.Next(0, question.Answers.Count - 1)].AnswerValue,
                                               CultureInfo.InvariantCulture);
                        }
                        catch{}
                    }
                    break;

                case QuestionType.MultyOption:
                    if (question.Answers.Count > 0)
                    {
                        var selectedAnswersCount = _rand.Next(1, question.Answers.Count);
                        var answers = new List<decimal>();
                        for (int i = 0; i < selectedAnswersCount; i++)
                        {
                            try
                            {
                                answers.Add(decimal.Parse(question.Answers[_rand.Next(0, question.Answers.Count - 1)].AnswerValue,
                                    CultureInfo.InvariantCulture));
                            }
                            catch {}
                        }
                        return answers.Distinct().ToArray();
                    }
                    break;

                case QuestionType.Numeric:
                    var numericQuestion = (INumericQuestion) question;
                    int maxValue = numericQuestion.MaxValue ?? 100;

                    return numericQuestion.IsInteger
                        ? _rand.Next(maxValue) as object
                        : new decimal(_rand.Next(maxValue)) as object;

                case QuestionType.DateTime:
                    return new DateTime(_rand.Next(1940, 2003), _rand.Next(1, 13), _rand.Next(1, 29));

                case QuestionType.Text:
                    return "value " + _rand.Next();

                case QuestionType.AutoPropagate:
                    return new decimal(_rand.Next(((IAutoPropagate) question).MaxValue));
            }

            return null;
        }

        private void CreateAnswers(int answersCount, Dictionary<Guid, Guid> interviews, IEnumerable<IQuestion> questions,
            IQuestionnaire questionnaire, QuestionnaireDocument questionnaireDocument, State state)
        {
            for (var j = 0; j < interviews.Count; j++)
            {
                var interview = interviews.ElementAt(j);
                for (int z = 0; z < answersCount; z++)
                {
                    IQuestion randomQuestion = questions.ElementAt(_rand.Next(questions.Count()));

                    Guid userId = interview.Value;
                    Guid interviewId = interview.Key;
                    var commands = CreateAnswerCommands(randomQuestion, interviewId, userId, questionnaire, questionnaireDocument, state);

                    foreach (ICommand command in commands.Where(command => command != null))
                    {
                        try
                        {
                            commandService.Execute(command);
                        }
                        catch {}
                    }

                    UpdateProgress();
                    LogStatus("answer questions", (j*answersCount) + z, interviews.Count*answersCount);
                }
            }
        }

        private static IEnumerable<ICommand> CreateAnswerCommands(IQuestion question, Guid interviewId, Guid userId,
            IQuestionnaire questionnaire, QuestionnaireDocument questionnaireDocument, State state)
        {
            var rosterLevel = questionnaire.GetRosterLevelForQuestion(question.PublicKey);

            if (rosterLevel == 1)
            {
                Guid rosterId = questionnaire.GetRostersFromTopToSpecifiedQuestion(question.PublicKey).First();

                Guid? rosterSizeQuestionId = questionnaireDocument.Find<IGroup>(rosterId).RosterSizeQuestionId;

                bool isFixedTitleRoster = state.FixedTitlesRosters.Contains(rosterId);
                bool isNumericQuestionRoster = rosterSizeQuestionId.HasValue && questionnaire.GetQuestionType(rosterSizeQuestionId.Value) == QuestionType.Numeric;

                if (isFixedTitleRoster)
                    return CreateAnswerCommandsForQuestionFromFixedTitlesRoster(question, rosterId, interviewId, userId, questionnaire, state);

                if (isNumericQuestionRoster)
                    return CreateAnswerCommandsForQuestionFromNumericQuestionRoster(
                        question, rosterId, rosterSizeQuestionId.Value, interviewId, userId, questionnaire, questionnaireDocument, state);

                return Enumerable.Empty<ICommand>();
            }

            return new[] { CreateAnswerCommand(question, interviewId, userId, EmptyPropagationVector, questionnaire, state) };
        }

        private static IEnumerable<ICommand> CreateAnswerCommandsForQuestionFromNumericQuestionRoster(
            IQuestion question, Guid rosterId, Guid rosterSizeQuestionId, Guid interviewId, Guid userId,
            IQuestionnaire questionnaire, QuestionnaireDocument questionnaireDocument, State state)
        {
            var commands = new List<ICommand>();

            bool isRosterSizeQuestionAnswered = state.NumericRosterInstanceCounts.ContainsKey(rosterId);

            if (!isRosterSizeQuestionAnswered)
            {
                commands.Add(
                    CreateAnswerCommand(questionnaireDocument.Find<IQuestion>(rosterSizeQuestionId), interviewId, userId, EmptyPropagationVector, questionnaire, state));
            }

            int rosterInstancesCount = state.NumericRosterInstanceCounts[rosterId];

            commands.AddRange(
                CreateAnswerCommandsForQuestionFromRoster(question, rosterInstancesCount, interviewId, userId, questionnaire, state));

            return commands;
        }

        private static IEnumerable<ICommand> CreateAnswerCommandsForQuestionFromFixedTitlesRoster(
            IQuestion question, Guid rosterId, Guid interviewId, Guid userId, IQuestionnaire questionnaire, State state)
        {
            int rosterInstancesCount = questionnaire.GetFixedRosterTitles(rosterId).Count();

            return CreateAnswerCommandsForQuestionFromRoster(question, rosterInstancesCount, interviewId, userId, questionnaire, state);
        }

        private static IEnumerable<ICommand> CreateAnswerCommandsForQuestionFromRoster(
            IQuestion question, int rosterInstancesCount, Guid interviewId, Guid userId, IQuestionnaire questionnaire, State state)
        {
            IEnumerable<decimal> rosterInstanceIds = Enumerable.Range(0, rosterInstancesCount).Select(rosterInstanceIndex => (decimal) rosterInstanceIndex);

            return CreateAnswerCommandsForQuestionFromRoster(question, rosterInstanceIds, interviewId, userId, questionnaire, state);
        }

        private static IEnumerable<ICommand> CreateAnswerCommandsForQuestionFromRoster(
            IQuestion question, IEnumerable<decimal> rosterInstanceIds, Guid interviewId, Guid userId, IQuestionnaire questionnaire, State state)
        {
            return (
                from rosterInstanceId in rosterInstanceIds
                let propagationVector = new[] { rosterInstanceId }
                select CreateAnswerCommand(question, interviewId, userId, propagationVector, questionnaire, state)
            ).ToList();
        }

        private static ICommand CreateAnswerCommand(IQuestion question, Guid interviewId, Guid userId, decimal[] propagationVector,
            IQuestionnaire questionnaire, State state)
        {
            Guid questionId = question.PublicKey;

            bool isNumericRosterSizeQuestion = question.QuestionType == QuestionType.Numeric && questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).Any();

            object answer;
            if (isNumericRosterSizeQuestion)
            {
                var numericQuestion = (INumericQuestion) question;
                int rosterCount = Math.Min(numericQuestion.MaxValue ?? MaxAllowedRosterCount, 7);

                foreach (Guid rosterId in questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId))
                {
                    state.NumericRosterInstanceCounts[rosterId] = rosterCount;
                }

                answer = rosterCount;
            }
            else
            {
                answer = GetAnswerByQuestion(question);
            }

            DateTime answersTime = DateTime.UtcNow;

            if (answer != null)
            {
                switch (question.QuestionType)
                {
                    case QuestionType.Text:
                        return new AnswerTextQuestionCommand(interviewId, userId, questionId, propagationVector, answersTime,
                            (string) answer);

                    case QuestionType.AutoPropagate:
                    case QuestionType.Numeric:
                        return CreateAnswerCommandForAnswerNumericQuestion(question, interviewId, userId, propagationVector, answersTime,
                            answer);

                    case QuestionType.DateTime:
                        return new AnswerDateTimeQuestionCommand(interviewId, userId, questionId, propagationVector, answersTime,
                            (DateTime) answer);

                    case QuestionType.SingleOption:
                        return new AnswerSingleOptionQuestionCommand(interviewId, userId, questionId, propagationVector, answersTime,
                            (decimal) answer);

                    case QuestionType.MultyOption:
                        return new AnswerMultipleOptionsQuestionCommand(interviewId, userId, questionId, propagationVector, answersTime,
                            (decimal[]) answer);
                }
            }

            return null;
        }

        private static ICommand CreateAnswerCommandForAnswerNumericQuestion(IQuestion question, Guid interviewId, Guid userId,
            decimal[] emptyPropagationVector,
            DateTime answersTime, object answer)
        {
            var isInteger = true;
            
            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
                isInteger = numericQuestion.IsInteger;

            if (isInteger)
                return new AnswerNumericIntegerQuestionCommand(interviewId, userId, question.PublicKey, emptyPropagationVector, answersTime,
                    Convert.ToInt32(answer));

                return new AnswerNumericRealQuestionCommand(interviewId, userId, question.PublicKey, emptyPropagationVector, answersTime,
                    (decimal) answer);
        }

        private QuestionnaireDocument ReadTemplate(string path, bool treateAsGzipped)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var template = JsonConvert.DeserializeObject<QuestionnaireDocument>(GetFileContent(path, treateAsGzipped), settings);
            template.PublicKey = Guid.NewGuid();
            return template;
        }

        private string GetFileContent(string path, bool treateAsGzipped)
        {
            string fileContent;
            if (!treateAsGzipped)
                fileContent = File.OpenText(path).ReadToEnd();
            else
            {
                using (Stream fileStream = File.OpenRead(path), zippedStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(zippedStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }

            return fileContent;
        }

        private void UpdateProgress()
        {
            Progress += 1;
        }

        private void Log(string message, string linkText = null, string link = null)
        {
            var logMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("G"), message);

            var logEntry = new LogMessage(logMessage);
            logEntry.Link = link;
            logEntry.LinkText = linkText;

            InvokeOnMainThread(() => LogMessages.Add(logEntry));
        }

        private void LogStatus(string message, int progress, int count)
        {
            var output = string.Format("[{0}] {1}: {2} of {3}", DateTime.Now.ToString("G"), message, progress + 1, count);
            InvokeOnMainThread(() =>  {
                if (LogMessages.Any(x => x.Message.Contains(message)))
                {
                    LogMessage logMessage = LogMessages[LogMessages.Count - 1];
                    logMessage.Message = output;
                }
                else
                {
                    LogMessages.Add(new LogMessage(output));
                }
            });
        }
    }
}
