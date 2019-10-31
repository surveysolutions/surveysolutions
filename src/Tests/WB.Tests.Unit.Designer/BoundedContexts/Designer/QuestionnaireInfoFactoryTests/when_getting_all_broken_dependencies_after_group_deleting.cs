using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_all_broken_dependencies_after_group_deleting : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: docId, chapterId: g1Id, children: new IComposite[]
            {
                Create.Group(groupId: g2Id, title: "Chapter 1 / Group 1", children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(numericQuestionId, "q1", "q2 == \"aaaa\"", title:  "Integer 1"),
                    Create.SingleQuestion(q5Id, "qqqq", title:  "sINGLE 1"),
                    Create.MultyOptionsQuestion(q3Id, enablementCondition: "q2 == \"aaaa\"", title:  "MultiOption", options: new List<Answer>{ Create.Option(1, "1"), Create.Option(2, "2") })
                }),
                Create.Roster(rosterId:g3Id, title: "Chapter 1 / Group 2", children: new IComposite[]
                {
                    Create.TextQuestion(q2Id, text: "text title", validationConditions: new List<ValidationCondition> {new ValidationCondition { Expression = "q1 > 10" } }),
                    Create.SingleQuestion(q4Id, cascadeFromQuestionId: q5Id, title: "single title"),
                })
            });

            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(
                questionDetailsReaderMock.Object,
                expressionProcessor: Create.RoslynExpressionProcessor());
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetAllBrokenGroupDependencies(questionnaireId, groupId);

        [NUnit.Framework.Test] public void should_return_not_null_view () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_not_null_view1 () => result.Select(x => x.Id).Should().BeEquivalentTo(q2Id.FormatGuid(), q4Id.FormatGuid());

        private static QuestionnaireInfoFactory factory;
        private static List<QuestionnaireItemLink> result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
        private static Guid groupId = g2Id;
    }
}
