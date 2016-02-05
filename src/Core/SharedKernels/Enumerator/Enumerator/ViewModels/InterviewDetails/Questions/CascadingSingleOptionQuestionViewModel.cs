using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
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

        public Identity Identity { get { return this.questionIdentity; } }

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

        private async Task FindMatchOptionAndSendAnswerQuestionCommandAsync(string enteredText)
        {
            var answerViewModel = this.Options.SingleOrDefault(i => i.Title == enteredText && i.ParentValue == answerOnParentQuestion);

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
                var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
                var parentAnswerModel = interview.GetSingleOptionAnswer(this.parentQuestionIdentity);
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
                    this.QuestionState.IsAnswered = false;
                    this.CanRemoveAnswer = false;
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId.FormatGuid());
            this.QuestionState.Dispose();
        }

        public void Handle(AnswerRemoved @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                this.QuestionState.IsAnswered = false;
                this.ResetTextInEditor = string.Empty;
                this.CanRemoveAnswer = false;
            }
        }
    }
}