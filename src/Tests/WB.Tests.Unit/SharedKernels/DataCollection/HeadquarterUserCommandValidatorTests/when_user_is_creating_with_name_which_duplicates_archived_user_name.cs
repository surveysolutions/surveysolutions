using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_user_is_creating_with_name_which_duplicates_archived_user_name : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            var userId = Guid.Parse("11111111111111111111111111111111");
            var user = Create.UserDocument(isArchived: true, userName: userName, userId:userId);

            headquarterUserCommandValidatorser = CreateHeadquarterUserCommandValidatorWithUsers(user);
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => headquarterUserCommandValidatorser.Validate(null, Create.CreateUserCommand(role: UserRoles.Supervisor, userName: userName)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserNameTakenByArchivedUsers = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserNameUsedByArchivedUser);

        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
        private static UserException exception;
        private static string userName = "name";
    }
}