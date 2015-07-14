using System;
using System.Collections.ObjectModel;
using System.Linq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
{
    public class TextListQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteraction;
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
                return newListItem;
            }
            set
            {
                newListItem = value;
                this.RaisePropertyChanged();
            }
        }

        private IMvxCommand valueChangeCommand;

        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(AddNewItemCommand)); }
        }

        private void AddNewItemCommand()
        {
            if (!string.IsNullOrWhiteSpace(NewListItem))
            {
                this.AddNewItemAndSaveAnswers(NewListItem.Trim());
            }
        }

        public TextListQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            IUserInteraction userInteraction,
            AnsweringViewModel answering)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteraction = userInteraction;
            this.Answering = answering;
            this.Answers = new ObservableCollection<TextListItemViewModel>();
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetTextListAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetTextListQuestion(entityIdentity.Id);

            if (answerModel.IsAnswered)
            {
                answerModel.Answers.Select(x => this.CreateListItemViewModel(x.Item1, x.Item2)).ForEach(x => this.Answers.Add(x));
            }

            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
            this.maxAnswerCount = questionModel.MaxAnswerCount;

            this.IsAddNewItemVisible = !this.maxAnswerCount.HasValue || this.Answers.Count < this.maxAnswerCount.Value;
        }

        private async void ListItemDeleted(object sender, EventArgs eventArgs)
        {
            var listItem = sender as TextListItemViewModel;

            if (listItem == null)
                return;

            if (this.isRosterSizeQuestion )
            {
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, 1);
                if (!(await this.userInteraction.ConfirmAsync(message)))
                {
                    return;
                }
            }

            Answers.Remove(listItem);

            SaveAnswers();
        }

        private void ListItemEdited(object sender, EventArgs eventArgs)
        {
            SaveAnswers();
        }

        private void AddNewItemAndSaveAnswers(string title)
        {
            var maxValue = Answers.Count == 0 ? -1 : Answers.Max(x => x.Value) + 1;

            Answers.Add(this.CreateListItemViewModel(maxValue, title));

            SaveAnswers();
            
            NewListItem = string.Empty;
        }

        private async void SaveAnswers()
        {
            if (maxAnswerCount.HasValue)
            {
                IsAddNewItemVisible = Answers.Count < maxAnswerCount.Value;
            }

            var answers = this.Answers.Select(x => new Tuple<decimal, string>(x.Value, x.Title)).ToArray();

            var command = new AnswerTextListQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answers: answers);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private TextListItemViewModel CreateListItemViewModel(decimal value, string title)
        {
            var optionViewModel = new TextListItemViewModel
            {
                Enablement = QuestionState.Enablement,

                Value = value,
                Title = title
            };

            optionViewModel.ItemEdited += ListItemEdited;
            optionViewModel.ItemDeleted += ListItemDeleted;

            return optionViewModel;
        }
    }
}