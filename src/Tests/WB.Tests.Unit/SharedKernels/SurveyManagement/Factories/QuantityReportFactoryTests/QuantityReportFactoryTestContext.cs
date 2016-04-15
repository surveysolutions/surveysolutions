﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    [Subject(typeof(QuantityReportFactory))]
    internal class QuantityReportFactoryTestContext
    {
        protected static QuantityReportFactory CreateQuantityReportFactory(
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null,
            IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage=null)
        {
            return new QuantityReportFactory(
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatuses>>(), interviewStatusTimeSpansStorage??new TestInMemoryWriter<InterviewStatusTimeSpans>());
        }

        protected static QuantityByInterviewersReportInputModel CreateQuantityByInterviewersReportInputModel(
            string period = "d", Guid? supervisorId = null,
            DateTime? from = null, int columnCount=2)
        {
            return new QuantityByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = columnCount,
                From = from ?? new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new[] {InterviewExportedAction.ApprovedBySupervisor}
            };
        }

        protected static QuantityBySupervisorsReportInputModel CreateQuantityBySupervisorsReportInputModel(
         string period = "d")
        {
            return new QuantityBySupervisorsReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new[] { InterviewExportedAction.ApprovedBySupervisor }
            };
        }
    }
}
