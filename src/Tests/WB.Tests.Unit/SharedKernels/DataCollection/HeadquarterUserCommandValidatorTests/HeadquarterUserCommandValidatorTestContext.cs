using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    [Subject(typeof (HeadquarterUserCommandValidator))]
    internal class HeadquarterUserCommandValidatorTestContext
    {
        protected static HeadquarterUserCommandValidator CreateHeadquarterUserCommandValidator(
            IPlainStorageAccessor<UserDocument> users = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews = null)
        {
            return
                new HeadquarterUserCommandValidator(
                    users ?? Mock.Of<IPlainStorageAccessor<UserDocument>>());
        }

        protected static HeadquarterUserCommandValidator CreateHeadquarterUserCommandValidatorWithUsers(
            params UserDocument[] users)
        {
            var userStorage = new TestPlainStorage<UserDocument>();
            foreach (var user in users)
            {
                userStorage.Store(user, user.PublicKey);
            }
            return
                new HeadquarterUserCommandValidator(
                    userStorage);
        }
    }
}