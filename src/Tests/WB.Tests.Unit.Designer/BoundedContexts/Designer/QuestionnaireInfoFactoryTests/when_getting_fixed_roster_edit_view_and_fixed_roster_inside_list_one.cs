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
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_fixed_roster_edit_view_and_fixed_roster_inside_list_one : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(children: new List<IComposite>
            {
                Create.Roster(rosterId: g2Id, title: "list_roster", variable:  "list_roster", rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: q1Id, children: new IComposite[]
                {
                    Create.Roster(rosterId: g3Id, title:  "list_roster_inside_list_roster", variable:"list_roster_inside_list_roster", rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: q2Id),
                    Create.TextListQuestion(q2Id, variable:"list_question", title: "list_question_inside_roster", maxAnswerCount: 16),
                }),
                Create.TextListQuestion(q1Id, variable:"list_question", title: "list_question"),
            });
            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetRosterEditView(questionnaireId, g3Id);

        [NUnit.Framework.Test] public void should_return_empty_grouped_list_of_multi_questions () =>
            result.NotLinkedMultiOptionQuestions.Count.Should().Be(0);

        [NUnit.Framework.Test] public void should_return_empty_grouped_list_of_integer_questions () =>
            result.NumericIntegerQuestions.Count.Should().Be(0);

        [NUnit.Framework.Test] public void should_return_empty_grouped_list_of_title_questions () =>
            result.NumericIntegerTitles.Count.Should().Be(0);
       
        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_questions_with_one_pair () =>
            result.TextListsQuestions.Count.Should().Be(4);

        [NUnit.Framework.Test] public void should_return_list_questions_at_3_with_id_equals_q1Id () =>
            result.TextListsQuestions.ElementAt(3).Id.Should().Be(q1Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_list_questions_at_3_with_q1_title () =>
            result.TextListsQuestions.ElementAt(3).Title.Should().Be(GetQuestion(q1Id).QuestionText);

        [NUnit.Framework.Test] public void should_return_list_questions_at_1_with_id_equals_q2Id () =>
            result.TextListsQuestions.ElementAt(1).Id.Should().Be(q2Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_list_questions_at_1_with_q2_title () =>
            result.TextListsQuestions.ElementAt(1).Title.Should().Be(GetQuestion(q2Id).QuestionText);

        private static IGroup GetGroup(Guid groupId)
        {
            return questionnaireView.Find<IGroup>(groupId);
        }

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
