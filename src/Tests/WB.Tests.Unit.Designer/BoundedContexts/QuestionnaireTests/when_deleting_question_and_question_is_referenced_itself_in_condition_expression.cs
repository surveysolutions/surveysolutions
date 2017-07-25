using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_itself_in_enablement_condition : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup( chapterId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionId : questionToBeDeleted, parentId : chapterId,responsibleId:responsibleId, variableName:"q", enablementCondition:"q != null");
            BecauseOf();
        }


        private void BecauseOf() => 
            questionnaire.DeleteQuestion(questionToBeDeleted, responsibleId);

        [NUnit.Framework.Test] public void should_doesnt_contain_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionToBeDeleted).ShouldBeNull();


        private static Questionnaire questionnaire;
        private static readonly Guid questionToBeDeleted = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid responsibleId= Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}