using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_synchronized_data_contains_roster_with_static_text : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = interviewItemId.Id,
                    IsRoster = true,
                    RosterFixedTitles = new []{"a","b"},
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>
                    {
                        new StaticText(staticTextId, staticText)
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        interviewItemId, new[]
                        {
                            new RosterSynchronizationDto(interviewItemId.Id, new decimal[0], 0, null, "a"),
                            new RosterSynchronizationDto(interviewItemId.Id, new decimal[0], 1, null, "b")
                        }
                    }
                });
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
                interviewSynchronizationDto);

        It should_first_item_in_first_roster_row_be_type_of_static_text_view_model = () =>
            getStaticTextInRosterRow(0).ShouldNotBeNull();

        It should_static_text_id_in_first_roster_row_be_equal_to_specified_staticTextId = () =>
            getStaticTextInRosterRow(0).PublicKey.Id.ShouldEqual(staticTextId);

        It should_static_text_in_first_roster_row_be_equal_to_specified_staticText = () =>
            getStaticTextInRosterRow(0).Text.ShouldEqual(staticText);

        It should_first_item_in_second_roster_row_be_type_of_static_text_view_model = () =>
            getStaticTextInRosterRow(1).ShouldNotBeNull();

        It should_static_text_id_in_second_roster_row_be_equal_to_specified_staticTextId = () =>
            getStaticTextInRosterRow(1).PublicKey.Id.ShouldEqual(staticTextId);

        It should_static_text_in_second_roster_row_be_equal_to_specified_staticText = () =>
            getStaticTextInRosterRow(1).Text.ShouldEqual(staticText);

        private static StaticTextViewModel getStaticTextInRosterRow(int indexOfRow)
        {
            return
                ((QuestionnaireGridViewModel)(interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(interviewItemId.Id, interviewItemId.InterviewItemPropagationVector)])).Rows.ElementAt(indexOfRow).Items[0] as StaticTextViewModel;
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static InterviewItemId interviewItemId =
            new InterviewItemId(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);

        private static Guid staticTextId = Guid.Parse("22222222222222222222222222222222");
        private static string staticText = "static text";
    }
}
