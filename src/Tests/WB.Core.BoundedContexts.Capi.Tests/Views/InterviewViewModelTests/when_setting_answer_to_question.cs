using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_setting_answer_to_mandatory_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            notAnsweredMandatoryQuestionId = Guid.Parse("11111111111111111111111111111111");
            mandatoryQuestionToBeAnsweredId = Guid.Parse("33333333333333333333333333333333");


            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = mandatoryQuestionToBeAnsweredId,
                    Mandatory = true,
                    QuestionType = QuestionType.Numeric
                },
                new NumericQuestion()
                {
                    PublicKey = notAnsweredMandatoryQuestionId,
                    Mandatory = true,
                    QuestionType = QuestionType.Numeric
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(questionnarie.PublicKey, new decimal[0]),
                        new RosterSynchronizationDto[] { }
                    }
                });
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);

            mandatoryQuestionToBeAnsweredItemId = new InterviewItemId(mandatoryQuestionToBeAnsweredId, new decimal[] { });
            notAnsweredMandatoryQuestionItemId = new InterviewItemId(notAnsweredMandatoryQuestionId, new decimal[] { });
        };

        Because of = () =>
            interviewViewModel.SetAnswer(mandatoryQuestionToBeAnsweredItemId, 1);

        private It should_answered_question_has_IsMandatoryAndEmpty_property_value_false = () =>
            interviewViewModel.FindQuestion(q => q.PublicKey == mandatoryQuestionToBeAnsweredItemId).SingleOrDefault().IsMandatoryAndEmpty.ShouldEqual(false);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid mandatoryQuestionToBeAnsweredId;
        private static Guid notAnsweredMandatoryQuestionId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static InterviewItemId mandatoryQuestionToBeAnsweredItemId;
        private static InterviewItemId notAnsweredMandatoryQuestionItemId;
    }
}
