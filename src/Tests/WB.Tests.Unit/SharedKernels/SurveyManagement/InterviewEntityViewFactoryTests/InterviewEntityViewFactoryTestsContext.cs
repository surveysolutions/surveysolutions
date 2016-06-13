using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    public class InterviewEntityViewFactoryTestsContext
    {
        public static IInterviewEntityViewFactory CreateInterviewEntityViewFactory()
        {
            return new InterviewEntityViewFactory(new SubstitutionService());
        }
    }
}