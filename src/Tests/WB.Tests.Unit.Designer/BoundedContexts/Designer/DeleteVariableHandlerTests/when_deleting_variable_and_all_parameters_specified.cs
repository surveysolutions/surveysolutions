using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DeleteVariableHandlerTests
{
    internal class when_deleting_variable_and_all_parameters_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddVariable(entityId : entityId, parentId : chapterId ,responsibleId:responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>            
                questionnaire.DeleteVariable(entityId: entityId, responsibleId: responsibleId);


        [NUnit.Framework.Test] public void should_dont_contains_Variable () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).ShouldBeNull();

        
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}