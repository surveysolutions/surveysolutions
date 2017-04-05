﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class when_map_report_data_is_being_queried : MapReportTestContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            long questionnaireVersion = 1;
            var variableName = "var";

            input = Mock.Of<MapReportInputModel>(x
                => x.Variable == variableName
                   && x.QuestionnaireId == questionnaireId
                   && x.QuestionnaireVersion == questionnaireVersion &&
                   x.NorthEastCornerLatitude == 80 &&
                   x.NorthEastCornerLongtitude == 175 &&
                   x.SouthWestCornerLatitude == -80 &&
                   x.SouthWestCornerLongtitude == 160);

            List<MapReportPoint> points = new List<MapReportPoint>();
            points.Add(new MapReportPoint("id1")
            {
                Latitude = -75.11,
                Longitude = -171.21,
                InterviewId = interview1Id,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variableName
            });
            points.Add(new MapReportPoint("id2")
            {
                Latitude = 72.555,
                Longitude = 170.32,
                InterviewId = interview2Id,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variableName
            });
            
            var repositoryReader = new TestInMemoryWriter<MapReportPoint>();
            foreach (var mapReportPoint in points)
            {
                repositoryReader.Store(mapReportPoint, mapReportPoint.Id);
            }

            mapReport = CreateMapReport(repositoryReader);
        };

        Because of = () =>
            view = mapReport.Load(input);

        It should_return_view_with_2_records_in_collection_of_points = () =>
            view.Points.Length.ShouldEqual(1);

        It should_interview_id_in_second_point_be_equal_to_interview2Id = () =>
            view.Points[0].Id.ShouldEqual(interview2Id.ToString());

        It should_answers_in_second_point_be_specified_value = () =>
            view.Points[0].Answers.ShouldEqual("72.555;170.32");

        private static Guid interview1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid interview2Id = Guid.Parse("22222222222222222222222222222222");
        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}