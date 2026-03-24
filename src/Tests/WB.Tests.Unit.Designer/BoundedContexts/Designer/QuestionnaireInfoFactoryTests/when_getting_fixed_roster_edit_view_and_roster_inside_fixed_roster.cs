using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    // Regression test for: Source question does not show up in the dropdown in nested roster
    // https://github.com/surveysolutions/surveysolutions/issues/4235
    // Scenario:
    //   1. Fixed roster `roster1`
    //   2. Numeric integer question `num` inside `roster1`
    //   3. Fixed roster `roster2` inside `roster1` (the one being edited)
    // Expected: GetRosterEditView for `roster2` should include `num` in NumericIntegerQuestions
    internal class when_getting_fixed_roster_edit_view_and_roster_inside_fixed_roster : QuestionnaireInfoFactoryTestContext
    {
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid numId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid multiId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid listId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(children: new List<IComposite>
            {
                Create.FixedRoster(rosterId: roster1Id, variable: "roster1", title: "Roster 1", children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(numId, variable: "num", title: "Numeric Question"),
                    Create.MultyOptionsQuestion(multiId, variable: "multi", title: "Multi Question",
                        options: new List<Answer> { Create.Option(1, "1"), Create.Option(2, "2") }),
                    Create.TextListQuestion(listId, variable: "list", title: "List Question"),
                    Create.FixedRoster(rosterId: roster2Id, variable: "roster2", title: "Roster 2"),
                })
            });
            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetRosterEditView(questionnaireId, roster2Id);

        [NUnit.Framework.Test]
        public void should_return_not_null_view() =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test]
        public void should_return_grouped_list_of_integer_questions_containing_num() =>
            result.NumericIntegerQuestions.Should().Contain(x => x.Id == numId.FormatGuid());

        [NUnit.Framework.Test]
        public void should_return_grouped_list_of_integer_questions_with_num() =>
            result.NumericIntegerQuestions.Where(x => !x.IsSectionPlaceHolder).Should().HaveCount(1);

        [NUnit.Framework.Test]
        public void should_return_grouped_list_of_multi_option_questions_containing_multi() =>
            result.NotLinkedMultiOptionQuestions.Should().Contain(x => x.Id == multiId.FormatGuid());

        [NUnit.Framework.Test]
        public void should_return_grouped_list_of_text_list_questions_containing_list() =>
            result.TextListsQuestions.Should().Contain(x => x.Id == listId.FormatGuid());

        private static IQuestion GetQuestion(Guid questionId)
        {
            return questionnaireView.Find<IQuestion>(questionId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionDetailsReaderMock;
    }
}
