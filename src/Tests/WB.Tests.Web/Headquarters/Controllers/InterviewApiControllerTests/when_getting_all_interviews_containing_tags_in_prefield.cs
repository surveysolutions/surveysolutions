using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers.Api;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewApiControllerTests
{
    internal class when_getting_all_interviews_containing_tags_in_prefilled : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
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

            allInterviewsViewFactoryFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<AllInterviewsInputModel>()))
                .Returns(interviewSummary);

            controller = CreateController(allInterviewsViewFactory: allInterviewsViewFactoryFactoryMock.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel = controller.Interviews(new InterviewsDataTableRequest()
                {Search = new DataTableRequest.SearchInfo()});

        [NUnit.Framework.Test]
        public void should_view_model_not_be_null() =>
            viewModel.Should().NotBeNull();


        [NUnit.Framework.Test]
        public void should_question_title_have_removed_tags() =>
            viewModel.Data.SingleOrDefault(x => x.InterviewId == interviewId).FeaturedQuestions.First().Question
                .Should().Be("test &gt; 1");

        private static InterviewApiController controller;
        private static InterviewsDataTableResponse viewModel;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");

        private static string titleWithTags = "<i>test > 1</i>";
    }
}
