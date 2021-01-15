#nullable enable
using System;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Assignments.Validators
{
    public class WebModeResponsibleAssignmentValidator : ICommandValidator<AssignmentAggregateRoot, CreateAssignment>
    {
        private readonly IUserViewFactory userViewFactory;

        public WebModeResponsibleAssignmentValidator(IUserViewFactory userViewFactory)
        {
            this.userViewFactory = userViewFactory ?? throw new ArgumentNullException(nameof(userViewFactory));
        }

        public void Validate(AssignmentAggregateRoot? aggregate, CreateAssignment command)
        {
            if (command.WebMode == true)
            {
                var user = userViewFactory.GetUser(command.ResponsibleId);

                if (user == null || !user.IsInterviewer())
                {
                    throw new AssignmentException(CommandValidatorsMessages.WebModeAssignmentShouldBeOnInterviewer, AssignmentDomainExceptionType.InvalidResponsible);
                }
            }
        }
    }
}
