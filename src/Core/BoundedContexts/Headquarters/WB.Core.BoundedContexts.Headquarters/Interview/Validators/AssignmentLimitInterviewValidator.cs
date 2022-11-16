using System;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Validators;

public class AssignmentLimitInterviewValidator: 
    ICommandValidator<StatefulInterview, CreateInterview>,
    ICommandValidator<StatefulInterview, InterviewCommand>
{
    private readonly IAssignmentsService assignmentsService;
    private readonly IAggregateRootPrototypeService prototypeService;
    private readonly IAggregateRootCache cache;

    public AssignmentLimitInterviewValidator(IAssignmentsService assignmentsService,
        IAggregateRootPrototypeService prototypeService,
        IAggregateRootCache cache)
    {
        this.assignmentsService = assignmentsService;
        this.prototypeService = prototypeService;
        this.cache = cache;
    }

    public void Validate(StatefulInterview aggregate, CreateInterview command)
    {
        if (command.Mode == InterviewMode.CAWI)
            CheckAssignmentLimitAndThrow(command.AssignmentId, command.InterviewId);
    }

    public void Validate(StatefulInterview aggregate, InterviewCommand command)
    {
        if (aggregate.Mode != InterviewMode.CAWI)
            return;
        
        var prototypeType = prototypeService.GetPrototypeType(command.InterviewId);
        if (prototypeType == PrototypeType.Temporary)
            CheckAssignmentLimitAndThrow(aggregate.GetAssignmentId(), command.InterviewId);
    }
    
    private void CheckAssignmentLimitAndThrow(int? assignmentId, Guid interviewId)
    {
        if (assignmentId is null)
            return;
        
        var assignment = assignmentsService.GetAssignment(assignmentId.Value);
        if (assignment.WebMode != true)
            return;
        
        if (assignment.InterviewsNeeded is null or > 0)
            return;

        cache.EvictAggregateRoot(interviewId);
        throw new InterviewException(CommandValidatorsMessages.AssignmentLimitReached, InterviewDomainExceptionType.InterviewSizeLimitReached);
    }
}