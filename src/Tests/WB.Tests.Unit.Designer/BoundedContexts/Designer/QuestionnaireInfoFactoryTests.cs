using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestOf(typeof(QuestionnaireInfoFactory))]
    internal class QuestionnaireInfoFactoryTest
    {
        [Test]
        public void when_getting_cascading_question_edit_view_with_filtered_SourceOfSingleQuestions_should_return_view_with_SourceOfSingleQuestions_with_questions_with_same_roster_scope()
        {
            string questionnaireId = "11111111111111111111111111111111";
            var rosterSizeId = Guid.Parse("22222222222222222222222222222222");
            var comboinrootId = Guid.Parse("33333333333333333333333333333333");
            var comboinsubsectionId = Guid.Parse("44444444444444444444444444444444");
            var comboinrosterId = Guid.Parse("55555555555555555555555555555555");
            var comboinsubsecofrosterId = Guid.Parse("66666666666666666666666666666666");
            var comboinnestedId = Guid.Parse("77777777777777777777777777777777");
            var comboinsubsecnestedId = Guid.Parse("88888888888888888888888888888888");
            var comboindiffnestedId = Guid.Parse("99999999999999999999999999999999");

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

            var questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
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
