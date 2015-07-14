using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
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
                return this.OriginalText;
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

            var parentAnswerModel = interview.GetSingleOptionAnswer(this.parentQuestionIdentity);
            if (parentAnswerModel.IsAnswered)
            {
                this.answerOnParentQuestion = parentAnswerModel.Answer;
            }

            this.Options = questionModel.Options.ToList();

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                this.selectedObject = this.CreateFormattedOptionModel(this.Options.SingleOrDefault(i => i.Value == selectedValue));
                this.ResetTextInEditor = this.selectedObject.OriginalText;
                this.FilterText = this.selectedObject.OriginalText;
            }
            else
            {
                this.FilterText = null;
            }

            this.eventRegistry.Subscribe(this, interviewId);
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
                    this.SelectedObject = null;
                    this.FilterText = null;
                }
                this.RaisePropertyChanged(); 
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get
            {
                return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand<string>(async enteredText => await this.FindMatchOptionAndSendAnswerQuestionCommand(enteredText)));
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
                    Action sendCommand = async () => await this.SendAnswerFilteredComboboxQuestionCommand(this.selectedObject.Value);
                    sendCommand();
                }
                this.RaisePropertyChanged();
            }
        }

        private bool isInitialized = false;
        private string filterText;
        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (value == this.filterText && this.isInitialized)
                {
                    return;
                }

                this.filterText = value;

                this.UpdateSuggestionsList(this.filterText);

                this.isInitialized = true;
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
            if (!this.answerOnParentQuestion.HasValue) 
                yield break;

            if (textHint.IsNullOrEmpty())
            {
                var options = this.Options.Where(x => x.ParentValue == this.answerOnParentQuestion.Value)
                                          .Select(x => this.CreateFormattedOptionModel(x));
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
                    yield return this.CreateFormattedOptionModel(model, textHint);
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
                       Selected = this.SelectedObject != null && model.Value == this.SelectedObject.Value
                   };
        }

        private void SetSuggestionsEmpty()
        {
            this.AutoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();
        }

        private List<CascadingComboboxItemViewModel> autoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();


        public List<CascadingComboboxItemViewModel> AutoCompleteSuggestions
        {
            get { return this.autoCompleteSuggestions; }
            set { this.autoCompleteSuggestions = value; this.RaisePropertyChanged(); }
        }

        private async Task FindMatchOptionAndSendAnswerQuestionCommand(string enteredText)
        {
            var answerViewModel = this.Options.SingleOrDefault(i => i.Title == enteredText);

            if (answerViewModel == null || answerViewModel.ParentValue != this.answerOnParentQuestion)
            {
                if (this.selectedObject != null)
                {
                    this.ResetTextInEditor = this.selectedObject.OriginalText;
                }
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(string.Format(UIResources.Interview_Question_Cascading_NoMatchingValue, enteredText));
                return;
            }

            var answerValue = answerViewModel.Value;
            if (this.selectedObject != null && answerValue == this.selectedObject.Value)
            {
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }

            await this.SendAnswerFilteredComboboxQuestionCommand(answerValue);
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
            if (@event.QuestionId == this.parentQuestionIdentity.Id
                && @event.PropagationVector.SequenceEqual(this.parentQuestionIdentity.RosterVector))
            {
                var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
                var parentAnswerModel = interview.GetSingleOptionAnswer(this.parentQuestionIdentity);
                if (parentAnswerModel.IsAnswered)
                {
                    this.answerOnParentQuestion = parentAnswerModel.Answer;
                    this.UpdateSuggestionsList(string.Empty);
                }              
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == this.questionIdentity.Id && question.RosterVector.Identical(this.questionIdentity.RosterVector))
                {
                    this.ResetTextInEditor = null;
                    this.QuestionState.IsAnswered = false;
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                }
            }
        }
    }
}