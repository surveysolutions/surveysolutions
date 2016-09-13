using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.CensusQuestionnaireDashboardItemViewModelTests
{
    internal class when_init : CensusQuestionnaireDashboardItemViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewViewRepository = Mock.Of<IPlainStorage<InterviewView>>(
                x => x.Count(Moq.It.IsAny<Expression<Func<InterviewView, bool>>>()) ==
                     interviewsByQuestionnaireCount);

            viewModel = CreateCensusQuestionnaireDashboardItemViewModel(interviewViewRepository: interviewViewRepository);
        };

        Because of = () => viewModel.Init(questionnaireView);

        It should_view_model_have_specified_questionnaire_message = () => viewModel.QuestionnaireName.ShouldEqual(string.Format(InterviewerUIResources.DashboardItem_Title, questionnaireView.Title, questionnaireView.GetIdentity().Version));
        It should_view_model_have_specified_comment = () => viewModel.Comment.ShouldEqual(string.Format(InterviewerUIResources.DashboardItem_CensusModeComment, interviewsByQuestionnaireCount));

        static CensusQuestionnaireDashboardItemViewModel viewModel;
        const int interviewsByQuestionnaireCount = 3;
        static readonly QuestionnaireView questionnaireView = new QuestionnaireView
        {
            Id = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1).ToString(),
            Title = "questionnaire title",
            Census = true
        };
    }
}
