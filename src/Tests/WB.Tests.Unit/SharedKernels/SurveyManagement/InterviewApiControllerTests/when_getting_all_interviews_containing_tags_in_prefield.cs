using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_all_interviews_containing_tags_in_prefield : InterviewApiControllerTestsContext
    {
        private Establish context = () =>
        {
            var allInterviewsViewFactoryFactoryMock = new Mock<IAllInterviewsFactory>();
            var interviewSummary = new AllInterviewsView()
            {
                Items = new List<AllInterviewsViewItem>()
                {
                    new AllInterviewsViewItem()
                    {
                        InterviewId = interviewId,
                        FeaturedQuestions = new List<InterviewFeaturedQuestion>()
                        {
                            new InterviewFeaturedQuestion()
                            {
                                Question = titleWithTags
                            }
                        }
                    }
                }
            };

            allInterviewsViewFactoryFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<AllInterviewsInputModel>())).Returns(interviewSummary);

            controller = CreateController(allInterviewsViewFactory: allInterviewsViewFactoryFactoryMock.Object);
        };

        Because of = () =>
            viewModel = controller.AllInterviews(new DocumentListViewModel());

        It should_view_model_not_be_null = () =>
            viewModel.ShouldNotBeNull();


        It should_question_title_have_removed_tags = () =>
            viewModel.Items.SingleOrDefault(x => x.InterviewId == interviewId).FeaturedQuestions.First().Question.ShouldEqual("test");

        private static InterviewApiController controller;
        private static AllInterviewsView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");

        private static string titleWithTags = "<i>test</i>";
    }
}
