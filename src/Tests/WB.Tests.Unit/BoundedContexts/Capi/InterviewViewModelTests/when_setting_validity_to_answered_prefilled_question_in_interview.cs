using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_setting_validity_to_answered_prefilled_question_in_interview : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            targetQuestionId = Guid.Parse("33333333333333333333333333333333");
            QuestionnaireDocument questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = targetQuestionId,
                    QuestionType = QuestionType.Numeric,
                    QuestionScope = QuestionScope.Headquarter,
                    Featured = true
                });

            QuestionnaireRosterStructure rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            InterviewSynchronizationDto interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            questionIdentity = ConversionHelper.ConvertIdAndRosterVectorToString(targetQuestionId, Empty.RosterVector);
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
            interviewViewModel.SetAnswer(questionIdentity, 3);
        };

        Because of = () =>
            interviewViewModel.SetQuestionValidity(questionIdentity, true);

        It should_mark_question_as_invalid = () =>
            interviewViewModel.FeaturedQuestions[questionIdentity].Status.HasFlag(QuestionStatus.Valid).ShouldBeTrue();

        private static InterviewViewModel interviewViewModel;
        private static Guid targetQuestionId;
        private static string questionIdentity;
    }
}