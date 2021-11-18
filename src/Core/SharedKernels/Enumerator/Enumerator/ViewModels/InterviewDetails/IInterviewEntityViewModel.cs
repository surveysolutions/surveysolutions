using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public interface IInterviewEntityViewModel : ICompositeEntity
    {
        Identity Identity { get; }
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
    
    public interface IInterviewEntityLateInitViewModel : IInterviewEntityViewModel
    {
        //void Setup(string interviewId, Identity entityIdentity, NavigationState navigationState);
        void InitIfNeed();
        IInterviewEntityViewModel WrappedEntity { get; }
    }

    public class InterviewEntityViewModelWrapper : IInterviewEntityLateInitViewModel, ICompositeEntity
    {
        private string interviewId;
        private Identity identity;
        private NavigationState navigationState;
        private readonly IInterviewEntityViewModel viewModel;
        private bool isInitialized;

        public InterviewEntityViewModelWrapper(IInterviewEntityViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public Identity Identity => identity;
        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.identity = entityIdentity;
            this.navigationState = navigationState;
        }

        public void InitIfNeed()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                viewModel.Init(interviewId, identity, navigationState);
            }
        }

        public IInterviewEntityViewModel WrappedEntity => viewModel;
    }
    /*public abstract class InterviewEntityViewModelBase : IInterviewEntityLateInitViewModel
    {
        public virtual Identity Identity { get; private set; }
        private string interviewId;
        private NavigationState navigationState;
        private bool isInitialized = false;

        public virtual void Setup(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            this.interviewId = interviewId;
            this.navigationState = navigationState;
        }

        public void InitIfNeed()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                Init(interviewId, Identity, navigationState);
            }
        }

        public abstract void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }*/
}