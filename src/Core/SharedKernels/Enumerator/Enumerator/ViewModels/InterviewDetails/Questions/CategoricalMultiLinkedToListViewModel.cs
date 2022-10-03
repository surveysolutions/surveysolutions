using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
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
    public class CategoricalMultiLinkedToListViewModel : 
        CategoricalMultiViewModelBase<int, int>,
        IAsyncViewModelEventHandler<TextListQuestionAnswered>,
        IAsyncViewModelEventHandler<LinkedToListOptionsChanged>,
        IAsyncViewModelEventHandler<MultipleOptionsQuestionAnswered>,
        IAsyncViewModelEventHandler<QuestionsEnabled>,
        IAsyncViewModelEventHandler<QuestionsDisabled>
    {
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private int[] selectedOptionsToSave;
        private Guid linkedToQuestionId;

        public CategoricalMultiLinkedToListViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel,
            IInterviewViewModelFactory interviewViewModelFactory,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher) 
            : base(
            questionStateViewModel, questionnaireRepository, eventRegistry, interviewRepository, principal, answering,
            instructionViewModel, throttlingModel, mainThreadAsyncDispatcher)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<int>>();
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
        }

        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<int>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => x.Value).ToArray();

        protected override int[] GetAnsweredOptionsFromInterview(IStatefulInterview interview) 
            => interview.GetMultiOptionLinkedToListQuestion(this.Identity)?.GetAnswer()?.CheckedValues.ToArray();

        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<int> optionViewModel, int answer)
            => optionViewModel.Checked = answer == optionViewModel.Value;

        protected override void RemoveAnswerFromOptionViewModel(CategoricalMultiOptionViewModel<int> optionViewModel)
            => optionViewModel.Checked = false;

        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId)
            => new AnswerMultipleOptionsQuestionCommand(interviewId, userId, this.Identity.Id, this.Identity.RosterVector, this.selectedOptionsToSave);

        protected override IEnumerable<CategoricalMultiOptionViewModel<int>> GetOptions(IStatefulInterview interview)
        {
            var listQuestion = interview.FindQuestionInQuestionBranch(this.linkedToQuestionId, this.Identity);
            if (listQuestion == null || listQuestion.IsDisabled()) yield break;

            var listOptions = listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows;
            var filteredOptions = interview.GetMultiOptionLinkedToListQuestion(this.Identity)?.Options;
            
            if (listOptions == null || filteredOptions == null) yield break;

            foreach (var optionCode in filteredOptions)
            {
                var vm = interviewViewModelFactory.GetNew<CategoricalMultiOptionViewModel<int>>();
                base.InitViewModel(listOptions.First(x => x.Value == optionCode).Text, optionCode, interview, vm, null);

                yield return vm;
            }
        }

        protected override bool IsInterviewAnswer(int interviewAnswer, int optionValue)
            => interviewAnswer == optionValue;

        public async Task HandleAsync(TextListQuestionAnswered @event)
        {
            if (@event.QuestionId != this.linkedToQuestionId)
                return;

            await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                foreach (var answer in @event.Answers)
                {
                    var option = this.Options.FirstOrDefault(o => o.Value == answer.Item1);
                    if (option != null) option.Title = answer.Item2;
                }
            });
        }

        public async Task HandleAsync(LinkedToListOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            await this.UpdateViewModelsAsync();
        }

        public async Task HandleAsync(MultipleOptionsQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;

            await this.UpdateViewModelsByAnsweredOptionsAsync(@event.SelectedValues.Select(Convert.ToInt32).ToArray());
        }

        public async Task HandleAsync(QuestionsEnabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId)) return;

            await this.UpdateViewModelsAsync();
        }

        public async Task HandleAsync(QuestionsDisabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId))
                return;

            await this.UpdateViewModelsAsync();
        }

        public override async Task HandleAsync(AnswersRemoved @event)
        {
            if (@event.Questions.Contains(this.Identity))
                await this.UpdateViewModelsByAnsweredOptionsAsync(Array.Empty<int>());

            if (@event.Questions.Any(question => question.Id == this.linkedToQuestionId))
                await this.UpdateViewModelsAsync();
        }
    }
}
