using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiLinkedToRosterTitleViewModel : CategoricalMultiLinkedToQuestionViewModel
    {
        private Guid linkedToRosterId;
        private HashSet<Guid> parentRosters;

        public CategoricalMultiLinkedToRosterTitleViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel) : base(
            questionStateViewModel, questionnaireRepository, eventRegistry, interviewRepository, principal, answering,
            instructionViewModel, throttlingModel)
        {
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            this.linkedToRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(this.Identity.Id);
            this.parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToRosterId).ToHashSet();
        }

        public override async Task HandleAsync(RosterInstancesTitleChanged @event)
        {
            if (!@event.ChangedInstances.Any(x => x.RosterInstance.GroupId == this.linkedToRosterId ||
                                                  this.parentRosters.Contains(x.RosterInstance.GroupId))) return;

            await this.UpdateViewModelsAsync();
        }
    }
}
