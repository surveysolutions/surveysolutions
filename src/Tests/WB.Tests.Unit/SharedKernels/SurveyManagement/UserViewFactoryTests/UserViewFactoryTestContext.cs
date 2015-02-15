using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [Subject(typeof(UserViewFactory))]
    class UserViewFactoryTestContext
    {
        protected static UserViewFactory CreateUserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            return new UserViewFactory(users ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>());
        }
    }
}
