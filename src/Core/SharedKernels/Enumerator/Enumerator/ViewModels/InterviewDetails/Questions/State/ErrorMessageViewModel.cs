using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ErrorMessageViewModel : DynamicTextViewModel
    {
        public ErrorMessageViewModel(ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService) : 
                base(eventRegistry, interviewRepository, substitutionService)
        {
        }

        public string ItemTag => base.identity + "_Msg";
    }
}