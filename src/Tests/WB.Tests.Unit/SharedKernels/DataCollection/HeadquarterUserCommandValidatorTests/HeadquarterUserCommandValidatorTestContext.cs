using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    [Subject(typeof (HeadquarterUserCommandValidator))]
    internal class HeadquarterUserCommandValidatorTestContext
    {
        protected static HeadquarterUserCommandValidator CreateHeadquarterUserCommandValidator(
            IQueryableReadSideRepositoryReader<UserDocument> users = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews = null)
        {
            return
                new HeadquarterUserCommandValidator(
                    users ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                    interviews ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }

        protected static User CreateUser()
        {
            return new User();
        }
    }
}