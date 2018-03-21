using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewAnswersCommandValidatorTests
{
    internal class when_responsible_supervisor_answer_on_supervisors_questions
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var mockOfInterviewSummaryViewFactory = new Mock<IInterviewSummaryViewFactory>();
            mockOfInterviewSummaryViewFactory.Setup(x => x.Load(interviewId)).Returns(new InterviewSummary
            {
                TeamLeadId = responsibleId
            });
            
            interview.Apply(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, supervisorId: responsibleId.FormatGuid()).Payload);
            commandValidator = Create.Service.InterviewAnswersCommandValidator(mockOfInterviewSummaryViewFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() => commandValidations.ForEach(validate =>
        {
            try
            {
                validate();
            }
            catch (InterviewException e)
            {
                exceptions.Add(e);
            }
        });

        [NUnit.Framework.Test] public void should_not_any_interview_exceptions () =>
            exceptions.Should().BeEmpty();

        static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid responsibleId = Guid.Parse("22222222222222222222222222222222");
        static readonly StatefulInterview interview = Create.AggregateRoot.StatefulInterview(interviewId, shouldBeInitialized: false);

        static readonly Action[] commandValidations =
        {
            () => commandValidator.Validate(interview, Create.Command.AnswerDateTimeQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerTextListQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerNumericRealQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerSingleOptionQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerSingleOptionLinkedQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerQRBarcodeQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerMultipleOptionsQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerMultipleOptionsLinkedQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerPictureQuestionCommand(interviewId: interviewId, userId: responsibleId)),
            () => commandValidator.Validate(interview, Create.Command.AnswerYesNoQuestion(interviewId: interviewId, userId: responsibleId, answer: new List<AnsweredYesNoOption>())),
            () => commandValidator.Validate(interview, Create.Command.AnswerGeoLocationQuestionCommand(interviewId: interviewId, userId: responsibleId))
        };
        static readonly List<InterviewException> exceptions = new List<InterviewException>();
        static InterviewAnswersCommandValidator commandValidator;
    }
}
