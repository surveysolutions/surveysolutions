using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_fixed_roster_edit_view_and_fixed_roster_inside_list_one : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
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
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetRosterEditView(questionnaireId, g3Id);

        It should_return_empty_grouped_list_of_multi_questions = () =>
            result.NotLinkedMultiOptionQuestions.Count.ShouldEqual(0);

        It should_return_empty_grouped_list_of_integer_questions = () =>
            result.NumericIntegerQuestions.Count.ShouldEqual(0);

        It should_return_empty_grouped_list_of_title_questions = () =>
            result.NumericIntegerTitles.Count.ShouldEqual(0);
       
        It should_return_grouped_list_of_integer_questions_with_one_pair = () =>
            result.TextListsQuestions.Count.ShouldEqual(4);

        It should_return_list_questions_at_3_with_id_equals_q1Id = () =>
            result.TextListsQuestions.ElementAt(3).Id.ShouldEqual(q1Id.FormatGuid());

        It should_return_list_questions_at_3_with_q1_title = () =>
            result.TextListsQuestions.ElementAt(3).Title.ShouldEqual(GetQuestion(q1Id).QuestionText);

        It should_return_list_questions_at_1_with_id_equals_q2Id = () =>
            result.TextListsQuestions.ElementAt(1).Id.ShouldEqual(q2Id.FormatGuid());

        It should_return_list_questions_at_1_with_q2_title = () =>
            result.TextListsQuestions.ElementAt(1).Title.ShouldEqual(GetQuestion(q2Id).QuestionText);

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
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
    }
}