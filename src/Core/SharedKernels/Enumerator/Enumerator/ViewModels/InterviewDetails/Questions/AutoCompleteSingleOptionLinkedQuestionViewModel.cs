#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AutoCompleteSingleOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IViewModelEventHandler<AnswersRemoved>,
        IViewModelEventHandler<LinkedOptionsChanged>,
        IViewModelEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private IViewModelEventRegistry liteEventRegistry;
        private IPrincipal principal;
        private IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState;
        private IStatefulInterview interview;
        private string? filterText;
        private IEnumerable<Guid> parentRosterIds;

        public AutoCompleteSingleOptionLinkedQuestionViewModel(
            IViewModelEventRegistry liteEventRegistry,
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.questionState = questionStateViewModel;
            this.parentRosterIds = Enumerable.Empty<Guid>();
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;

            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);
            this.questionState.Init(interviewId, entityIdentity, navigationState);

            this.interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire =
                this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            var linkedToRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(entityIdentity.Id);
            this.parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedEntity(linkedToRosterId).ToHashSet();
            BindSuggestionList();

            var selectedOption = this.AutoCompleteSuggestions.FirstOrDefault(x => x.Selected);
            if (selectedOption != null)
            {
                this.FilterText = selectedOption.Title;
            }
            
            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private void BindSuggestionList()
        {
            var singleOptionLinkedQuestionOptionViewModels = CreateOptions().ToList();
            this.AutoCompleteSuggestions = singleOptionLinkedQuestionOptionViewModels;
        }

        public List<SingleOptionLinkedQuestionOptionViewModel> AutoCompleteSuggestions { get; set; }

        public AnsweringViewModel Answering { get; set; }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        public Identity Identity { get; private set; }

        public string? FilterText
        {
            get => filterText;
            set => SetProperty(ref filterText, value);
        }

        public IMvxAsyncCommand ShowErrorIfNoAnswerCommand => new MvxAsyncCommand(SendAnswer);
        public IMvxCommand<string> FilterCommand => new MvxCommand<string>(Filter);

        private void Filter(string filter)
        {
            this.FilterText = filter;
        }

        public IMvxCommand<SingleOptionLinkedQuestionOptionViewModel> SaveAnswerBySelectedOptionCommand => new MvxCommand<SingleOptionLinkedQuestionOptionViewModel>(RememberSelectedOption);

        private void RememberSelectedOption(SingleOptionLinkedQuestionOptionViewModel option)
        {
            this.FilterText = option.Title;
        }

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(RemoveAnswer);

        private async Task RemoveAnswer()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interview.Id,
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                Clear();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private void Clear()
        {
            this.FilterText = null;
        }

        private async Task SendAnswer()
        {
            var selectedOption = this.AutoCompleteSuggestions.FirstOrDefault(x => x.Title.Equals(this.FilterText));

            if (selectedOption == null)
                return;

            var command = new AnswerSingleOptionLinkedQuestionCommand(
                this.interview.Id,
                this.principal.CurrentUserIdentity.UserId,
                this.Identity.Id,
                this.Identity.RosterVector,
                selectedOption.RosterVector);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private IEnumerable<SingleOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var linkedQuestion = interview.GetLinkedSingleOptionQuestion(this.Identity);

            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, linkedQuestion.GetAnswer()?.SelectedValue,
                    interview);
        }

        private SingleOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption,
            RosterVector answeredOption, IStatefulInterview interview)
        {
            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
                RosterVector = linkedOption,
                Title = interview.GetLinkedOptionTitle(this.Identity, linkedOption),
                Selected = linkedOption.Equals(answeredOption),
                QuestionState = this.questionState,
                Parent = this
            };
            
            return optionViewModel;
        }

        public void Dispose()
        {
            questionState.Dispose();
            InstructionViewModel.Dispose();
            liteEventRegistry.Unsubscribe(this);
        }


        public void Handle(AnswersRemoved @event)
        {
            for (int i = 0; i < @event.Questions.Length; i++)
            {
                var question = @event.Questions[i];
                if (this.Identity.Equals(question.Id, question.RosterVector))
                {
                    Clear();
                    break;
                }
            }
        }

        public void Handle(LinkedOptionsChanged @event)
        {
            bool meChanged = @event.ChangedLinkedQuestions.Any(x => x.QuestionId == this.Identity);

            if (meChanged)
            {
                BindSuggestionList();
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated =
                @event.ChangedInstances.Any(x => this.parentRosterIds.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                BindSuggestionList();
            }
        }
    }
}
