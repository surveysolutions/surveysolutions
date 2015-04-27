using System.Collections.ObjectModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RostersReferenceViewModel : MvxNotifyPropertyChanged, IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public ObservableCollection<RosterReferenceViewModel> RosterReferences { get; set; }

        public void Init(string interviewId, Identity questionIdentity)
        {
            
        }
    }
}