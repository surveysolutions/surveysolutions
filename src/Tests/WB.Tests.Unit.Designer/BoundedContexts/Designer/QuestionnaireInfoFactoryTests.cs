using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(QuestionnaireInfoFactory))]
    internal class QuestionnaireInfoFactoryTest
    {
        [Test]
        public void when_getting_cascading_question_edit_view_with_filtered_SourceOfSingleQuestions_should_return_view_with_SourceOfSingleQuestions_with_questions_with_same_roster_scope()
        {
            QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision(Id.g1);

            var rosterSizeId = Id.g2;
            var comboinrootId = Id.g3;
            var comboinsubsectionId = Id.g4;
            var comboinrosterId = Id.g5;
            var comboinsubsecofrosterId = Id.g6;
            var comboinnestedId = Id.g7;
            var comboinsubsecnestedId = Id.g8;
            var comboindiffnestedId = Id.g9;

            var questionnaireView = Create.QuestionnaireDocumentWithOneChapter(
                children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(rosterSizeId),
                    Create.SingleQuestion(comboinrootId),
                    Create.Group(children: new[]
                    {
                        Create.SingleQuestion(comboinsubsectionId)
                    }),
                    Create.Roster(rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeId,
                        children: new IComposite[]
                        {
                            Create.SingleQuestion(comboinrosterId),
                            Create.Group(children: new[]
                            {
                                Create.SingleQuestion(comboinsubsecofrosterId)
                            }),
                            Create.Roster(rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeId,
                                children: new IComposite[]
                                {
                                    Create.SingleQuestion(comboinnestedId),
                                    Create.Group(children: new[]
                                    {
                                        Create.SingleQuestion(comboinsubsecnestedId)
                                    })
                                }),
                            Create.Roster(children: new[]
                            {
                                Create.SingleQuestion(comboindiffnestedId)
                            })
                        })
                });

            var questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            var factory = new QuestionnaireInfoFactory(questionDetailsReaderMock.Object, null);

            var result = factory.GetQuestionEditView(questionnaireId, comboinsubsecnestedId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.SourceOfSingleQuestions, Is.Not.Null);
            Assert.That(result.SourceOfSingleQuestions.Where(x => x.Type == "singleoption").Select(x => x.Id),
                Is.EquivalentTo(new[]
                {
                    comboinrootId, comboinsubsectionId, comboinrosterId, comboinsubsecofrosterId, comboinnestedId
                }.Select(x => x.ToString("N"))));
        }
    }
}
