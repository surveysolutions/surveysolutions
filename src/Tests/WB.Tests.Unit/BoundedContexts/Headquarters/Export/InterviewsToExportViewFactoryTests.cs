using System;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Export
{
    [TestOf(typeof(InterviewsToExportViewFactory))]
    public class InterviewsToExportViewFactoryTests
    {
        [Test]
        public void should_return_interviews_by_questionnaire()
        {
            var interviewSummaries = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gA, questionnaireId: Id.g1, questionnaireVersion: 1), Id.gA);
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gB, questionnaireId: Id.g2, questionnaireVersion: 1), Id.gB);

            var viewFactory =
                Create.Service.InterviewsToExportViewFactory(
                    interviewSummaries);

            // Act
            var interviews = viewFactory.GetInterviewsToExport(new QuestionnaireIdentity(Id.g1, 1), null, null, null);

            // Assert
            Assert.That(interviews, Has.Count.EqualTo(1));
            Assert.That(interviews[0].Id, Is.EqualTo(Id.gA));
        }

        [Test]
        public void should_return_interviews_by_status()
        {
            var interviewSummaries = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gA, questionnaireId: Id.g1, questionnaireVersion: 1, status: InterviewStatus.Completed), Id.gA);
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gB, questionnaireId: Id.g1, questionnaireVersion: 1, status: InterviewStatus.RejectedByHeadquarters), Id.gB);

            var viewFactory =
                Create.Service.InterviewsToExportViewFactory(
                    interviewSummaries);

            // Act
            var interviews = viewFactory.GetInterviewsToExport(new QuestionnaireIdentity(Id.g1, 1), InterviewStatus.Completed, null, null);

            // Assert
            Assert.That(interviews, Has.Count.EqualTo(1));
            Assert.That(interviews[0].Id, Is.EqualTo(Id.gA));
        }

        
        [Test]
        public void should_return_interviews_by_dates()
        {
            var date = new DateTime(2018, 5, 12);

            var interviewSummaries = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gA, questionnaireId: Id.g1, questionnaireVersion: 1, updateDate: date), Id.gA);
            interviewSummaries.Store(Create.Entity.InterviewSummary(Id.gB, questionnaireId: Id.g1, questionnaireVersion: 1, updateDate: date.AddDays(7)), Id.gB);

            var viewFactory =
                Create.Service.InterviewsToExportViewFactory(
                    interviewSummaries);

            // Act
            var interviews = viewFactory.GetInterviewsToExport(new QuestionnaireIdentity(Id.g1, 1), null, date.AddDays(-1), date.AddDays(1));

            // Assert
            Assert.That(interviews, Has.Count.EqualTo(1));
            Assert.That(interviews[0].Id, Is.EqualTo(Id.gA));
        }
    }
}
