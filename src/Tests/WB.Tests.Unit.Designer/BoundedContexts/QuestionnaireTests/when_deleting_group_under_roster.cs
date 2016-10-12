using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_group_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true);
           
            questionnaire.AddGroup(groupInsideRosterId,  rosterId, responsibleId: responsibleId);
        };

        Because of = () => questionnaire.DeleteGroup(groupInsideRosterId, responsibleId);

        It should_doesnt_contain_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupInsideRosterId).ShouldBeNull();

        
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupInsideRosterId = Guid.Parse("FFFEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}