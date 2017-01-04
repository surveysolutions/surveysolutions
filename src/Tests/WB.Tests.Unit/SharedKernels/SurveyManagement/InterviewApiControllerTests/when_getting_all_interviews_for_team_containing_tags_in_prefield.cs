using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_all_interviews_for_team_containing_tags_in_prefield : InterviewApiControllerTestsContext
    {
        private Establish context = () =>
        {
            var teamInterviewViewFactoryMock = new Mock<ITeamInterviewsFactory>();
            var interviewSummary = new TeamInterviewsView()
            {
                Items = new List<TeamInterviewsViewItem>()
                {
                    new TeamInterviewsViewItem()
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

            teamInterviewViewFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<TeamInterviewsInputModel>())).Returns(interviewSummary);

            var globalInfoProvider =
                Mock.Of<IGlobalInfoProvider>(
                    g => g.GetCurrentUser() == new UserLight() { Id = Guid.Parse("A1111111111111111111111111111111") });

            controller = CreateController(teamInterviewViewFactory: teamInterviewViewFactoryMock.Object, 
                globalInfoProvider : globalInfoProvider);
        };

        Because of = () =>
            viewModel = controller.TeamInterviews(new DocumentListViewModel());

        It should_view_model_not_be_null = () =>
            viewModel.ShouldNotBeNull();

        It should_question_title_have_removed_tags = () =>
            viewModel.Items.SingleOrDefault(x => x.InterviewId == interviewId).FeaturedQuestions.First().Question.ShouldEqual("test");

        private static InterviewApiController controller;
        private static TeamInterviewsView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string titleWithTags = "<i>test</i>";
    }
}

