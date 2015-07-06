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
    internal class when_user_is_creating_with_name_which_dublicates_archived_user_name : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            var userStorage = new TestInMemoryWriter<UserDocument>();
            userStorage.Store(Create.UserDocument(isArchived: true, userName: userName), userId);

            headquarterUserCommandValidatorser =
              CreateHeadquarterUserCommandValidator(
                  users:userStorage);
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => headquarterUserCommandValidatorser.Validate(null, Create.CreateUserCommand(role: UserRoles.Supervisor, userName: userName)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserNameTakenByArchivedUsers = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserNameTakenByArchivedUsers);

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
        private static UserException exception;
        private static string userName = "name";
    }
}