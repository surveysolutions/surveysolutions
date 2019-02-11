using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : BaseFilteredQuestionViewModel, 
         ILiteEventHandler<SingleOptionQuestionAnswered>,
         ICompositeQuestionWithChildren
    {
        private readonly ThrottlingViewModel throttlingModel;

        private readonly IQuestionnaireStorage questionnaireRepository;

        private Identity parentQuestionIdentity;
        private int? answerOnParentQuestion;

        private bool showCascadingAsList;
        private int showCascadingAsListThreshold;

        private int defaultCascadingAsListThreshold = 50;

        public CascadingSingleOptionQuestionViewModel(IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry, 
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel, 
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher,
            ThrottlingViewModel throttlingModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository,
                eventRegistry: eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository ??
                                           throw new ArgumentNullException(nameof(questionnaireRepository));

            this.Options = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();

            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;

            this.throttlingModel = throttlingModel;
            this.throttlingModel.Init(SaveAnswer);
        }

        protected override void Initialize(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            showCascadingAsList = questionnaire.ShowCascadingAsList(entityIdentity.Id);
            showCascadingAsListThreshold = Math.Min(defaultCascadingAsListThreshold, questionnaire.GetCascadingAsListThreshold(entityIdentity.Id) ?? defaultCascadingAsListThreshold);

            var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
            if (!cascadingQuestionParentId.HasValue) throw new NullReferenceException($"Parent of cascading question {entityIdentity} is missing");
            
            var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

            this.parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

            var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (parentSingleOptionQuestion.IsAnswered())
            {
                this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            }

            UpdateOptions();
        }
        protected override CategoricalOption GetOptionByFilter(string filter)
            => this.interview.GetOptionForQuestionWithFilter(this.Identity, filter, this.answerOnParentQuestion);

        protected override CategoricalOption GetAnsweredOption(int answer)
            => this.interview.GetOptionForQuestionWithoutFilter(this.Identity, answer, this.answerOnParentQuestion);

        protected override IEnumerable<CategoricalOption> GetSuggestions(string filter)
        {
            if (!this.answerOnParentQuestion.HasValue)
                yield break;

            var categoricalOptions = this.interview.GetTopFilteredOptionsForQuestion(this.Identity,
                this.answerOnParentQuestion.Value, filter, SuggestionsMaxCount);

            foreach (var categoricalOption in categoricalOptions)
                yield return categoricalOption;
        }

        protected override async Task SaveAnswerAsync(string optionText)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            await base.SaveAnswerAsync(optionText);
        }

        public async void Handle(SingleOptionQuestionAnswered @event)
        {
            if (!this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector)) return;

            var parentSingleOptionQuestion = this.interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (!parentSingleOptionQuestion.IsAnswered()) return;

            this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            
            //add fork?
            await this.UpdateFilterAndSuggestionsAsync(string.Empty);

            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.RaisePropertyChanged(() => RenderAsComboBox);
                UpdateOptions();
                this.RaisePropertyChanged(() => Options);
                this.RaisePropertyChanged(() => Children);
            });
        }

        
        public bool RenderAsComboBox
        {
            get {

                if (!showCascadingAsList || answerOnParentQuestion == null)
                    return true;

                //cache the value to void extra db calls
                return !interview.HasCascadingQuestionMoreOptionsThenInThreshold(this.Identity, showCascadingAsListThreshold);
            }
        }


        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();

                if (!showCascadingAsList)
                    return result;

                result.Add(new OptionBorderViewModel(this.QuestionState, true));
                result.AddCollection(Options);
                result.Add(new OptionBorderViewModel(this.QuestionState, false));
                return result;
            }
        }

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> Options { get; private set; }

        private void UpdateOptions()
        {
            this.Options.ForEach(x => x.DisposeIfDisposable());
            this.Options.Clear();

            if (!RenderAsComboBox)
            {
                var singleOptionQuestionOptionViewModels = GetSuggestions(String.Empty)
                    .Select(model => this.ToViewModel(model, isSelected: Answer.HasValue && model.Value == Answer.Value))
                    .ToList();

                singleOptionQuestionOptionViewModels.ForEach(x => this.Options.Add(x));
            }
        }

        private SingleOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                QuestionState = this.QuestionState,
            };
            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            selectedOptionToSave = selectedOption.Value;

            this.Options.Where(x => x.Selected && x.Value != selectedOptionToSave).ForEach(x => x.Selected = false);

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                this.throttlingModel.CancelPendingAction();
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }


        private int? previousOptionToReset = null;
        private int? selectedOptionToSave = null;
        private IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;

        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave == this.previousOptionToReset)
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;
            
            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.principal.CurrentUserIdentity.UserId,
                this.Identity.Id,
                this.Identity.RosterVector,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);

                this.previousOptionToReset = this.selectedOptionToSave;

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private SingleOptionQuestionOptionViewModel GetOptionByValue(int? value)
        {
            return value.HasValue
                ? this.Options.FirstOrDefault(x => x.Value == value.Value)
                : null;
        }
    }
}
