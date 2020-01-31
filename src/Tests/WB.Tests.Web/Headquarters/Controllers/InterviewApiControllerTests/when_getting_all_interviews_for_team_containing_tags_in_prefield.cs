using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers.Api;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewApiControllerTests
{
    internal class when_getting_all_interviews_for_team_containing_tags_in_prefield : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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

            var authorizedUser =
                Mock.Of<IAuthorizedUser>(g => g.Id == Guid.Parse("A1111111111111111111111111111111"));

            controller = CreateController(teamInterviewViewFactory: teamInterviewViewFactoryMock.Object, 
                authorizedUser : authorizedUser);
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel = controller.GetTeamInterviews(new InterviewsDataTableRequest() { Length = 10, Search = new DataTableRequest.SearchInfo() });

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_question_title_have_removed_tags () =>
            viewModel.Data.SingleOrDefault(x => x.InterviewId == interviewId).FeaturedQuestions.First().Question.Should().Be("test");

        private static InterviewApiController controller;
        private static TeamInterviewsDataTableResponse viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string titleWithTags = "<i>test</i>";
    }
}

