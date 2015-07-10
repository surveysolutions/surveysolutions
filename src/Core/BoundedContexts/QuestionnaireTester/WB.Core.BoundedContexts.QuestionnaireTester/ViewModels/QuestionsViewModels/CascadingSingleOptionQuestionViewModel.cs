using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            public bool Selected { get; set; }

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
        private List<CascadingOptionModel> Options { get; set; }
        private readonly ILiteEventRegistry eventRegistry;

        public Identity Identities { get { return this.questionIdentity; } }

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

            var parentRosterVector = entityIdentity.RosterVector.Take(questionModel.RosterLevelDepthOfParentQuestion).ToArray();
            this.parentQuestionIdentity = new Identity(questionModel.CascadeFromQuestionId, parentRosterVector);

            var parentAnswerModel = interview.GetSingleOptionAnswer(parentQuestionIdentity);
            if (parentAnswerModel.IsAnswered)
            {
                answerOnParentQuestion = parentAnswerModel.Answer;
            }

            this.Options = questionModel.Options.ToList();

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                selectedObject = CreateFormattedOptionModel(Options.SingleOrDefault(i => i.Value == selectedValue));
                ResetTextInEditor = selectedObject.OriginalText;
                FilterText = selectedObject.OriginalText;
            }
            else
            {
                FilterText = null;
            }

            this.eventRegistry.Subscribe(this);
        }

        private string resetTextInEditor;
        public string ResetTextInEditor
        {
            get { return this.resetTextInEditor; }
            set 
            { 
                this.resetTextInEditor = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    SelectedObject = null;
                    FilterText = null;
                }
                this.RaisePropertyChanged(); 
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get
            {
                return valueChangeCommand ?? (valueChangeCommand = new MvxCommand<string>(async enteredText => await this.FindMatchOptionAndSendAnswerQuestionCommand(enteredText)));
            }
        }

        private CascadingComboboxItemViewModel selectedObject;
        public CascadingComboboxItemViewModel SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                if (this.selectedObject == value)
                    return;
                
                this.selectedObject = value;
                if (this.selectedObject != null)
                {
                    Action sendCommand = async () => await SendAnswerFilteredComboboxQuestionCommand(this.selectedObject.Value);
                    sendCommand();
                }
                RaisePropertyChanged();
            }
        }

        private bool isInitialized = false;
        private string filterText;
        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (value == this.filterText && isInitialized)
                {
                    return;
                }

                this.filterText = value;

                this.UpdateSuggestionsList(this.filterText);

                isInitialized = true;
            }
        }

        private void UpdateSuggestionsList(string textHint)
        {
            var list = this.GetSuggestionsList(textHint).ToList();

            if (list.Any())
            {
                this.AutoCompleteSuggestions = list;
            }
            else
            {
                this.SetSuggestionsEmpty();
            }
        }

        private IEnumerable<CascadingComboboxItemViewModel> GetSuggestionsList(string textHint)
        {
            if (!answerOnParentQuestion.HasValue) 
                yield break;

            if (textHint.IsNullOrEmpty())
            {
                var options = this.Options.Where(x => x.ParentValue == this.answerOnParentQuestion.Value)
                                          .Select(x => CreateFormattedOptionModel(x));
                foreach (var option in options)
                {
                    yield return option;
                }
                yield break;
            }

            foreach (CascadingOptionModel model in this.Options.Where(x => x.ParentValue == this.answerOnParentQuestion.Value))
            {
                var index = model.Title.IndexOf(textHint, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                {
                    yield return CreateFormattedOptionModel(model, textHint);
                }
            }
        }

        private CascadingComboboxItemViewModel CreateFormattedOptionModel(CascadingOptionModel model, string hint = null)
        {
            var text = !string.IsNullOrEmpty(hint) ? 
                Regex.Replace(model.Title, hint, "<b>" + hint + "</b>", RegexOptions.IgnoreCase) :
                model.Title;

            return new CascadingComboboxItemViewModel
                   {
                       Text = text,
                       OriginalText = model.Title,
                       Value = model.Value,
                       ParentValue = model.ParentValue,
                       Selected = SelectedObject != null && model.Value == SelectedObject.Value
                   };
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

        private async Task FindMatchOptionAndSendAnswerQuestionCommand(string enteredText)
        {
            var answerViewModel = this.Options.SingleOrDefault(i => i.Title == enteredText);

            if (answerViewModel == null || answerViewModel.ParentValue != answerOnParentQuestion)
            {
                if (this.selectedObject != null)
                {
                    ResetTextInEditor = this.selectedObject.OriginalText;
                }
                this.QuestionState.Validity.MarkAnswerAsInvalidWithMessage(string.Format(UIResources.Interview_Question_Cascading_NoMatchingValue, enteredText));
                return;
            }

            var answerValue = answerViewModel.Value;
            if (selectedObject != null && answerValue == selectedObject.Value)
            {
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }

            await SendAnswerFilteredComboboxQuestionCommand(answerValue);
        }

        private async Task SendAnswerFilteredComboboxQuestionCommand(decimal answerValue)
        {
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
                    UpdateSuggestionsList(string.Empty);
                }              
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == questionIdentity.Id && question.RosterVector.Identical(questionIdentity.RosterVector))
                {
                    ResetTextInEditor = null;
                    this.QuestionState.IsAnswered = false;
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                }
            }
        }
    }
}