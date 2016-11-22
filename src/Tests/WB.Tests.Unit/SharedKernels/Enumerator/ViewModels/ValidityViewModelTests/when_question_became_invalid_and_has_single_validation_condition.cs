using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.ValidityViewModelTests
{
    [Subject(typeof(ValidityViewModel))]
    public class when_question_became_invalid_and_has_single_validation_condition
    {
        Establish context = () =>
        {
            questionIdentity = Create.Entity.Identity(Guid.NewGuid(), RosterVector.Empty);
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.Question(questionId: questionIdentity.Id,
                validationConditions: new List<ValidationCondition>
                {
                    new ValidationCondition {Expression = "validation 1", Message = "message 1"},
                }));
            
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);
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

            var statefulInterviewRepository = Setup.StatefulInterviewRepository(interview);

            viewModel = Create.ViewModel.ValidityViewModel(questionnaire: plainQuestionnaire,
                interviewRepository: statefulInterviewRepository,
                entityIdentity: questionIdentity);
            viewModel.Init("interviewid", questionIdentity);
        };

        Because of = () =>
        {
            viewModel.Handle(
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
        };

        It should_set_validation_caption = () => viewModel.Error.Caption.ShouldEqual(UIResources.Validity_Answered_Invalid_ErrorCaption);

        It should_show_single_error_message = () => viewModel.Error.ValidationErrors.Count.ShouldEqual(1);

        It should_show_error_message_without_index_postfix = () => viewModel.Error.ValidationErrors.First()?.PlainText.ShouldEqual("message 1");

        static ValidityViewModel viewModel;
        static Identity questionIdentity;
    }
}