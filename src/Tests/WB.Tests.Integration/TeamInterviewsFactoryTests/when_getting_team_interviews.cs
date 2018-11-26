using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.TeamInterviewsFactoryTests
{
    internal class when_getting_team_interviews : TeamInterviewsFactoryTestContext
    {
        [NUnit.Framework.Test] public void should_return_correct_total_count () {
            Guid responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var version = 1;

            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedBySupervisor, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: 2),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: Guid.NewGuid(), questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.ApprovedByHeadquarters, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.Completed, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
                Abc.Create.Entity.InterviewSummary(status: InterviewStatus.InterviewerAssigned, responsibleId: responsibleId, questionnaireId: questionnaireId, questionnaireVersion: version),
            };

            PostgreReadSideStorage<InterviewSummary> repository;
            PostgreReadSideStorage<QuestionAnswer> featuredQuestionAnswersReader;
            var reportFactory = CreateTeamInterviewsFactory(out repository, out featuredQuestionAnswersReader);

            interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid()));

            // Act
            var report = reportFactory.Load(new TeamInterviewsInputModel()
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = version,
            });

            // Assert
            report.TotalCount.Should().Be(3);
        }

    }
}
