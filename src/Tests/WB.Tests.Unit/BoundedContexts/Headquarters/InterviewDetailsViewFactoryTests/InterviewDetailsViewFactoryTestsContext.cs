using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewDetailsViewFactoryTests
{
    [Subject(typeof(InterviewDetailsViewFactory))]
    internal class InterviewDetailsViewFactoryTestsContext
    {
    }
}