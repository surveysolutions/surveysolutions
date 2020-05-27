using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Web.Headquarters.CommandTransformatorTests
{
    internal class CommandTransformatorTestsContext
    {
        public static CommandTransformator CreateCommandTransformator()
        {
            return new CommandTransformator(Mock.Of<IAuthorizedUser>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IInterviewUniqueKeyGenerator>(),
                Mock.Of<IUserViewFactory>());
        }
    }
}
