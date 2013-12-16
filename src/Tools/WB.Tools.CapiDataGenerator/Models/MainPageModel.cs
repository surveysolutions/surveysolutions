using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CAPI.Android.Core.Model;
using Cirrious.MvvmCross.ViewModels;
using Core.Supervisor;
using Core.Supervisor.Views.User;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tools.CapiDataGenerator;
using WB.UI.Shared.Web;

namespace CapiDataGenerator
{
    using WB.Core.Infrastructure.Raven.Implementation.ReadSide;

    public class MainPageModel : MvxViewModel
    {
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

        readonly Random _rand = new Random();
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

            this.SupervisorList =
                new ObservableCollection<UserListItem>(
                    userListViewFactory.Load(new UserListViewInputModel() {Role = UserRoles.Supervisor}).Items);
            if (this.SupervisorList.Count == 0)
            {
                var emptySupervisor = new UserListItem(Guid.Empty, "Please, create new supervisor", null, DateTime.Now,
                    false, null);
                this.SupervisorList = new ObservableCollection<UserListItem>() {emptySupervisor};
                this.SelectedSupervisor = emptySupervisor;
            }
            var hq = userListViewFactory.Load(new UserListViewInputModel() {Role = UserRoles.Headquarter})
                .Items.FirstOrDefault();
            if (hq == null)
            {
                this.HeadquarterName = "Please, create new headquarter";
            }
            else
            {
                this._headquarterUser = new UserLight(hq.UserId, hq.UserName);
                this.HeadquarterName = this._headquarterUser.Name;
            }

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

