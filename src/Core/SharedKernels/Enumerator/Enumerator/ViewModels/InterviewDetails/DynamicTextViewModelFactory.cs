using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    internal class DynamicTextViewModelFactory : IDynamicTextViewModelFactory
    {
        private readonly IServiceLocator serviceLocator;

        public DynamicTextViewModelFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public DynamicTextViewModel CreateDynamicTextViewModel()
            => this.serviceLocator.GetInstance<DynamicTextViewModel>();

        public ErrorMessageViewModel CreateErrorMessage() => this.serviceLocator.GetInstance<ErrorMessageViewModel>();
    }
}