using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IInterviewEntityViewModel : ICompositeEntity
    {
        Identity Identity { get; }
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
    
    public interface IInterviewEntityLateInitViewModel : IInterviewEntityViewModel
    {
        void InitDataIfNeed();
    }

    public abstract class InterviewQuestionViewModelBase : MvxNotifyPropertyChanged,
        IInterviewEntityLateInitViewModel
    {
        protected Identity questionIdentity;
        public Identity Identity => questionIdentity;
        protected string interviewId;
        protected NavigationState NavigationState { get; private set; }
        private bool isInitialized = false;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;
            this.NavigationState = navigationState;

            InitFast();
        }

        public void InitDataIfNeed()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                InitData();
            }
        }

        public abstract void InitFast();
        public abstract void InitData();
    }
}