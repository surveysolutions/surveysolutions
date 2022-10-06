using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiLinkedToQuestionViewModel : CategoricalMultiViewModelBase<RosterVector, RosterVector>,
        IAsyncViewModelEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IAsyncViewModelEventHandler<LinkedOptionsChanged>,
        IAsyncViewModelEventHandler<RosterInstancesTitleChanged>
    {
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private RosterVector[] selectedOptionsToSave;
        private HashSet<Guid> parentRosters;

        public CategoricalMultiLinkedToQuestionViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel,
            IInterviewViewModelFactory interviewViewModelFactory, IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher)
            : base(questionStateViewModel, questionnaireRepository, eventRegistry,
                interviewRepository, principal, answering, instructionViewModel, throttlingModel,
                mainThreadAsyncDispatcher)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<RosterVector>>();
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
            this.parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(linkedToQuestionId).ToHashSet();
        }

        protected override bool IsInterviewAnswer(RosterVector interviewAnswer, RosterVector optionValue)
            => interviewAnswer == optionValue;

        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<RosterVector>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => x.Value).ToArray();
        
        protected override RosterVector[] GetAnsweredOptionsFromInterview(IStatefulInterview interview)
            => interview.GetLinkedMultiOptionQuestion(this.Identity)?.GetAnswer()?.ToRosterVectorArray();

        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<RosterVector> optionViewModel, RosterVector answer)
            => optionViewModel.Checked = answer == optionViewModel.Value;

        protected override void RemoveAnswerFromOptionViewModel(CategoricalMultiOptionViewModel<RosterVector> optionViewModel)
            => optionViewModel.Checked = false;
        
        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId)
        => new AnswerMultipleOptionsLinkedQuestionCommand(
                interviewId,
                userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                this.selectedOptionsToSave);

        protected override IEnumerable<CategoricalMultiOptionViewModel<RosterVector>> GetOptions(IStatefulInterview interview)
        {
            foreach (var answeredOption in interview.GetLinkedMultiOptionQuestion(this.Identity).Options)
            {
                var vm = interviewViewModelFactory.GetNew<CategoricalMultiOptionViewModel<RosterVector>>();
                base.InitViewModel(interview.GetLinkedOptionTitle(this.Identity, answeredOption), answeredOption, interview, vm, null);

                yield return vm;
            }
        }

        public async Task HandleAsync(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;
            await this.UpdateViewModelsByAnsweredOptionsAsync(@event.SelectedRosterVectors?.Select(RosterVector.Convert).ToArray());
        }

        public async Task HandleAsync(LinkedOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            await this.UpdateViewModelsAsync();
        }

        public virtual async Task HandleAsync(RosterInstancesTitleChanged @event)
        {
            if (!@event.ChangedInstances.Any(x => this.parentRosters.Contains(x.RosterInstance.GroupId))) return;

            await this.UpdateViewModelsAsync();
        }
    }
}
