using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Interview.Validators;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Interviews;

[TestOf(typeof(AssignmentLimitInterviewValidator))]
public class AssignmentLimitInterviewValidatorTests
{ 
    [Test]
    public void should_not_allow_interview_creation_if_assignment_limit_reached()
    {
        var assignmentService = Create.Service.AssignmentService(
            Create.Entity.Assignment(id: 3, quantity: 5,
                webMode: true, interviewSummary: new InterviewSummary[]
                {
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                })
            );
        var prototypeService = Mock.Of<IAggregateRootPrototypeService>(s => 
            s.GetPrototypeType(Id.g3) == PrototypeType.Permanent);

        var validator = Create.Service.AssignmentLimitInterviewValidator(assignmentService, prototypeService);

        TestDelegate act = () => validator.Validate(null, Create.Command.CreateInterview(userId: Id.gA, assignmentId: 3, interviewMode: InterviewMode.CAWI));

        Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
            .With.Message.EqualTo(CommandValidatorsMessages.AssignmentLimitReached)
            .And.Property(nameof(InterviewException.ExceptionType))
            .EqualTo(InterviewDomainExceptionType.AssignmentLimitReached));
    }
    
    [Test]
    public void should_not_allow_answer_temporally_interview_if_assignment_limit_reached()
    {
        var assignmentService = Create.Service.AssignmentService(
            Create.Entity.Assignment(id: 3, quantity: 5,
                webMode: true, interviewSummary: new InterviewSummary[]
                {
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                    Create.Entity.InterviewSummary(Guid.NewGuid()),
                })
            );
        var prototypeService = Mock.Of<IAggregateRootPrototypeService>(s => 
            s.GetPrototypeType(Id.g7) == PrototypeType.Temporary);

        var validator = Create.Service.AssignmentLimitInterviewValidator(assignmentService, prototypeService);

        var statefulInterview = Create.AggregateRoot.StatefulInterview(Id.g7);
        statefulInterview.Apply(Create.Event.InterviewCreated(assignmentId: 3));
        statefulInterview.ChangeInterviewMode(Id.g1, DateTimeOffset.Now, InterviewMode.CAWI);

        TestDelegate act = () => validator.Validate(statefulInterview, Create.Command.AnswerTextQuestionCommand(userId: Id.gA, interviewId: Id.g7, answer: "ttt" ));

        Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
            .With.Message.EqualTo(CommandValidatorsMessages.AssignmentLimitReached)
            .And.Property(nameof(InterviewException.ExceptionType))
            .EqualTo(InterviewDomainExceptionType.AssignmentLimitReached));
    }
}