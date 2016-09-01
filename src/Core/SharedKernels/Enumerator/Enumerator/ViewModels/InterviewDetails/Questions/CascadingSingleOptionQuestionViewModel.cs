using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : MvxNotifyPropertyChanged, 
         IInterviewEntityViewModel,
         ILiteEventHandler<SingleOptionQuestionAnswered>,
         ILiteEventHandler<AnswersRemoved>,
         ILiteEventHandler<AnswerRemoved>,
         IDisposable
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
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected IStatefulInterview interview;

        private Identity questionIdentity;
        private Identity parentQuestionIdentity;
        private Guid interviewId;
        private decimal? answerOnParentQuestion = null;

        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        private readonly ILiteEventRegistry eventRegistry;

        public Identity Identities => this.questionIdentity;

        private const int SuggestionsMaxCount = 15;

        public CascadingSingleOptionQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering, 
            ILiteEventRegistry eventRegistry)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.eventRegistry = eventRegistry;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
            if (!cascadingQuestionParentId.HasValue) throw new ArgumentNullException("parent of cascading question is missing");

            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;
            var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

            this.parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

            var parentAnswerModel = interview.GetSingleOptionAnswer(this.parentQuestionIdentity);
            if (parentAnswerModel.IsAnswered)
            {
                this.answerOnParentQuestion = parentAnswerModel.Answer;
            }

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                var answerOption = this.interview.GetOptionForQuestionWithoutFilter(this.questionIdentity, (int)selectedValue.Value, (int?)parentAnswerModel.Answer);
                this.selectedObject = this.CreateFormattedOptionModel(answerOption);
                this.ResetTextInEditor = this.selectedObject.OriginalText;
                this.FilterText = this.selectedObject.OriginalText;
            }
            else
            {
                this.FilterText = string.Empty;
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
                    this.FilterText = string.Empty;
                }
                this.RaisePropertyChanged(); 
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get
            {
                return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand<string>(async enteredText => await this.FindMatchOptionAndSendAnswerQuestionCommandAsync(enteredText)));
            }
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    if (!QuestionState.IsAnswered)
                    {
                        ResetTextInEditor = "";
                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                        return;
                    }
                    try
                    {
                        await this.Answering.SendRemoveAnswerCommandAsync(
                            new RemoveAnswerCommand(this.interviewId,
                                this.principal.CurrentUserIdentity.UserId,
                                this.questionIdentity,
                                DateTime.UtcNow));

                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                    }
                    catch (InterviewException exception)
                    {
                        this.QuestionState.Validity.ProcessException(exception);
                    }
                });
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
                    Action sendCommand = async () => await this.SendAnswerFilteredComboboxQuestionCommandAsync(this.selectedObject.Value);
                    sendCommand();
                }
                this.RaisePropertyChanged();
            }
        }

        private bool isInitialized = false;
        private string filterText = string.Empty;
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

                this.RaisePropertyChanged();

                this.CanRemoveAnswer = !string.IsNullOrEmpty(this.filterText);
            }
        }

        private bool canRemoveAnswer;

        public bool CanRemoveAnswer
        {
            set
            {
                if (canRemoveAnswer != value)
                {
                    this.canRemoveAnswer = value;
                    this.RaisePropertyChanged(); 
                }
            }
            get { return canRemoveAnswer; }
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

            var options = this.interview.GetFilteredOptionsForQuestion(questionIdentity, (int)this.answerOnParentQuestion.Value, textHint)   
                .Select(x => x)
                .Take(SuggestionsMaxCount)
                .ToList();

            foreach (var cascadingComboboxItemViewModel in options)
            {
                yield return CreateFormattedOptionModel(cascadingComboboxItemViewModel, textHint);
            }
        }

        private CascadingComboboxItemViewModel CreateFormattedOptionModel(CategoricalOption model, string hint = null)
        {
            var text = !string.IsNullOrEmpty(hint) ? 
                Regex.Replace(model.Title, hint, "<b>" + hint + "</b>", RegexOptions.IgnoreCase) :
                model.Title;

            return new CascadingComboboxItemViewModel
                   {
                       Text = text,
                       OriginalText = model.Title,
                       Value = model.Value,
                       ParentValue = model.ParentValue.Value,
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

        private async Task FindMatchOptionAndSendAnswerQuestionCommandAsync(string enteredText)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            var answerViewModel = this.interview.GetOptionForQuestionWithFilter(this.questionIdentity, enteredText, (int)answerOnParentQuestion.Value); 
                
            if (answerViewModel == null)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(string.Format(UIResources.Interview_Question_Cascading_NoMatchingValue, enteredText));
                return;
            }

            var answerValue = answerViewModel.Value;
            if (this.selectedObject != null && answerValue == this.selectedObject.Value)
            {
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }

            await this.SendAnswerFilteredComboboxQuestionCommandAsync(answerValue);
        }

        private async Task SendAnswerFilteredComboboxQuestionCommandAsync(decimal answerValue)
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
                await this.Answering.SendAnswerQuestionCommandAsync(command);

                if (this.selectedObject != null)
                    this.resetTextInEditor = this.selectedObject.OriginalText;
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            if (this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                var parentAnswerModel = this.interview.GetSingleOptionAnswer(this.parentQuestionIdentity);
                if (parentAnswerModel.IsAnswered)
                {
                    this.answerOnParentQuestion = parentAnswerModel.Answer;
                    this.UpdateSuggestionsList(string.Empty);
                }              
            }

            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                CanRemoveAnswer = true;
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.ResetTextInEditor = null;
                    this.CanRemoveAnswer = false;
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public void Handle(AnswerRemoved @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                this.ResetTextInEditor = string.Empty;
                this.CanRemoveAnswer = false;
            }
        }
    }
}