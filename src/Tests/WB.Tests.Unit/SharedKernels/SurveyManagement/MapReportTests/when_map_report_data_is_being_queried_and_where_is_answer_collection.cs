using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class when_map_report_data_is_being_queried_and_where_is_answer_collection : MapReportTestContext
    {
        Establish context = () =>
        {
            input = Mock.Of<MapReportInputModel>(x
                => x.Variable == "var"
                    && x.QuestionnaireId == Guid.Parse("11111111111111111111111111111111")
                    && x.QuestionnaireVersion == 1);

            var answersCollectionMock =
                Mock.Of<AnswersByVariableCollection>(x => x.Answers == new Dictionary<Guid, Dictionary<string, string>>
                {
                    {
                        interview1Id, new Dictionary<string, string>
                        {
                            { "0.5", "11.11;11.11" },
                            { "1.5", "22;22" },
                        }
                    },
                    {
                        interview2Id, new Dictionary<string, string>
                        {
                            { "#", "5555;66666" }
                        }
                    }

                });

            var repositoryReader = new Mock<IReadSideKeyValueStorage<AnswersByVariableCollection>>();

            repositoryReader.Setup(x => x.GetById(repositoryId)).Returns(answersCollectionMock);

            mapReport = CreateMapReport(repositoryReader.Object);
        };

        Because of = () =>
            view = mapReport.Load(input);

        It should_return_view_with_2_records_in_collection_of_points = () =>
            view.Points.Length.ShouldEqual(2);

        It should_interview_id_in_first_point_be_equal_to_interview1Id = () =>
            view.Points[0].InterviewId.ShouldEqual(interview1Id.ToString());

        It should_answers_in_first_point_be_specified_value = () =>
            view.Points[0].Answers.ShouldEqual("11.11;11.11|22;22");

        It should_interview_id_in_second_point_be_equal_to_interview2Id = () =>
            view.Points[1].InterviewId.ShouldEqual(interview2Id.ToString());

        It should_answers_in_second_point_be_specified_value = () =>
            view.Points[1].Answers.ShouldEqual("5555;66666");

        private static string repositoryId = "var-11111111111111111111111111111111-1";
        private static Guid interview1Id = Guid.Parse("11111111111111111111111111111111");
        private static Guid interview2Id = Guid.Parse("22222222222222222222222222222222");
        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}