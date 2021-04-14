using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(ResponsibleAssignmentValidator))]
    public class WebModeResponsibleAssignmentValidatorTests 
    {
        [Test]
        public void should_not_allow_creating_assignment_on_non_interviewer_in_web_mode()
        {
            var userViewFactory =
                Create.Storage.UserViewFactory(Create.Entity.HqUser(Id.gA, role: UserRoles.Supervisor));

            var validator = Create.Service.WebModeResponsibleAssignmentValidator(userViewFactory);

            TestDelegate act = () =>
                validator.Validate(null, Create.Command.CreateAssignment(responsibleId: Id.gA, webMode: true));

            Assert.That(act, Throws.Exception.InstanceOf<AssignmentException>().With.Message.EqualTo(CommandValidatorsMessages.WebModeAssignmentShouldBeOnInterviewer)
                .And.Property(nameof(AssignmentException.ExceptionType)).EqualTo(AssignmentDomainExceptionType.InvalidResponsible));
        }
    }
}
