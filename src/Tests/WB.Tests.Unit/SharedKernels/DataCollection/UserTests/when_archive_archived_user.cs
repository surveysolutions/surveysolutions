using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_archived_user : UserTestContext
    {
        Establish context = () =>
        {
            eventContext = Create.EventContext();
            user = CreateUser();
            user.ApplyEvent(Create.UserArchived());
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserArchived);

        private static EventContext eventContext;
        private static User user;
        private static UserException exception;
    }
}