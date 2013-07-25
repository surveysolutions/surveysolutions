using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Cirrious.MvvmCross.ViewModels;
using Core.Supervisor.Views.User;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Utility;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Backup;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace CapiDataGenerator
{
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

        readonly Random _rand = new Random();
        readonly Timer _timer = new Timer(1000);
        private DateTime _startTime;
        private UserLight _headquarterUser;

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

                    var questionsCount = template.GetAllQuestions<IQuestion>().Count(x => !x.Featured);

                    acount = (int) (questionsCount*((double) acount/100));
                    ccount = (int) (questionsCount*((double) ccount/100));
                    scount = (int) (icount*qcount*((double) scount/100));

                    TotalCount = icount + icount*qcount*(acount + ccount + scount + 1);

                    try
                    {
                        var users = CreateUsers(icount);
                        var questionnaries = CreateQuestionnaires(template, qcount, users);
                        CreateAnswers(template, acount, questionnaries);
                        CreateComments(template, ccount, questionnaries);
                        ChangeStatuses(scount, questionnaries);
                    }
                    catch (Exception e)
                    {
                        Log(e.Message);
                    }
                }

                Log("create backup");
                string backupPath = backupService.Backup();
                Log(string.Format("backup was saved to {0}", backupPath));

                Log("end");
                _timer.Stop();
                Progress = 0;
                CanGenerate = true;
                
            });
        }

        private void ChangeStatuses(int statusesCount, List<Guid> questionnaires)
        {
            for (int z = 0; z < statusesCount; z++)
            {
                var qId = questionnaires.ElementAt(_rand.Next(questionnaires.Count()));
                commandService.Execute(new ChangeStatusCommand()
                {
                    CompleteQuestionnaireId = qId,
                    Responsible = new UserLight(Guid.NewGuid(), string.Empty),
                    Status = _rand.Next(0, 1) == 0 ? SurveyStatus.Complete : SurveyStatus.Redo
                });

                UpdateProgress();
                LogStatus("change statuses", z, statusesCount);
            }
        }

        private void CreateComments(QuestionnaireDocument template, int commentsCount, List<Guid> questionnaires)
        {
            var questions = ((CompleteQuestionnaireDocument)template).GetQuestions().Where(x=>!x.Featured);
            for (int j = 0; j < questionnaires.Count; j++)
            {
                var qId = questionnaires[j];
                for (int z = 0; z < commentsCount; z++)
                {
                    var question = questions.ElementAt(_rand.Next(questions.Count()));
                    var isAutoQuestion = question is IAutoPropagate;
                    commandService.Execute(new SetCommentCommand(completeQuestionnaireId: qId,
                        questionPublicKey: question.PublicKey, 
                        propogationPublicKey: isAutoQuestion ? null : question.PropagationPublicKey,
                        questionComments: "auto comment",
                        user: new UserLight(Guid.NewGuid(), string.Empty)));

                    UpdateProgress();
                    LogStatus("set comments", (j*commentsCount) + z, questionnaires.Count*commentsCount);
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
                    email: string.Concat(userName, "@mail.com"), roles: new[] {UserRoles.User}, isLocked: false,
                    supervsor: new UserLight(SelectedSupervisor.UserId, SelectedSupervisor.UserName)));
                InvokeOnMainThread(() => InterviewersList.Add(userName));
                UpdateProgress();
                LogStatus("create users", i, usersCount);
            }

            return users;
        }

        private List<Guid> CreateQuestionnaires(QuestionnaireDocument template, int questionnariesCount, List<UserLight> users)
        {
            Log("import template");
            commandService.Execute(new ImportQuestionnaireCommand(_headquarterUser.Id, template));

            var completeDocument = (CompleteQuestionnaireDocument)template;

            var questionnaires = new List<Guid>();
            for (int i = 0; i < users.Count; i++)
            {
                var interviewer = users[i];
                for (int j = 0; j < questionnariesCount; j++)
                {
                    LogStatus("create questionnaires", (i * questionnariesCount) + j, questionnariesCount * users.Count);

                    completeDocument.Creator = new UserLight(SelectedSupervisor.UserId, SelectedSupervisor.UserName);
                    completeDocument.Status = SurveyStatus.Initial;
                    completeDocument.PublicKey = Guid.NewGuid();
                    completeDocument.Responsible = interviewer;

                    commandService.Execute(new CreateCompleteQuestionnaireCommand(completeDocument.PublicKey,
                        template.PublicKey, _headquarterUser));
                    commandService.Execute(new ChangeAssignmentCommand(completeDocument.PublicKey, completeDocument.Creator));
                    commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = completeDocument.PublicKey, Status = SurveyStatus.Unassign, Responsible = completeDocument.Creator });
                    commandService.Execute(new ChangeAssignmentCommand(completeDocument.PublicKey, interviewer));
                    commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = completeDocument.PublicKey, Status = SurveyStatus.Initial, Responsible = interviewer });

                    questionnaires.Add(completeDocument.PublicKey);

                    commandService.Execute(new CreateNewAssigment(completeDocument));

                    UpdateProgress();
                }
            }

            return questionnaires;
        }

        private void CreateAnswers(QuestionnaireDocument template, int answersCount, List<Guid> questionnaires)
        {
            var questions = ((CompleteQuestionnaireDocument)template).GetQuestions().Where(x=>!x.Featured);
            for (int j = 0; j < questionnaires.Count; j++)
            {
                var qId = questionnaires[j];
                for (int z = 0; z < answersCount; z++)
                {

                    var question = questions.ElementAt(_rand.Next(questions.Count()));
                    var isAutoQuestion = question is IAutoPropagate;
                    commandService.Execute(new SetAnswerCommand(completeQuestionnaireId: qId,
                        questionPublicKey: question.PublicKey, answersList: GetDummyCompleteAnswers(question),
                        answerValue: GetDummyAnswer(question),
                        propogationPublicKey: isAutoQuestion ? null : question.PropagationPublicKey));

                    UpdateProgress();
                    LogStatus("answer questions", (j*answersCount) + z, questionnaires.Count*answersCount);
                }
            }
        }

        private string GetDummyAnswer(IQuestion q)
        {
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
                return (new DateTime(_rand.Next(1940, 2003), _rand.Next(1, 13), _rand.Next(1, 29))).ToString(CultureInfo.InvariantCulture);
            }
            if (q is INumericQuestion)
            {
                return _rand.Next(100).ToString(CultureInfo.InvariantCulture);
            }
            if (q is IAutoPropagate)
            {
                var question = (AutoPropagateCompleteQuestion)q;
                var maxValue = question.MaxValue;
                return _rand.Next(maxValue).ToString(CultureInfo.InvariantCulture);
            }
            if (q is ITextCompleteQuestion)
            {
                return "value " + _rand.Next();
            }
            return string.Empty;
        }

        private List<Guid> GetDummyCompleteAnswers(IQuestion q)
        {
            if (q is IMultyOptionsQuestion)
            {
                var question = (MultyOptionsCompleteQuestion)q;
                var answersCount = question.Answers.Count;
                var selectedAnswersCount = _rand.Next(1, answersCount + 1);
                var result = new List<Guid>();
                for (int i = 0; i < selectedAnswersCount; i++)
                {
                    var answer = question.Answers[_rand.Next(0, answersCount)];
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
                        question.Answers[_rand.Next(answersCount)].PublicKey
                    };
            }
            return new List<Guid>() { Guid.NewGuid() };
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
