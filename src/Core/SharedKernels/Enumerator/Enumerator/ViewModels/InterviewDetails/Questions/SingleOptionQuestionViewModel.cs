using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel, 
        IDisposable,
        IDetailsCompositeItem,
        ILiteEventHandler<AnswerRemoved>
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly QuestionInstructionViewModel instructionViewModel;

        public SingleOptionQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.instructionViewModel = instructionViewModel;
        }

        private Identity questionIdentity;
        private Guid interviewId;

        public IList<SingleOptionQuestionOptionViewModel> Options { get; private set; }
        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public bool HasOptions => true;

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.instructionViewModel.Init(interviewId, entityIdentity);
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;
            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

            this.UpdateQuestionOptions();

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateQuestionOptions()
        {
            var interview = this.interviewRepository.Get(interviewId.FormatGuid());
            var answerModel = interview.GetSingleOptionAnswer(this.questionIdentity);
            var selectedValue = Monads.Maybe(() => answerModel.Answer);

            this.Options = this.filteredOptionsViewModel.GetOptions()
                .Select(model => this.ToViewModel(model, isSelected: model.Value == selectedValue))
                .ToList();
        }

        private void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            this.UpdateQuestionOptions();
            this.RaisePropertyChanged(() => Options);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            var previousOption = this.Options.SingleOrDefault(option => option.Selected && option != selectedOption);

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);

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

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity,
                        DateTime.UtcNow));
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                foreach (var option in this.Options.Where(option => option.Selected))
                {
                    option.Selected = false;
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();

            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
        }

        public IEnumerable<object> Children
        {
            get
            {
                var result = new List<Object>();
                result.Add(this.QuestionState.Header);
                if (this.instructionViewModel.HasInstructions)
                    result.Add(this.instructionViewModel);
                result.Add(new OptionTopBorderViewModel<SingleOptionQuestionAnswered>(this.QuestionState));
                result.AddRange(this.Options);
                result.Add(new OptionBottomBorderViewModel<SingleOptionQuestionAnswered>(this.QuestionState));
                result.Add(this.QuestionState.Validity);
                result.Add(this.QuestionState.Comments);
                return result;
            }
        }
    }
}