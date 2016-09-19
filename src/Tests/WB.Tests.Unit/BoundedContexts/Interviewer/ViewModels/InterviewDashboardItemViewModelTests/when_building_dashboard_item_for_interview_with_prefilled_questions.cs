using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    public class when_building_dashboard_item_for_interview_with_prefilled_questions : InterviewDashboardItemViewModelTestsContext
    {
        [Test]
        public void should_put_answers_for_each_interview()
        {
            Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var prefilledQuestions = new SqliteInmemoryStorage<PrefilledQuestionView>();
            prefilledQuestions.Store(Create.Entity.PrefilledQuestionView(interviewId: interviewId));
            prefilledQuestions.Store(Create.Entity.PrefilledQuestionView(interviewId: Guid.NewGuid()));

            IPlainStorage<QuestionnaireView> questionnaires =
                Mock.Of<IPlainStorage<QuestionnaireView>>(x => x.GetById(Moq.It.IsAny<string>()) == Create.Entity.QuestionnaireView(questionnaireIdentity));

            var viewModel = GetViewModel(prefilledQuestions: prefilledQuestions,
                questionnaireViewRepository: questionnaires);

            viewModel.Init(Create.Entity.InterviewView(interviewId: interviewId,
                questionnaireId: questionnaireIdentity.ToString(),
                status: InterviewStatus.InterviewerAssigned));

            Assert.That(viewModel.PrefilledQuestions.Count, Is.EqualTo(1));
        }
    }
}