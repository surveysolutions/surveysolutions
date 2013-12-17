using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModel
{
    internal class when_delete_roster_row_method_called : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var rosterGroup = new Group() { PublicKey = rosterId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId };
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                rosterGroup);

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, Dictionary<decimal, int?>>()
                {
                    { new InterviewItemId(rosterId, new decimal[0]), new Dictionary<decimal, int?> { { 0, null } } }
                });

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.RemovePropagatedScreen(rosterId, new decimal[0], 0);

        It should_contain_screen_with_added_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldNotContain(new InterviewItemId(rosterId, new decimal[] { 0 }));

        private static WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
    }
}
