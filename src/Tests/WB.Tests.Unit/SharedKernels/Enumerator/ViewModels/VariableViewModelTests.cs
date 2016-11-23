using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(VariableViewModel))]
    public class VariableViewModelTests
    {
        [Test]
        public void When_init_and_variable_value_is_null_Then_text_should_contains_variable_name()
        {
            //arrange
            var variableIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.Variable(variableIdentity.Id));
            var interview = Setup.StatefulInterview(questionnaire);
            var viewModel = Create.ViewModel.VariableViewModel(
                questionnaire: Create.Entity.PlainQuestionnaire(questionnaire),
                interviewRepository: Create.Fake.StatefulInterviewRepositoryWith(interview));

            //act
            viewModel.Init("interviewid", variableIdentity, Create.Other.NavigationState());
            //assert
            viewModel.Text.ShouldEqual("v1 : <empty>");
        }
    }
}
