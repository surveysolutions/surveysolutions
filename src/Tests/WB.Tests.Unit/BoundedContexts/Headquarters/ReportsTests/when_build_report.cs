using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ReportsTests
{
    [TestFixture]
    public class when_build_report
    {
        private IInterviewReportDataRepository interviewReportDataRepository;
        private SurveyStatisticsReport Subject;

        [SetUp]
        public void Setup()
        {
            this.interviewReportDataRepository = Mock.Of<IInterviewReportDataRepository>();
            this.Subject = new SurveyStatisticsReport(interviewReportDataRepository);
        }

        [Test]
        public void should_be_possible_to_merge_report_views()
        {
            const string TeamLead = "TeamLead";
            const string Responsible = "Responsible";

            var rv1 = new ReportView
            {
                Columns = new[] { TeamLead, Responsible, "col1", "col2" },
                Headers = new[] { TeamLead, Responsible, "col1 header", "col2" },
                Totals = new object[] { "AllTeams", "AllRepos", 20, 30 },
                Data = new[]
                {
                    new object[] { "Vasya", "Pupkin",  5, 12 },
                    new object[] { "Vasya", "Tulkin", 15,  8 },
                    new object[] { "Ivan", "Pobeda",  15,  8 },
                    new object[] { "Roma", "Popugai", 15,  8 } // do not exists in rv2
                }
            };

            var rv2 = new ReportView
            {
                Columns = new[] { TeamLead, Responsible, "col3", "col4", },
                Headers = new[] { TeamLead, Responsible, "col3", "col4 header" },
                Totals = new object[] { "AllTeams", "AllRepos", 50, 70},
                Data = new[]
                {
                    new object[] { "Vasya", "Pupkin",  50, 120 },
                    new object[] { "Vasya", "Tulkin", 105, 80 },
                    new object[] { "Ivan" , "Pobeda", 150, 80 },
                    new object[] { "Petya", "Chocolade", 150,80 } // do not exists in rv1
                }
            };

            var result = rv1.Merge(rv2, TeamLead, Responsible);

            Assert.That(result.Columns, Is.EqualTo(new[] { TeamLead, Responsible, "col1", "col2", "col3", "col4" }));
            Assert.That(result.Headers, Is.EqualTo(new[] { TeamLead, Responsible, "col1 header", "col2", "col3", "col4 header" }));
            Assert.That(result.Totals, Is.EqualTo(new object[] { "AllTeams", "AllRepos", 20, 30, 50, 70 }));

            result.Data.AssertExactlyOneRowMatch("Vasya", "Pupkin",     5, 12,  50, 120);
            result.Data.AssertExactlyOneRowMatch("Vasya", "Tulkin",    15,  8, 105,  80);
            result.Data.AssertExactlyOneRowMatch("Ivan",  "Pobeda",    15,  8, 150,  80);
            result.Data.AssertExactlyOneRowMatch("Roma",  "Popugai",   15,  8,  0L,  0L);
            result.Data.AssertExactlyOneRowMatch("Petya", "Chocolade", 0L, 0L, 150,  80);
        }

        [Test]
        public void should_get_numeric_report_for_numeric_variable()
        {
            this.Subject.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Question = Mock.Of<IQuestion>(q =>
                    q.QuestionType == QuestionType.Numeric && q.Answers == new List<Answer>())
            });

            Mock.Get(this.interviewReportDataRepository).Verify(r => r.GetNumericalReportData(
                It.IsAny<QuestionnaireIdentity>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool>(),
                It.IsAny<long>(), It.IsAny<long>()
            ), Times.Once);
        }

        [Test]
        public void should_get_pivot_report_for_categorical_in_pivot()
        {
            this.Subject.GetReport(new SurveyStatisticsReportInputModel
            {
                Pivot = true,
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Question = Mock.Of<IQuestion>(q =>
                    q.QuestionType == QuestionType.SingleOption && q.Answers == new List<Answer>()),
                ConditionalQuestion = Mock.Of<IQuestion>(q =>
                    q.QuestionType == QuestionType.SingleOption && q.Answers == new List<Answer>())
            });

            Mock.Get(this.interviewReportDataRepository).Verify(r => r.GetCategoricalPivotData(
                It.IsAny<Guid?>(),
                It.IsAny<QuestionnaireIdentity>(),
                It.IsAny<Guid>(), It.IsAny<Guid>()
            ), Times.Once);
        }

        [Test]
        public void should_get_categorical_report_for_categorical()
        {
            this.Subject.GetReport(new SurveyStatisticsReportInputModel
            {
                Pivot = false,
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Question = Mock.Of<IQuestion>(q =>
                    q.QuestionType == QuestionType.SingleOption && q.Answers == new List<Answer>()),
                ConditionalQuestion = Mock.Of<IQuestion>(q =>
                    q.QuestionType == QuestionType.SingleOption && q.Answers == new List<Answer>())
            });

            Mock.Get(this.interviewReportDataRepository)
                .Verify(r => r.GetCategoricalReportData(
                    It.IsAny<GetCategoricalReportParams>()), Times.Once);
        }
    }
}
