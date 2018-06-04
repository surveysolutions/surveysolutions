using System;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_creating_interview_with_no_featured_questions_but_with_variable : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventContext = new EventContext();

            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            variableId=Guid.NewGuid();
            questionnaireVersion = 18;

            questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.Variable(id: variableId, type: VariableType.Boolean, expression: "true"));

            interview = SetupStatefullInterviewWithExpressionStorageWithoutCreate(questionnaire);

            command = Create.Command.CreateInterview(Guid.Empty, userId, questionnaireIdentity, responsibleSupervisorId, null, null, null);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            eventContext = new EventContext();
            interview.CreateInterview(command);
        }

        [NUnit.Framework.Test] public void should_raise_InterviewCreated_event () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        [NUnit.Framework.Test] public void should_provide_questionnaire_id_in_InterviewCreated_event () =>
            eventContext.GetSingleEvent<InterviewCreated>().QuestionnaireId.Should().Be(questionnaireId);

        [NUnit.Framework.Test] public void should_provide_questionnaire_verstion_in_InterviewCreated_event () =>
            eventContext.GetSingleEvent<InterviewCreated>().QuestionnaireVersion.Should().Be(questionnaireVersion);

        [NUnit.Framework.Test] public void should_set_variable_value () =>
            eventContext.GetSingleEvent<VariablesChanged>().ChangedVariables[0].NewValue.Should().Be(true);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static long questionnaireVersion;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Guid variableId;
        private static StatefulInterview interview;
        private static QuestionnaireIdentity questionnaireIdentity;
        private static CreateInterview command;
    }
}
