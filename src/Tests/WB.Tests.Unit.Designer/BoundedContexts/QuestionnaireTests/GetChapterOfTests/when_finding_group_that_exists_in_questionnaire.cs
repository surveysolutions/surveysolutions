using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.GetChapterOfTests
{
    [Subject(typeof(Questionnaire))]
    internal class when_finding_group_that_exists_in_questionnaire
    {
        Establish context = () =>
        {
            targetGroupId = Guid.NewGuid();
            chapterId = Guid.NewGuid();
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                chapterId,
                new Group
                {
                    Children = new List<IComposite>()
                    {
                        new Group("group") { PublicKey = targetGroupId }
                    }.ToReadOnlyCollection()
                },
                new Group("group") { PublicKey = Guid.NewGuid() }
            );
        };

        Because of = () => foundGroup = questionnaire.GetChapterOfItemById(targetGroupId);

        It should_find_needed_group = () => foundGroup.PublicKey.ShouldEqual(chapterId);

        private static QuestionnaireDocument questionnaire;
        private static Guid targetGroupId;
        private static IComposite foundGroup;
        private static Guid chapterId;
    }
}

