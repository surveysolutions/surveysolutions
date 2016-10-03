using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_itself_in_enablement_condition : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(Create.Event.AddGroup(groupId: chapterId));
            questionnaire.AddQuestion(Create.Event.AddTextQuestion(questionId : questionToBeDeleted, parentId : chapterId));
            questionnaire.UpdateNumericQuestion(Create.Event.UpdateNumericIntegerQuestion(questionToBeDeleted, "q", enablementCondition: "q > 10"));
        };


        Because of = () => 
            questionnaire.DeleteQuestion(questionToBeDeleted, responsibleId);

        It should_doesnt_contain_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionToBeDeleted).ShouldBeNull();


        private static Questionnaire questionnaire;
        private static readonly Guid questionToBeDeleted = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid responsibleId= Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}