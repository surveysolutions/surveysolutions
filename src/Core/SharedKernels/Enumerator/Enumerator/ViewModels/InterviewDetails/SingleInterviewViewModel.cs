using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class SingleInterviewViewModel : BaseViewModel
    {
        protected SingleInterviewViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService)
            : base(principal, viewModelNavigationService) {}

        public abstract IReadOnlyCollection<Translation> AvailableTranslations { get; }
        public abstract Guid? CurrentLanguage { get; }
    }
}