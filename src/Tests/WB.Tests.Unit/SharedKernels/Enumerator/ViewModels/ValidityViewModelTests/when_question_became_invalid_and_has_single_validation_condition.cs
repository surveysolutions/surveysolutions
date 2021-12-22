using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.ValidityViewModelTests
{
    public class when_question_became_invalid_and_has_single_validation_condition
    {
        [NUnit.Framework.OneTimeSetUp] public void context () 
        {
            questionIdentity = Create.Entity.Identity(Guid.NewGuid(), RosterVector.Empty);
            QuestionnaireDocument questionnaire = 
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(questionId: questionIdentity.Id,
                        validationConditions: new List<ValidationCondition>
                {
                    new() {Expression = "validation 1", Message = "message 1"},
                }));
            
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "text answer"));
            interview.Apply(
                Create.Event.AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        questionIdentity,
                        new List<FailedValidationCondition>
                        {
                            new FailedValidationCondition(0)
                        }
                    }
                }));

            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(interview);

            viewModel = Create.ViewModel.ValidityViewModel(questionnaire: plainQuestionnaire,
                interviewRepository: statefulInterviewRepository,
                entityIdentity: questionIdentity);
            viewModel.Init("interviewid", questionIdentity, Create.Other.NavigationState());
            BecauseOf();
        }

        public void BecauseOf() 
        {
            viewModel.HandleAsync(
                Create.Event.AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        questionIdentity,
                        new List<FailedValidationCondition>
                        {
                            new FailedValidationCondition(0)
                        }
                    }
                }));
        }
        
        [NUnit.Framework.Test] public void should_show_single_error_message () => 
            viewModel.Error.ValidationErrors.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_show_error_message_without_index_postfix () => viewModel.Error.ValidationErrors.First()?.PlainText.Should().Be("message 1");

        static ValidityViewModel viewModel;
        static Identity questionIdentity;
    }
}
