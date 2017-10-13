using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests
    {
        private static InterviewFactory CreateInterviewFactory(
            IPlainStorageAccessor<InterviewEntity> interviewEntitiesRepository = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ISessionProvider sessionProvider = null,
            IEntitySerializer<object> jsonSerializer = null,
            IQueryableReadSideRepositoryReader<InterviewEntity> interviewRepository = null,
            IRosterStructureService rosterStructureService = null)
            => new InterviewFactory(
                summaryRepository: interviewSummaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                sessionProvider: sessionProvider ?? Mock.Of<ISessionProvider>(),
                jsonSerializer: jsonSerializer ?? Mock.Of<IEntitySerializer<object>>());
        [Test]
        public void when_remove_flag_question_received_by_interviewer()
        {
            //arrange
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.RejectedBySupervisor,
                ReceivedByInterviewer = true
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, false));
            //assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }

        [Test]
        public void when_set_flag_question_received_by_interviewer()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var questionIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"),
                Create.RosterVector(1));

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.RejectedBySupervisor,
                ReceivedByInterviewer = true
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            var exception = Assert.Catch<InterviewException>(() => factory.SetFlagToQuestion(interviewId, questionIdentity, true));
            //assert
            Assert.That(exception, Is.Not.Null); 
            Assert.That(exception.Message, Is.EqualTo($"Can't modify Interview {interviewId} on server, because it received by interviewer."));
        }
    }
}