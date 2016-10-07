using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MoveVariableHandlerTests
{
    internal class when_moving_variable_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddVariable(entityId : entityId, parentId : chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = targetEntityId });
        };

        Because of = () =>            
                questionnaire.MoveVariable(entityId: entityId, responsibleId: responsibleId, targetEntityId: targetEntityId, targetIndex: targetIndex);


        It should_moved_IVariable_to_new_group_with_PublicKey_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).GetParent().PublicKey.ShouldEqual(targetEntityId);

        It should_moved_IVariable_to_new_group_with_TargetIndex_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).GetParent().Children[targetIndex].PublicKey.ShouldEqual(entityId);

        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static int targetIndex = 0;
        
    }
}