using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_creating_interview_with_no_featured_questions_but_with_variable : in_standalone_app_domain
    {
        Establish context = () =>
        {
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

            command = Create.Command.CreateInterview(Guid.Empty, userId, questionnaireIdentity, DateTime.Now, responsibleSupervisorId, null, null, null);
        };

        Because of = () =>
        {
            eventContext = new EventContext();
            interview.CreateInterview(command);
        };

        It should_raise_InterviewCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        It should_provide_questionnaire_id_in_InterviewCreated_event = () =>
            eventContext.GetSingleEvent<InterviewCreated>().QuestionnaireId.ShouldEqual(questionnaireId);

        It should_provide_questionnaire_verstion_in_InterviewCreated_event = () =>
            eventContext.GetSingleEvent<InterviewCreated>().QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        It should_set_variable_value = () =>
            eventContext.GetSingleEvent<VariablesChanged>().ChangedVariables[0].NewValue.ShouldEqual(true);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

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