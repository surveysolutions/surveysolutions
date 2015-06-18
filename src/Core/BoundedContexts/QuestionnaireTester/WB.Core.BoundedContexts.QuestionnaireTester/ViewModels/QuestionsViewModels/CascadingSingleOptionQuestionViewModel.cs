using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class CascadingSingleOptionQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
         ILiteEventHandler<SingleOptionQuestionAnswered>,
         ILiteEventHandler<AnswersRemoved>
    {
        public class CascadingComboboxItemViewModel 
        {
            public string Text { get; set; }
            public string OriginalText { get; set; }
            public decimal Value { get; set; }
            public decimal ParentValue { get; set; }

            public override string ToString()
            {
                return OriginalText;
            }
        }

        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private Identity questionIdentity;
        private Identity parentQuestionIdentity;
        private Guid interviewId;
        private decimal? answerOnParentQuestion = null;

        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        public IList<CascadingComboboxItemViewModel> Options { get; set; }
        private readonly ILiteEventRegistry eventRegistry;

        public CascadingSingleOptionQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering, 
            ILiteEventRegistry eventRegistry)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.eventRegistry = eventRegistry;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetQuestion<CascadingSingleOptionQuestionModel>(entityIdentity.Id);
            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            var parentRosterVector = entityIdentity.RosterVector.Take(questionModel.RosterLevelDeepOfParentQuestion).ToArray();
            this.parentQuestionIdentity = new Identity(questionModel.CascadeFromQuestionId, parentRosterVector);

            var parentAnswerModel = interview.GetSingleOptionAnswer(parentQuestionIdentity);
            if (parentAnswerModel.IsAnswered)
            {
                answerOnParentQuestion = parentAnswerModel.Answer;
            }

            this.Options = questionModel
                .Options
                .Select(this.ToViewModel)
                .ToList();

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                SelectedObject = Options.SingleOrDefault(i => i.Value == selectedValue);
            }

            this.eventRegistry.Subscribe(this);
        }

        private CascadingComboboxItemViewModel ToViewModel(CascadingOptionModel model)
        {
            var optionViewModel = new CascadingComboboxItemViewModel
            {
                Text = model.Title,
                OriginalText = model.Title,
                Value = model.Value,
                ParentValue = model.ParentValue
            };

            return optionViewModel;
        }

        private string answer;
        public string Answer
        {
            get { return this.answer; }
            set
            {
                if (answer != value)
                {
                    this.answer = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerFilteredComboboxQuestionCommand)); }
        }

        private CascadingComboboxItemViewModel selectedObject;
        public CascadingComboboxItemViewModel SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                this.selectedObject = value;
                RaisePropertyChanged();
            }
        }

        private string filterText;
        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    this.filterText = null;
                    SetSuggestionsEmpty();
                    return;
                }
                    
                this.filterText = value;

                var list = this.GetSuggestionsList(this.filterText).ToList();

                if (list.Any())
                {
                    AutoCompleteSuggestions = list;
                }
                else
                {
                    SetSuggestionsEmpty();
                }
            }
        }

        private IEnumerable<CascadingComboboxItemViewModel> GetSuggestionsList(string textHint)
        {
            if (!answerOnParentQuestion.HasValue) 
                yield break;

            var upperTextHint = textHint.ToUpper();

            foreach (var model in Options.Where(x => x.ParentValue == answerOnParentQuestion.Value))
            {
                if (model.Text.IsNullOrEmpty())
                    continue;

                string upperText = model.Text.ToUpper();

                var index = upperText.IndexOf(upperTextHint, StringComparison.CurrentCulture);
                if (index >= 0)
                {
                    yield return new CascadingComboboxItemViewModel
                    {
                        Text = model.Text.Insert(index + textHint.Length, "</b>").Insert(index, "<b>"),
                        OriginalText = model.Text,
                        Value = model.Value,
                        ParentValue = model.ParentValue
                    };
                }
            }
        }

        private void SetSuggestionsEmpty()
        {
            AutoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();
        }

        private List<CascadingComboboxItemViewModel> autoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();

        public List<CascadingComboboxItemViewModel> AutoCompleteSuggestions
        {
            get { return this.autoCompleteSuggestions; }
            set { this.autoCompleteSuggestions = value; RaisePropertyChanged(); }
        }

        private async void SendAnswerFilteredComboboxQuestionCommand()
        {
            var answerViewModel = this.Options.SingleOrDefault(i => i.Text == this.Answer);

            if (answerViewModel == null)
            {
                this.QuestionState.Validity.MarkAnswerAsInvalidWithMessage(UIResources.Interview_Question_Text_MaskError);
                return;
            }

            var answerValue = answerViewModel.Value;

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.principal.CurrentUserIdentity.UserId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                answerValue);

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

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            if (@event.QuestionId == parentQuestionIdentity.Id
                && @event.PropagationVector.SequenceEqual(parentQuestionIdentity.RosterVector))
            {
                var interview = this.interviewRepository.Get(interviewId.FormatGuid());
                var parentAnswerModel = interview.GetSingleOptionAnswer(parentQuestionIdentity);
                if (parentAnswerModel.IsAnswered)
                {
                    answerOnParentQuestion = parentAnswerModel.Answer;
                }              
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == questionIdentity.Id && question.RosterVector.SequenceEqual(questionIdentity.RosterVector))
                {
                    SelectedObject = null;
                }
            }
        }
    }
}