using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.CommandTransformatorTests
{
    internal class CommandTransformatorTestsContext
    {
        public static CommandTransformator CreateCommandTransformator()
        {
            return new CommandTransformator(Mock.Of<IAuthorizedUser>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IInterviewUniqueKeyGenerator>());
        }
    }
}
