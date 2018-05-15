using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
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
                It.IsAny<int>(), It.IsAny<int>()
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