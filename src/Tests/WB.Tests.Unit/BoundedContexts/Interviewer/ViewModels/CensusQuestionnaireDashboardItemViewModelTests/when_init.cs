using System;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.CensusQuestionnaireDashboardItemViewModelTests
{
    internal class when_init : CensusQuestionnaireDashboardItemViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewViewRepository = Mock.Of<IPlainStorage<InterviewView>>(
                x => x.Count(Moq.It.IsAny<Expression<Func<InterviewView, bool>>>()) ==
                     interviewsByQuestionnaireCount);

            viewModel = CreateCensusQuestionnaireDashboardItemViewModel(interviewViewRepository: interviewViewRepository);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init(questionnaireView);

        [NUnit.Framework.Test] public void should_view_model_have_specified_questionnaire_message () => viewModel.Title.Should().Be(string.Format(InterviewerUIResources.DashboardItem_Title, questionnaireView.Title, questionnaireView.GetIdentity().Version));
        [NUnit.Framework.Test] public void should_view_model_have_specified_comment () => viewModel.SubTitle.Should().Be(string.Format(InterviewerUIResources.DashboardItem_CensusModeComment, interviewsByQuestionnaireCount));

        static CensusQuestionnaireDashboardItemViewModel viewModel;
        const int interviewsByQuestionnaireCount = 3;

        private static readonly QuestionnaireView questionnaireView = new QuestionnaireView
        {
            Id = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1).ToString(),
            Title = "questionnaire title",
            Census = true
        };
    }
}
