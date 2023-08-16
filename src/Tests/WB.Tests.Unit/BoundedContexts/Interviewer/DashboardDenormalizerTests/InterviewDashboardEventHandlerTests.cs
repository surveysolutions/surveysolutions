using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests;

[TestOf(typeof(InterviewDashboardEventHandler))]
public class InterviewDashboardEventHandlerTests
{
    [Test]
    public void When_interview_created_with_identified_entities_Should_record_sort_index_for_prefilled_questions()
    {
        Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        Guid interviewId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        Guid prefilledQ1 = Guid.Parse("11111111111111111111111111111111");
        Guid prefilledQ2 = Guid.Parse("22222222222222222222222222222222");
        Guid prefilledQ3 = Guid.Parse("33333333333333333333333333333333");
        Guid prefilledQ4 = Guid.Parse("44444444444444444444444444444444");

        QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithCover(questionnaireId, 
            children: Create.Entity.Group(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: prefilledQ1, preFilled: true),
                Create.Entity.TextQuestion(questionId: prefilledQ2, preFilled: true),
                Create.Entity.TextQuestion(questionId: prefilledQ3, preFilled: true),
                Create.Entity.TextQuestion(questionId: prefilledQ4, preFilled: true)
            }));

        SqliteInmemoryStorage<PrefilledQuestionView> prefilledQuestionsStorage = new SqliteInmemoryStorage<PrefilledQuestionView>();

        var interviewViewStorage = new SqliteInmemoryStorage<InterviewView>();
        interviewViewStorage.Store(Create.Entity.InterviewView(interviewId: interviewId,
            questionnaireId: new QuestionnaireIdentity(questionnaireId, 1).ToString()));

        var denormalizer = Create.Service.DashboardDenormalizer(
            interviewViewRepository: interviewViewStorage,
            prefilledQuestions: prefilledQuestionsStorage, 
            questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

        var @event = Create.Event.InterviewOnClientCreated(questionnaireId, 1).ToPublishedEvent(interviewId);
        var @eventAnswer1 = Create.Event.TextQuestionAnswered(prefilledQ1, answer: "q1").ToPublishedEvent(interviewId);
        var @eventAnswer2 = Create.Event.TextQuestionAnswered(prefilledQ2, answer: "q2").ToPublishedEvent(interviewId);
        var @eventAnswer3 = Create.Event.TextQuestionAnswered(prefilledQ3, answer: "q3").ToPublishedEvent(interviewId);
        var @eventAnswer4 = Create.Event.TextQuestionAnswered(prefilledQ4, answer: "q4").ToPublishedEvent(interviewId);

        // Act
        denormalizer.Handle(@event);
        denormalizer.Handle(@eventAnswer4);
        denormalizer.Handle(@eventAnswer2);
        denormalizer.Handle(@eventAnswer3);
        denormalizer.Handle(@eventAnswer1);

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