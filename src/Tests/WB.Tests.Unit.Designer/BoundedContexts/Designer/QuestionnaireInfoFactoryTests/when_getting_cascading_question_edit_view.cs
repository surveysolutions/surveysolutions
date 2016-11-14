using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_cascading_question_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: docId, chapterId: g1Id, children: new IComposite[]
            {
                Create.Roster(rosterId: g2Id, title: "Roster", fixedRosterTitles: new[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2"), new FixedRosterTitle(3, "3")}, children: new List<IComposite>
                {
                    Create.NumericIntegerQuestion(q4Id, "int", title: "Integer 1"),
                    Create.SingleQuestion(q5Id, "linked_question", linkedToQuestionId: q4Id, title: "linked"),
                }),
                Create.SingleQuestion(q1Id, "list_question", title: "cascading_question"),
                Create.SingleQuestion(q2Id, "list_question", title: "cascading_question_2", cascadeFromQuestionId: q1Id),
                Create.SingleQuestion(q3Id, "list_question", title: "cascading_question_3", cascadeFromQuestionId: q2Id),
            });
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetQuestionEditView(questionnaireId, questionId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_grouped_list_of_single_questions_with_3_items = () =>
            result.SourceOfSingleQuestions.Count.ShouldEqual(3);

        It should_return_list_withfirst_placeholder_item = () =>
            result.SourceOfSingleQuestions.ElementAt(0).IsSectionPlaceHolder.ShouldBeTrue();

        It should_return_single_question_with_id__g1 = () =>
            result.SourceOfSingleQuestions.ElementAt(1).Id.ShouldEqual(q1Id.FormatGuid());

        It should_return_single_question_with_id__g2 = () =>
            result.SourceOfSingleQuestions.ElementAt(2).Id.ShouldEqual(q2Id.FormatGuid());

        private static QuestionnaireInfoFactory factory;
        private static NewEditQuestionView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid questionId = q3Id;
    }
}