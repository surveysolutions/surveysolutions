using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : MvxNotifyPropertyChanged, 
         IInterviewEntityViewModel,
         ILiteEventHandler<SingleOptionQuestionAnswered>,
         ILiteEventHandler<AnswersRemoved>,
         ICompositeQuestion,
         IDisposable
    {
        public class CascadingComboboxItemViewModel 
        {
            public string Text { get; set; }
            public string OriginalText { get; set; }
            public decimal Value { get; set; }
            public decimal ParentValue { get; set; }

            public override string ToString()
            {
                return this.Text.Replace("</b>", "").Replace("<b>", "");
            }
        }

        private readonly IPrincipal principal;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private Identity questionIdentity;
        private Identity parentQuestionIdentity;
        private string interviewId;
        private Guid interviewGuid;
        private decimal? answerOnParentQuestion = null;
        private CancellationTokenSource suggestionsCancellation;
        
        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public AnsweringViewModel Answering { get; private set; }
        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        public Identity Identities => this.questionIdentity;

        private const int SuggestionsMaxCount = 50;

        public CascadingSingleOptionQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering, 
            QuestionInstructionViewModel instructionViewModel,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.principal = principal ?? throw new ArgumentNullException(nameof(principal));
            this.questionnaireRepository = questionnaireRepository ?? throw new ArgumentNullException(nameof(questionnaireRepository));
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            answering.Delay = TimeSpan.FromMilliseconds(500);
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
            if (!cascadingQuestionParentId.HasValue) throw new NullReferenceException($"Parent of cascading question {entityIdentity} is missing");

            this.questionIdentity = entityIdentity;
            this.interviewGuid = interview.Id;
            this.interviewId = interviewId;
            var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

            this.parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

            var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (parentSingleOptionQuestion.IsAnswered)
            {
                this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            }

            this.UpdateOptionsState(interview);
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateOptionsState(IStatefulInterview interview)
        {
            var singleOptionQuestion = interview.GetSingleOptionQuestion(this.Identity);

            if (singleOptionQuestion.IsAnswered)
            {
                var selectedValue = singleOptionQuestion.GetAnswer().SelectedValue;
                var answerOption = interview.GetOptionForQuestionWithoutFilter(this.questionIdentity, selectedValue, (int?) this.answerOnParentQuestion);

                this.SelectedObject = this.CreateFormattedOptionModel(answerOption);
                this.ResetTextInEditor = this.selectedObject.OriginalText;
                this.FilterText = this.selectedObject.OriginalText;

                this.DefaultText = answerOption == null ? String.Empty : answerOption.Title;
                this.ResetTextInEditor = this.DefaultText;
            }
            else
            {
                this.UpdateSuggestionsList(this.FilterText ?? DefaultText ?? string.Empty);
            }
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

        public ICommand ValueChangeCommand => new MvxAsyncCommand<string>(this.FindMatchOptionAndSendAnswerQuestionAsync);

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    if (!questionState.IsAnswered)
                    {
                        ResetTextInEditor = "";
                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                        return;
                    }
                    try
                    {
                        await this.Answering.SendRemoveAnswerCommandAsync(
                            new RemoveAnswerCommand(this.interviewGuid,
                                this.principal.CurrentUserIdentity.UserId,
                                this.questionIdentity,
                                DateTime.UtcNow));

                        this.ClearText();
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
            get => this.selectedObject;
            set => this.RaiseAndSetIfChanged(ref this.selectedObject, value);
        }

        private string filterText;
        public string FilterText
        {
            get => this.filterText;
            set
            {
                if (value == this.filterText)
                {
                    return;
                }

                this.filterText = value;
                
                this.UpdateSuggestionsList(this.filterText);
                this.RaisePropertyChanged();
                this.CanRemoveAnswer = !string.IsNullOrEmpty(this.filterText);
            }
        }

        public string DefaultText { get; set; }

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
            get => canRemoveAnswer;
        }

        private void UpdateSuggestionsList(string textHint)
        {
            this.suggestionsCancellation?.Cancel();
            this.suggestionsCancellation = new CancellationTokenSource();

            Answering.ExecuteActionAsync(async token =>
                await Task.Run(() => this.UpdateSuggestions(textHint, token), token),
                suggestionsCancellation.Token).ConfigureAwait(false);
        }

        private void UpdateSuggestions(string textHint, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var list = this.GetSuggestionsList(textHint).ToList();

            if (!token.IsCancellationRequested)
            {
                if (list.Count > 0)
                {
                    this.mainThreadDispatcher.RequestMainThreadAction(() => this.AutoCompleteSuggestions = list);
                }
                else
                {
                    this.mainThreadDispatcher.RequestMainThreadAction(this.SetSuggestionsEmpty);
                }
            }
        }

        private IEnumerable<CascadingComboboxItemViewModel> GetSuggestionsList(string textHint)
        {
            if (!this.answerOnParentQuestion.HasValue) 
                yield break;

            var interview = this.interviewRepository.Get(this.interviewId);

            var options = interview.GetTopFilteredOptionsForQuestion(questionIdentity, (int)this.answerOnParentQuestion.Value, 
                textHint, SuggestionsMaxCount);

            foreach (var cascadingComboboxItemViewModel in options)
            {
                yield return CreateFormattedOptionModel(cascadingComboboxItemViewModel, textHint);
            }
        }

        private CascadingComboboxItemViewModel CreateFormattedOptionModel(CategoricalOption model, string hint = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (!hint.IsNullOrEmpty())
            {
                //Insert and IndexOf with culture specific search cannot be used together 
                //http://stackoverflow.com/questions/4923187/string-indexof-and-replace
                var startIndexOfSearchedText = model.Title.IndexOf(hint, StringComparison.OrdinalIgnoreCase);
                if (startIndexOfSearchedText > -1)
                {
                    return new CascadingComboboxItemViewModel
                    {
                        Text = model.Title.Insert(startIndexOfSearchedText + hint.Length, "</b>")
                            .Insert(startIndexOfSearchedText, "<b>"),
                        Value = model.Value,
                        OriginalText = model.Title,
                        ParentValue = model.ParentValue.Value
                    };
                }
            }

            return new CascadingComboboxItemViewModel
            {
                Text = model.Title,
                OriginalText = model.Title,
                Value = model.Value,
                ParentValue = model.ParentValue.Value
            };
        }

        private void SetSuggestionsEmpty()
        {
            this.AutoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();
        }

        private List<CascadingComboboxItemViewModel> autoCompleteSuggestions = new List<CascadingComboboxItemViewModel>();

        public List<CascadingComboboxItemViewModel> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        private async Task FindMatchOptionAndSendAnswerQuestionAsync(string enteredText)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            var interview = this.interviewRepository.Get(this.interviewId);

            var categoricalOption = interview.GetOptionForQuestionWithFilter(this.questionIdentity, enteredText, (int)answerOnParentQuestion.Value);
                
            if (categoricalOption == null)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(string.Format(UIResources.Interview_Question_Cascading_NoMatchingValue, enteredText));
                return;
            }

            await this.SendAnswerFilteredComboboxQuestionCommandAsync(categoricalOption.Value);
        }

        private async Task SendAnswerFilteredComboboxQuestionCommandAsync(int answerValue)
        {
            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewGuid,
                this.principal.CurrentUserIdentity.UserId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                answerValue);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);

                if (this.selectedObject != null)
                {
                    this.resetTextInEditor = this.selectedObject.OriginalText;
                    this.FilterText = selectedObject.OriginalText;
                    this.DefaultText = selectedObject.OriginalText;
                }

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void ClearText()
        {
            this.ResetTextInEditor = string.Empty;
            this.DefaultText = null;
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            if (this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                var interview = this.interviewRepository.Get(this.interviewId);
                var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
                if (parentSingleOptionQuestion.IsAnswered)
                {
                    this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
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
                    this.ClearText();
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }
    }
}