using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    internal class when_translating_propagating_properties_to_roster_properties_and_propagating_question_references_3_groups_by_3_triggers_and_2_other_groups_are_not_referenced : QuestionnaireDocumentUpgraderTestsContext
    {
        Establish context = () =>
        {
            firstReferencedGroupId = Guid.Parse("11111111111111111111111111111111");
            secondReferencedGroupId = Guid.Parse("22222222222222222222222222222222");
            thirdReferencedGroupId = Guid.Parse("33333333333333333333333333333333");
            firstNotReferencedGroupId = Guid.Parse("00000000000000001111111111111111");
            secondNotReferencedGroupId = Guid.Parse("00000000000000002222222222222222");

            initialDocument = CreateQuestionnaireDocument(
                CreateAutoPropagatingQuestion(triggers: new List<Guid>
                {
                    firstReferencedGroupId,
                    secondReferencedGroupId,
                    thirdReferencedGroupId,
                }),

                CreateGroup(groupId: firstReferencedGroupId, setup: g => { g.Propagated = Propagate.AutoPropagated; }),
                CreateGroup(groupId: secondReferencedGroupId, setup: g => { g.Propagated = Propagate.Propagated; }),
                CreateGroup(groupId: thirdReferencedGroupId, setup: g => { g.Propagated = Propagate.AutoPropagated; }),

                CreateGroup(groupId: firstNotReferencedGroupId, setup: g => { g.Propagated = Propagate.AutoPropagated; }),
                CreateGroup(groupId: secondNotReferencedGroupId, setup: g => { g.Propagated = Propagate.Propagated; })
            );

            upgrader = CreateQuestionnaireDocumentUpgrader();
        };

        Because of = () =>
            resultDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(initialDocument);

        It should_return_document_with_3_roster_groups = () =>
            resultDocument.GetAllGroups().Count(group => group.IsRoster).ShouldEqual(3);

        It should_return_document_with_2_not_roster_groups = () =>
            resultDocument.GetAllGroups().Count(group => !group.IsRoster).ShouldEqual(2);

        It should_make_first_referenced_group_roster = () =>
            resultDocument.GetGroup(firstReferencedGroupId).IsRoster.ShouldBeTrue();

        It should_make_second_referenced_group_roster = () =>
            resultDocument.GetGroup(secondReferencedGroupId).IsRoster.ShouldBeTrue();

        It should_make_third_referenced_group_roster = () =>
            resultDocument.GetGroup(thirdReferencedGroupId).IsRoster.ShouldBeTrue();

        It should_make_first_not_referenced_group_not_roster = () =>
            resultDocument.GetGroup(firstNotReferencedGroupId).IsRoster.ShouldBeFalse();

        It should_make_second_not_referenced_group_not_roster = () =>
            resultDocument.GetGroup(secondNotReferencedGroupId).IsRoster.ShouldBeFalse();

        It should_mark_first_roster_group_as_not_propagated = () =>
            resultDocument.GetGroup(firstReferencedGroupId).Propagated.ShouldEqual(Propagate.None);

        It should_make_second_roster_group_as_not_propagated = () =>
            resultDocument.GetGroup(secondReferencedGroupId).Propagated.ShouldEqual(Propagate.None);

        It should_make_third_roster_group_as_not_propagated = () =>
            resultDocument.GetGroup(thirdReferencedGroupId).Propagated.ShouldEqual(Propagate.None);

        It should_make_first_not_referenced_group_as_not_propagated = () =>
            resultDocument.GetGroup(firstNotReferencedGroupId).Propagated.ShouldEqual(Propagate.None);

        It should_make_second_not_referenced_group_as_not_propagated = () =>
            resultDocument.GetGroup(secondNotReferencedGroupId).Propagated.ShouldEqual(Propagate.None);


        private static QuestionnaireDocument resultDocument;
        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument initialDocument;
        private static Guid firstReferencedGroupId;
        private static Guid secondReferencedGroupId;
        private static Guid thirdReferencedGroupId;
        private static Guid firstNotReferencedGroupId;
        private static Guid secondNotReferencedGroupId;
    }
}