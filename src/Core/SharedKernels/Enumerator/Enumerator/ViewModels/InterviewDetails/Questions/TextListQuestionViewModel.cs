using System;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel, IDisposable
    {
        private readonly IPrincipal principal;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<TextListQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;

        private int? maxAnswerCount;

        public ObservableCollection<TextListItemViewModel> Answers { get; set; }

        public bool IsAddNewItemVisible
        {
            get { return this.isAddNewItemVisible; }
            set { this.isAddNewItemVisible = value; this.RaisePropertyChanged(); }
        }

        private string newListItem = string.Empty;

        private bool isAddNewItemVisible;

        public string NewListItem
        {
            get
            {
                return this.newListItem;
            }
            set
            {
                this.newListItem = value;
                this.RaisePropertyChanged();
            }
        }

        private IMvxCommand valueChangeCommand;

        public IMvxCommand ValueChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(this.AddNewItemCommand)); }
        }

        private void AddNewItemCommand()
        {
            if (!string.IsNullOrWhiteSpace(this.NewListItem))
            {
                this.AddNewItemAndSaveAnswers(this.NewListItem.Trim());
            }
        }

        public TextListQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            IUserInteractionService userInteractionService,
            AnsweringViewModel answering)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteractionService = userInteractionService;
            this.Answering = answering;
            this.Answers = new ObservableCollection<TextListItemViewModel>();
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetTextListAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (answerModel.IsAnswered)
            {
                answerModel.Answers.Select(x => this.CreateListItemViewModel(x.Item1, x.Item2)).ForEach(x => this.Answers.Add(x));
            }

            this.isRosterSizeQuestion = questionnaire.ShouldQuestionSpecifyRosterSize(entityIdentity.Id);
            this.maxAnswerCount = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);

            this.IsAddNewItemVisible = this.IsNeedShowAddNewItem();
        }

        private async void ListItemDeleted(object sender, EventArgs eventArgs)
        {
            var listItem = sender as TextListItemViewModel;

            if (listItem == null)
                return;

            if (this.isRosterSizeQuestion )
            {
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, 1);
                if (!(await this.userInteractionService.ConfirmAsync(message)))
                {
                    return;
                }
            }

            this.Answers.Remove(listItem);

            this.SaveAnswers();
        }

        private void ListItemEdited(object sender, EventArgs eventArgs)
        {
            this.SaveAnswers();
        }

        private void AddNewItemAndSaveAnswers(string title)
        {
            var maxValue = this.Answers.Count == 0 ? 1 : this.Answers.Max(x => x.Value) + 1;

            this.Answers.Add(this.CreateListItemViewModel(maxValue, title));

            this.SaveAnswers();
            
            this.NewListItem = string.Empty;
        }

        private async void SaveAnswers()
        {
            this.IsAddNewItemVisible = this.IsNeedShowAddNewItem();

            if (this.Answers.Any(x => string.IsNullOrWhiteSpace(x.Title)))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_List_Empty_Values_Are_Not_Allowed);
                return;
            }

            var answers = this.Answers.Select(x => new Tuple<decimal, string>(x.Value, x.Title)).ToArray();

            var command = new AnswerTextListQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answers: answers);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private bool IsNeedShowAddNewItem()
        {
            if (this.maxAnswerCount.HasValue)
            {
                var isInvalidMaxAnswerCountRule = this.Answers.Count >= this.maxAnswerCount.Value;
                return !isInvalidMaxAnswerCountRule;
            }

            var isInvalidRosterSizeRule = this.isRosterSizeQuestion && this.Answers.Count >= Constants.MaxRosterRowCount;
            return !isInvalidRosterSizeRule;
        }

        private TextListItemViewModel CreateListItemViewModel(decimal value, string title)
        {
            var optionViewModel = new TextListItemViewModel
            {
                Enablement = this.QuestionState.Enablement,

                Value = value,
                Title = title
            };

            optionViewModel.ItemEdited += this.ListItemEdited;
            optionViewModel.ItemDeleted += this.ListItemDeleted;

            return optionViewModel;
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
        }
    }
}