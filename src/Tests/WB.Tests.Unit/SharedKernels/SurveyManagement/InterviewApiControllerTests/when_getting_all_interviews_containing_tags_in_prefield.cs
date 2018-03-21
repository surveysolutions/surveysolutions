using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_all_interviews_containing_tags_in_prefield : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel = controller.AllInterviews(new DocumentListViewModel());

        [NUnit.Framework.Test] public void should_view_model_not_be_null () =>
            viewModel.Should().NotBeNull();


        [NUnit.Framework.Test] public void should_question_title_have_removed_tags () =>
            viewModel.Items.SingleOrDefault(x => x.InterviewId == interviewId).FeaturedQuestions.First().Question.Should().Be("test &gt; 1");

        private static InterviewApiController controller;
        private static AllInterviewsView viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");

        private static string titleWithTags = "<i>test > 1</i>";
    }
}
