using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class InitializeTests : QuestionnaireTestsContext
    {
        public void Initialize_When_sharedPersons_is_null_Then_does_not_throw_any_exception()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.Initialize(Guid.NewGuid(), document: null, sharedPersons: null);

            // assert
            Assert.DoesNotThrow(act);
        }
    }
}