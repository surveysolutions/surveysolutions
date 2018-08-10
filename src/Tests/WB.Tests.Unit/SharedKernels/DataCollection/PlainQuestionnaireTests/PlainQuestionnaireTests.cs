using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    internal class PlainQuestionnaireTests : PlainQuestionnaireTestsContext
    {
        [Test]
        public void when_getting_roster_id_by_variable_name_and_variable_name_is_null_should_return_null()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(variable: "group1"),
                Create.Entity.Group(variable: "group2")
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

            // act
            var rosterId = plainQuestionnaire.GetRosterIdByVariableName(null);

            //assert 
            Assert.That(rosterId, Is.Null);
        }
    }
}
