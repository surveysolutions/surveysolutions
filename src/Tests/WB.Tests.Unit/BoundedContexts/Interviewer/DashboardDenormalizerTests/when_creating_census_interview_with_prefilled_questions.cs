using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    [TestOf(typeof(InterviewDashboardEventHandler))]
    internal class InterviewDashboardEventHandlerTest
    {
        [Test]
        public void When_census_interview_created_Should_record_sort_index_for_prefilled_questions()
        {
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid prefilledQ1 = Guid.Parse("11111111111111111111111111111111");
            Guid prefilledQ2 = Guid.Parse("22222222222222222222222222222222");
            Guid prefilledQ3 = Guid.Parse("33333333333333333333333333333333");
            Guid prefilledQ4 = Guid.Parse("44444444444444444444444444444444");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocument(questionnaireId, 
                Create.Entity.Group(children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: prefilledQ1, preFilled: true),
                    Create.Entity.TextQuestion(questionId: prefilledQ2, preFilled: true),
                    Create.Entity.TextQuestion(questionId: prefilledQ3, preFilled: true),
                    Create.Entity.TextQuestion(questionId: prefilledQ4, preFilled: true)
                }));

            SqliteInmemoryStorage<PrefilledQuestionView> prefilledQuestionsStorage = new SqliteInmemoryStorage<PrefilledQuestionView>();

            var @event = Create.Event.InterviewOnClientCreated(questionnaireId, 1).ToPublishedEvent();
            var denormalizer = Create.Service.DashboardDenormalizer(
                prefilledQuestions: prefilledQuestionsStorage, 
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            // Act
            denormalizer.Handle(@event);

            // Assert
            var allPrefilledQuestions = prefilledQuestionsStorage.LoadAll().OrderBy(x => x.SortIndex).ToList();

            Assert.That(allPrefilledQuestions.Count, Is.EqualTo(4));

            Assert.That(allPrefilledQuestions[0].SortIndex, Is.EqualTo(0));
            Assert.That(allPrefilledQuestions[0].QuestionId, Is.EqualTo(prefilledQ1));

            Assert.That(allPrefilledQuestions[1].SortIndex, Is.EqualTo(1));
            Assert.That(allPrefilledQuestions[1].QuestionId, Is.EqualTo(prefilledQ2));

            Assert.That(allPrefilledQuestions[2].SortIndex, Is.EqualTo(2));
            Assert.That(allPrefilledQuestions[2].QuestionId, Is.EqualTo(prefilledQ3));

            Assert.That(allPrefilledQuestions[3].SortIndex, Is.EqualTo(3));
            Assert.That(allPrefilledQuestions[3].QuestionId, Is.EqualTo(prefilledQ4));
        }
    }
}
