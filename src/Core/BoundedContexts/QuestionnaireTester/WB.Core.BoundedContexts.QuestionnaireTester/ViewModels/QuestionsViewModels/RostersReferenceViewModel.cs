using System.Collections.ObjectModel;

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RostersReferenceViewModel : BaseInterviewItemViewModel
    {
        public override void Init(Identity identity, InterviewModel interviewModel, QuestionnaireModel questionnaireModel)
        {

        }

        public bool IsDisabled { get; set; }
        public ObservableCollection<RosterReferenceViewModel> RosterReferences { get; set; }
    }
}