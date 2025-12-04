using AutoMapper;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.UI.WebTester.Services.Implementation;

public class WebTesterWebInterviewInterviewEntityFactory : WebInterviewInterviewEntityFactory
{
    public WebTesterWebInterviewInterviewEntityFactory(IMapper autoMapper, IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy, ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy, IWebNavigationService webNavigationService, ISubstitutionTextFactory substitutionTextFactory) : base(autoMapper, enumeratorGroupStateCalculationStrategy, supervisorGroupStateCalculationStrategy, webNavigationService, substitutionTextFactory)
    {
    }

    protected override void ApplyReviewState(GenericQuestion result, InterviewTreeQuestion question, IStatefulInterview callerInterview,
        bool isReviewMode)
    {
        base.ApplyReviewState(result, question, callerInterview, isReviewMode);

        result.IsForSupervisor = question.IsSupervisors || question.IsHidden;
    }
}
