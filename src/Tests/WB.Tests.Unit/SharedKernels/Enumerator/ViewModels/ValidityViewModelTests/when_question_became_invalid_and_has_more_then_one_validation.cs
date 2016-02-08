using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Test.Core;
using Machine.Specifications;
using Main.Core.Documents;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.ValidityViewModelTests
{
    [Ignore("Can't find a way to execute code in the InvokeOnMainThread context")]
    public class when_question_became_invalid_and_has_more_then_one_validation
    {
        Establish context = () =>
        {
            questionIdentity = Create.Identity(Guid.NewGuid(), RosterVector.Empty);
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(Create.Question(questionId: questionIdentity.Id,
                validationConditions: new List<ValidationCondition>
                {
                    new ValidationCondition {Expression = "validation 1", Message = "message 1"},
                    new ValidationCondition {Expression = "validation 2", Message = "message 2"}
                }));

            failedValidationConditions = new List<FailedValidationCondition>
            {
                new FailedValidationCondition(0),
                new FailedValidationCondition(1)
            };

            var plainQuestionnaire = Create.PlainQuestionnaire(questionnaire);

            var interview = Substitute.For<IStatefulInterview>();
            interview.GetFailedValidationConditions(questionIdentity)
                .Returns(failedValidationConditions);
            interview.IsValid(questionIdentity).Returns(false);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.Get(null).ReturnsForAnyArgs(interview);

            viewModel = Create.ViewModels.ValidityViewModel(questionnaire: plainQuestionnaire,
                interviewRepository: statefulInterviewRepository,
                entityIdentity: questionIdentity);
        };

        Because of = () =>
        {
            viewModel.Handle(
                Create.Event.AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    {
                        questionIdentity,
                        failedValidationConditions
                    }
                }));
        };

        It should_show_all_failed_validation_messages = () => viewModel.Error.ValidationErrors.Count.ShouldEqual(2);

        static ValidityViewModel viewModel;
        static Identity questionIdentity;
        static List<FailedValidationCondition> failedValidationConditions;
    }
}