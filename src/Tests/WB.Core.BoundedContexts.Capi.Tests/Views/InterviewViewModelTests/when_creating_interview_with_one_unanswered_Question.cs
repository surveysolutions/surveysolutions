using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_creating_interview_with_one_unanswered_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            targetQuestionId = Guid.Parse("33333333333333333333333333333333");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = targetQuestionId,
                    QuestionType = QuestionType.Numeric
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

        It should_count_of_unansweredQuestions_in_statistic_be_equal_to_1 = () =>
           interviewViewModel.Statistics.UnansweredQuestions.Count.ShouldEqual(1);

        It should_unansweredQuestions_in_statistic_contain_targetQuestionId = () =>
          interviewViewModel.Statistics.UnansweredQuestions.Any(q => q.PublicKey == new InterviewItemId(targetQuestionId, new decimal[0])).ShouldBeTrue();

        It should_answeredQuestions_in_statistic_cont_has_zero_elements = () =>
           interviewViewModel.Statistics.AnsweredQuestions.ShouldBeEmpty();

        It should_invalidQuestions_in_statistic_be_empty = () =>
          interviewViewModel.Statistics.InvalidQuestions.ShouldBeEmpty();

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Guid targetQuestionId;
    }
}
