using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    public class when_building_dashboard_item_for_interview_with_prefilled_questions : InterviewDashboardItemViewModelTestsContext
    {
        [Test]
        public void should_put_answers_for_each_interview()
        {
            Guid interviewId = Id.gA;
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var viewModel = GetViewModel();

            viewModel.Init(Create.Entity.InterviewView(interviewId: interviewId,
                   questionaireTitle: "Sample",
                    questionnaireId: questionnaireIdentity.ToString(),
                    status: InterviewStatus.InterviewerAssigned),
                new List<PrefilledQuestion>
                {
                    new PrefilledQuestion(){ Answer = "A", Question = "B"},
                    new PrefilledQuestion(){ Answer = "A1", Question = "B1"},
                    new PrefilledQuestion(){ Answer = "A2", Question = "B2"},
                    new PrefilledQuestion(){ Answer = "A3", Question = "B3"}
                });

            viewModel.IsExpanded = true;
            Assert.That(viewModel.PrefilledQuestions, Has.Count.EqualTo(4));
   
            viewModel.IsExpanded = false;
            Assert.That(viewModel.PrefilledQuestions, Has.Count.EqualTo(3), "should limit to 3 items in non expanded view");
        }
    }
}
