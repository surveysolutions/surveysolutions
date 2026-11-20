using System;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestFixture]
    [TestOf(typeof(TesterEventStore))]
    public class TesterInterviewReplayTests
    {
        [Test]
        public void when_replaying_an_old_answer_after_reload_and_cache_eviction_should_use_its_original_questionnaire()
        {
            var questionnaireId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            var interviewId = Guid.NewGuid();
            var oldDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: Guid.NewGuid(), children: new[] { Create.Entity.NumericIntegerQuestion(questionId) });
            var newDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            oldDocument.PublicKey = questionnaireId;
            newDocument.PublicKey = questionnaireId;
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var questionnaires = Create.Storage.QuestionnaireStorage(
                repository: new TesterQuestionnaireDocumentStorage(), memoryCache: cache);
            questionnaires.StoreQuestionnaire(questionnaireId, 1, oldDocument);

            var events = new TesterEventStore();
            var stream = new UncommittedEventStream(Guid.NewGuid(), null);
            stream.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 1, 0, DateTime.UtcNow,
                Create.Event.InterviewCreated(questionnaireId, 1)));
            stream.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 2, 0, DateTime.UtcNow,
                Create.Event.NumericIntegerQuestionAnswered(questionId: questionId, answer: 42)));
            events.Store(stream);

            questionnaires.StoreQuestionnaire(questionnaireId, 2, newDocument);
            cache.Compact(1);
            var interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireVersion: 1, questionnaireRepository: questionnaires, shouldBeInitialized: false);
            interview.InitializeFromHistory(interviewId, events.Read(interviewId, 0));

            interview.GetAnswerAsString(new Identity(questionId, RosterVector.Empty)).Should().Be("42");
            interview.QuestionnaireIdentity.Version.Should().Be(1);
        }
    }
}
