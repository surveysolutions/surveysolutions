using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.SurveyAndStatusesTests
{
    [TestOf(typeof(SurveysAndStatusesReport))]
    internal class SvSurveysAndStatusesReportTests : ReportContext
    {
        [Test]
        public void When_database_is_empty_It_should_return_0_records()
        {
            var report = Sv.SurveyAndStatuses();

            // act
            var view = report.Load(new SurveysAndStatusesReportInputModel());

            //assert
            Assert.That(view.TotalCount, Is.EqualTo(0));
        }


        [Test]
        public void When_2_questionnaires_has_some_interviews()
        {
            Guid firstTeamLeadId = Guid.Parse("11111111111111111111111111111111");
            Guid secondTeamLeadId = Guid.Parse("22222222222222222222222222222222");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid questionnaire1Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.SupervisorAssigned),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.InterviewerAssigned),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedBySupervisor),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedBySupervisor),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedByHeadquarters),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedByHeadquarters),

                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaire1Id, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaire1Id, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaire1Id, status: InterviewStatus.Completed),
            };

            var report = Sv.SurveyAndStatuses(interviews);

            // act
            var view = report.Load(new SurveysAndStatusesReportInputModel
            {
                // this filter doesm't work in tests for some reasons
                TeamLeadName = firstTeamLeadId.FormatGuid(),
                Order = "CompletedCount ASC"
            });
            
            //assert
            Assert.That(view.TotalCount, Is.EqualTo(1));
            
            var firstLine = view.Items.First();

            Assert.That(firstLine.SupervisorAssignedCount, Is.EqualTo(1));
            Assert.That(firstLine.InterviewerAssignedCount, Is.EqualTo(1));
            Assert.That(firstLine.CompletedCount, Is.EqualTo(1));
            Assert.That(firstLine.ApprovedBySupervisorCount, Is.EqualTo(1));
            Assert.That(firstLine.RejectedBySupervisorCount, Is.EqualTo(1));
            Assert.That(firstLine.ApprovedByHeadquartersCount, Is.EqualTo(1));
            Assert.That(firstLine.RejectedByHeadquartersCount, Is.EqualTo(1));
        }

        [Test]
        public void when_questionnaire_id_not_provided_should_search_for_all_interviews_from_all_versions_grouped_by_questionnaire()
        {
            Guid questionnaireId = Id.gA;
            Guid questionnaire1Id = Id.gB;
            Guid teamLeadId = Id.gC;
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, teamLeadId: teamLeadId, status: InterviewStatus.Completed),
                Create.Entity.InterviewSummary(questionnaireId: questionnaire1Id, teamLeadId: teamLeadId, questionnaireVersion: 1, status: InterviewStatus.Completed),
                Create.Entity.InterviewSummary(questionnaireId: questionnaire1Id, teamLeadId: teamLeadId, questionnaireVersion: 2, status: InterviewStatus.Completed),
                
            };

            var report = Sv.SurveyAndStatuses(interviews);

            // Act
            var view = report.Load(new SurveysAndStatusesReportInputModel { Order = "CompletedCount ASC" });
            
            // Assert
            Assert.That(view.TotalCount, Is.EqualTo(2));
            
            var firstLine = view.Items.First();

            Assert.That(firstLine.CompletedCount, Is.EqualTo(1));
            
            var secondLine = view.Items.ToArray()[1];

            Assert.That(secondLine.CompletedCount, Is.EqualTo(2));
        }
    }
}

