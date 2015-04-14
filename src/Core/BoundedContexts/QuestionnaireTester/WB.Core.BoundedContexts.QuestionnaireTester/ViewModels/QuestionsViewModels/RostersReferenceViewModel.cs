using System.Collections.Generic;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RostersReferenceViewModel : IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public List<RosterReferenceViewModel> RosterReferences { get; set; }
    }
}