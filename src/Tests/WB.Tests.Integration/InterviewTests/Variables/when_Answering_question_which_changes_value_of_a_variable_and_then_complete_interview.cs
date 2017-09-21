using System;
using System.ComponentModel;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    [TestOf(typeof(StatefulInterview))]
    internal class when_Answering_question_which_changes_value_of_a_variable_and_then_complete_interview : InterviewTestsContext
    {
        [Test]
        public void should_not_fire_event_about_change_variable_on_complete()
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            var textQuetionId = Guid.Parse("21111111111111111111111111111111");
            var variableId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(textQuetionId, variable: "txt"),
                    Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "txt.Length")
                });

            var interview = SetupStatefullInterviewWithExpressionStorage(questionnaire);
            interview.AnswerTextQuestion(userId, textQuetionId, new decimal[0], DateTime.Now, "any string");

            var eventContext = new EventContext();

            // act
            interview.CompleteWithoutFirePassiveEvents(userId, String.Empty, DateTime.UtcNow);

            // assert
            eventContext.ShouldNotContainEvent<VariablesChanged>();

            eventContext.Dispose();
            eventContext = null;
        }
    }
}