        private bool _onlyForSupervisor = false;
        public bool OnlyForSupervisor
        {
            get
            {
                return _onlyForSupervisor;
            }
            set
            {
                _onlyForSupervisor = value;
                RaisePropertyChanged(() => OnlyForSupervisor);
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

        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();
        public ObservableCollection<string> LogMessages
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
                this._headquarterUser == null)
                return;

            LogMessages.Clear();

            Task.Run(() =>
            {
                CanGenerate = false;
                Log("start");

                _startTime = DateTime.Now;
                _timer.Start();

                QuestionnaireDocument template = null;

                try
                {
                    template = ReadTemplate(this.TemplatePath);
                }
                catch (Exception)
                {
                    
                    Log("bad template");
                }

                if (template != null)
                {
                    int qcount = 0;
                    int icount = 0;
                    int acount = 0;
                    int ccount = 0;
                    int scount = 0;
                    int.TryParse(QuestionnairesCount, out qcount);
                    int.TryParse(InterviewersCount, out icount);
                    int.TryParse(AnswersCount, out acount);
                    int.TryParse(CommentsCount, out ccount);
                    int.TryParse(StatusesCount, out scount);

                    if (scount > 100)
                    {
                        scount = 100;
                        StatusesCount = scount.ToString();
                    }

                    var onlyForSupervisor = this.OnlyForSupervisor;

                    var questions = template.GetAllQuestions().Where(x => !x.Featured);
                    var questionsCount = questions.Count();

                    acount = (int) (questionsCount*((double) acount/100));
                    ccount = (int) (questionsCount*((double) ccount/100));
                    scount = (int) (icount*qcount*((double) scount/100));

                    TotalCount = icount + icount*qcount*(acount + ccount + 2) + scount;

                    this.EnableCacheInAllRepositoryWriters();
                    try
                    {
                        AppSettings.Instance.AreSupervisorEventsNowPublishing = true;
                        var users = CreateUsers(icount);
                        var questionnaries = this.CreateInterviews(template, qcount, users, onlyForSupervisor);
                        CreateAnswers(acount, questionnaries, questions);
                        CreateComments(ccount, questionnaries, questions);
                        ChangeStatuses(scount, questionnaries, onlyForSupervisor);

                        if (!onlyForSupervisor)
                        {
                            Log("create backup");
                            string backupPath = backupService.Backup();
                            Log(string.Format("backup was saved to {0}", backupPath));
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

        private void ChangeStatuses(int statusesCount, Dictionary<Guid, Guid> interviews, bool onlyForSupervisor)
        {
            for (int z = 0; z < statusesCount; z++)
            {
                var interview = interviews.ElementAt(z);
                commandService.Execute(new CompleteInterviewCommand(interview.Key, interview.Value, "auto complete comment"));

                changeLogManipulator.CloseDraftRecord(interview.Key);

                if (onlyForSupervisor)
                {
                    commandService.Execute(new ApproveInterviewCommand(interview.Key, interview.Value, "auto approve comment"));
                }

                UpdateProgress();
                LogStatus("set complete status", z, statusesCount);
            }
        }

        private void CreateComments(int commentsCount, Dictionary<Guid, Guid> interviews, IEnumerable<IQuestion> questions)
        {
            for (int j = 0; j < interviews.Count; j++)
            {
                var interview = interviews.ElementAt(j);
                for (int z = 0; z < commentsCount; z++)
                {
                    var question = questions.ElementAt(_rand.Next(questions.Count()));

                    commandService.Execute(new CommentAnswerCommand(interviewId: interview.Key,
                                                                    userId: interview.Value,
                                                                    questionId: question.PublicKey,
                                                                    rosterVector: new decimal[0],
                                                                    commentTime: DateTime.UtcNow,
                                                                    comment: "auto comment"));

                    UpdateProgress();
                    LogStatus("set comments", (j*commentsCount) + z, interviews.Count*commentsCount);
                }
            }
        }


        private List<UserLight> CreateUsers(int usersCount)
        {
            var users = new List<UserLight>();
            for (int i = 0; i < usersCount; i++)
            {
                var uId = Guid.NewGuid();
                var userName = string.Format("interviewer_{0}_{1}", i, DateTime.Now.Ticks);
                users.Add(new UserLight(uId, userName));
                commandService.Execute(new CreateUserCommand(publicKey: uId, userName: userName,
                    password: SimpleHash.ComputeHash(userName),
                    email: string.Concat(userName, "@mail.com"), roles: new[] {UserRoles.Operator}, isLocked: false,
                    supervsor: new UserLight(SelectedSupervisor.UserId, SelectedSupervisor.UserName)));
                InvokeOnMainThread(() => InterviewersList.Add(userName));
                UpdateProgress();
                LogStatus("create users", i, usersCount);
            }

            return users;
        }

        private Dictionary<Guid, Guid> CreateInterviews(IQuestionnaireDocument template, int questionnariesCount, List<UserLight> users, bool onlyForSupervisor)
        {
            Log("import template");
            commandService.Execute(new ImportFromDesigner(_headquarterUser.Id, template));

            var featuredQuestions = template.GetFeaturedQuestions();

            var interviews = new Dictionary<Guid, Guid>();
            for (int i = 0; i < users.Count; i++)
            {
                var interviewer = users[i];
                for (int j = 0; j < questionnariesCount; j++)
                {
                    LogStatus("create interviews", (i * questionnariesCount) + j, questionnariesCount * users.Count);

                    Guid interviewId = Guid.NewGuid();

                    commandService.Execute(new CreateInterviewCommand(interviewId: interviewId,
                        questionnaireId: template.PublicKey,
                        supervisorId: SelectedSupervisor.UserId,
                        userId: interviewer.Id,
                        answersTime: DateTime.UtcNow,
                        answersToFeaturedQuestions: this.GetAnswersByFeaturedQuestions(featuredQuestions)));

                    commandService.Execute(new AssignInterviewerCommand(interviewId: interviewId, userId: SelectedSupervisor.UserId, interviewerId: interviewer.Id));
                    
                    interviews.Add(interviewId, interviewer.Id);

                    UpdateProgress();
                }
            }
            AppSettings.Instance.AreSupervisorEventsNowPublishing = onlyForSupervisor;
            for (int i = 0; i < interviews.Count; i++)
            {
                LogStatus("synchronize interview", i, interviews.Count);

                var interview = interviews.ElementAt(i);
                commandService.Execute(new SynchronizeInterviewCommand(interviewId: interview.Key, userId: interview.Value,
                                                                       sycnhronizedInterview: new InterviewSynchronizationDto(
                                                                           id: interview.Key,
                                                                           status: InterviewStatus.InterviewerAssigned,
                                                                           userId: interview.Value,
                                                                           questionnaireId: template.PublicKey,
                                                                           questionnaireVersion: 1,
                                                                           answers: new AnsweredQuestionSynchronizationDto[0], 
                                                                           disabledGroups: new HashSet<InterviewItemId>(), 
                                                                           disabledQuestions: new HashSet<InterviewItemId>(), 
                                                                           validAnsweredQuestions: new HashSet<InterviewItemId>(), 
                                                                           invalidAnsweredQuestions: new HashSet<InterviewItemId>(),
                                                                           propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, List<decimal>>(),
                                                                           wasCompleted: false)));
                changeLogManipulator.CreateOrReopenDraftRecord(interview.Key);
                UpdateProgress();
            }

            return interviews;
        }

        private Dictionary<Guid, object> GetAnswersByFeaturedQuestions(IEnumerable<IQuestion> featuredQuestions)
        {
            return featuredQuestions.ToDictionary(featuredQuestion => featuredQuestion.PublicKey, this.GetAnswerByQuestion);
        }

        private object GetAnswerByQuestion(IQuestion question)
        {
            object answer = null;
            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    if (question.Answers.Count > 0)
                    {
                        try
                        {
                            answer = decimal.Parse(question.Answers[_rand.Next(0, question.Answers.Count - 1)].AnswerValue,
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
                            catch{}
                        }
                        answer = answers.Distinct().ToArray();
                    }
                    break;
                case QuestionType.Numeric:
                    answer = new decimal(_rand.Next(100));
                    break;
                case QuestionType.DateTime:
                    answer = new DateTime(_rand.Next(1940, 2003), _rand.Next(1, 13), _rand.Next(1, 29));
                    break;
                case QuestionType.Text:
                    answer = "value " + _rand.Next();
                    break;
                case QuestionType.AutoPropagate:
                    return new decimal(_rand.Next(((IAutoPropagate) question).MaxValue));
                    break;
            }
            return answer;
        }

        private void CreateAnswers(int answersCount, Dictionary<Guid, Guid> interviews, IEnumerable<IQuestion> questions)
        {
            for (var j = 0; j < interviews.Count; j++)
            {
                var interview = interviews.ElementAt(j);
                for (int z = 0; z < answersCount; z++)
                {
                    var command = this.SendAnswerCommand(question: questions.ElementAt(_rand.Next(questions.Count())),
                                                         responsibleId: interview.Value, interviewId: interview.Key);
                    if (command != null)
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

        private ICommand SendAnswerCommand(IQuestion question, Guid interviewId, Guid responsibleId)
        {
            ICommand command = null;

            decimal[] emptyPropagationVector = { };
            Guid questionId = question.PublicKey;
            Guid userId = responsibleId;
            DateTime answersTime = DateTime.UtcNow;

            object answer = this.GetAnswerByQuestion(question);

            if (answer != null)
            {
                switch (question.QuestionType)
                {
                    case QuestionType.Text:
                        command = new AnswerTextQuestionCommand(interviewId, userId, questionId, emptyPropagationVector, answersTime,
                                                                (string) answer);
                        break;

                    case QuestionType.AutoPropagate:
                    case QuestionType.Numeric:
                        command = this.CreateAnswerCommandForAnswerNumericQuestion(question, interviewId, userId, emptyPropagationVector, answersTime, answer);
                        break;

                    case QuestionType.DateTime:
                        command = new AnswerDateTimeQuestionCommand(interviewId, userId, questionId, emptyPropagationVector, answersTime,
                                                                    (DateTime) answer);
                        break;

                    case QuestionType.SingleOption:
                        command = new AnswerSingleOptionQuestionCommand(interviewId, userId, questionId, emptyPropagationVector, answersTime,
                                                                        (decimal) answer);
                        break;

                    case QuestionType.MultyOption:
                        command = new AnswerMultipleOptionsQuestionCommand(interviewId, userId, questionId, emptyPropagationVector,
                                                                           answersTime, (decimal[]) answer);
                        break;
                }
            }

            return command;
        }

        private ICommand CreateAnswerCommandForAnswerNumericQuestion(IQuestion question, Guid interviewId, Guid userId,
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

        private QuestionnaireDocument ReadTemplate(string path)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var template = JsonConvert.DeserializeObject<QuestionnaireDocument>(File.OpenText(path).ReadToEnd(), settings);
            template.PublicKey = Guid.NewGuid();
            return template;
        }

        private void UpdateProgress()
        {
            Progress += 1;
        }

        private void Log(string message)
        {
            InvokeOnMainThread(() => LogMessages.Add(message));
        }

        private void LogStatus(string message, int progress, int count)
        {
            var output = string.Format("{0}: {1} of {2}", message, progress + 1, count);
            InvokeOnMainThread(
                () =>
                {
                    if (LogMessages.Any(x => x.Contains(message)))
                    {
                        LogMessages[LogMessages.Count - 1] = output;
                    }
                    else
                    {
                        LogMessages.Add(output);
                    }
                });
        }
    }
}
