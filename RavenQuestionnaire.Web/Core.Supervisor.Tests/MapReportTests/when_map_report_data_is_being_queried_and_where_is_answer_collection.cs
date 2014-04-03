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

namespace Core.Supervisor.Tests.MapReportTests
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
                        Guid.Parse("44444444444444444444444444444444"), new Dictionary<string, string>
                        {
                            { "0.5", "11.11;11.11" },
                            { "1.5", "22;22" },
                        }
                    },
                    {
                        Guid.Parse("55555555555555555555555555555555"), new Dictionary<string, string>
                        {
                            { "#", "5555;66666" }
                        }
                    }

                });

            var repositoryReader = new Mock<IReadSideRepositoryReader<AnswersByVariableCollection>>();

            repositoryReader.Setup(x => x.GetById(repositoryId)).Returns(answersCollectionMock);

            mapReport = CreateMapReport(repositoryReader.Object);
        };

        Because of = () =>
            view = mapReport.Load(input);

        It should_return_view_with_2_records_in_collection_of_abswers = () =>
            view.Answers.Length.ShouldEqual(2);

        private It should_set_concrete_values_in_views_Answers_property = () =>
            view.Answers.ShouldContainOnly(new[] { "11.11;11.11|22;22", "5555;66666" });

        private static string repositoryId = "var-11111111111111111111111111111111-1";
        private static MapReport mapReport;
        private static MapReportInputModel input;
        private static MapReportView view;
    }
}