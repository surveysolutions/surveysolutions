using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_applying_VariablesChanged_event_for_deleted_variable : InterviewTestsContext
    {
        [Test]
        public void should_not_throw_null_reference_exception()
        {
            Guid rosterId = Guid.Parse("44444444444444444444444444444444");
            Guid numericId = Guid.Parse("55555555555555555555555555555555");
            Guid variableId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(numericId),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: numericId, children: new[]
                {
                    Create.Entity.Variable(variableId)
                })
            });

            var interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);

            Assert.DoesNotThrow(() => interview.Apply(Create.Event.VariablesChanged(
                Create.Entity.ChangedVariable(Identity.Create(variableId, Create.Entity.RosterVector(0)), 1))));
        }
    }
}
