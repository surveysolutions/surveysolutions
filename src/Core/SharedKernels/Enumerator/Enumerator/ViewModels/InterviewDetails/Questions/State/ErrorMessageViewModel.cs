﻿using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ErrorMessageViewModel : DynamicTextViewModel, IInterviewEntity
    {
        public ErrorMessageViewModel(ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService) : 
                base(eventRegistry, interviewRepository, substitutionService)
        {
        }

        public string ItemTag => base.identity + "_Msg";

        public void InitializeNavigation(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.InterviewId = interviewId;
            this.Identity = entityIdentity;
            this.NavigationState = navigationState;
        }

        public string InterviewId { get; private set; }
        public Identity Identity { get; private set; }
        public NavigationState NavigationState { get; private set; }
    }
}
