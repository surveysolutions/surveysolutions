using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.DenormalizerStorageItem;
using Machine.Specifications;
using Raven.Client.Embedded;

namespace Core.Supervisor.Tests.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMemberTests
{
    [Subject(typeof(Supervisor.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMember))]
    internal class when_applying_index_to_8_misc_initial_statuses : RavenIndexesTestContext
    {
        Establish context = () =>
        {
            IEnumerable<SummaryItem> generateStatisticDocumentsWithUnassignedCounts = GenerateStatisticDocuments(
                new SummaryItemSketch { Initial = 2, ResponsibleSupervisor = SupervisorA, Responsible = SupervisorA, Template = Template1 },
                new SummaryItemSketch { Initial = 7, ResponsibleSupervisor = SupervisorA, Responsible = InterviewerA1, Template = Template1 },
                new SummaryItemSketch { Initial = 11, ResponsibleSupervisor = SupervisorA, Responsible = InterviewerA2, Template = Template1 },
                new SummaryItemSketch { Initial = 13, ResponsibleSupervisor = SupervisorB, Responsible = SupervisorB, Template = Template1 },
                new SummaryItemSketch { Initial = 23, ResponsibleSupervisor = SupervisorB, Responsible = InterviewerB1, Template = Template1 },
                new SummaryItemSketch { Initial = 0, ResponsibleSupervisor = SupervisorB, Responsible = InterviewerB2, Template = Template1 }
            );

            documentStore = CreateDocumentStore(generateStatisticDocumentsWithUnassignedCounts);
        };

        Because of = () =>
            resultItems = QueryUsingIndex<SummaryItem>(documentStore, typeof(Supervisor.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMember));

        It should_return_7_line_items = () =>
            resultItems.Length.ShouldEqual(7);
        
        It should_set_0_to_Unassigned_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.UnassignedCount == 0);

        It should_set_0_to_Completed_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.CompletedCount == 0);

        It should_set_0_to_Approved_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.ApprovedCount == 0);

        It should_set_0_to_Errors_property_for_all_line_items = () =>
           resultItems.ShouldEachConformTo(lineItem => lineItem.CompletedWithErrorsCount == 0);

        It should_set_0_to_Redo_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.RedoCount == 0);

        It should_set_2x56_to_Initial_property_for_sum_of_all_line_items = () =>
            resultItems.Sum(lineItem => lineItem.InitialCount).ShouldEqual(2*56);

        It should_group_each_Totals_to_be_equal_Initial_property_for_line_items = () =>
           resultItems.ShouldEachConformTo(lineItem => lineItem.TotalCount == lineItem.InitialCount);

        It should_set_2x56_to_Total_property_for_sum_of_all_line_items = () =>
            resultItems.Sum(lineItem => lineItem.TotalCount).ShouldEqual(56*2);
        
        // Team A

        It should_return_1_line_item_for_SupervisorA_team = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Count()
                .ShouldEqual(1);

        It should_set_20_to_line_with_supervisorA_and_empty_responsible_for_Unassigned_property = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Single().InitialCount.ShouldEqual(2+7+11);

        It should_set_20_to_line_with_supervisorA_and_empty_responsible_for_Total_property = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Single().TotalCount.ShouldEqual(2+7+11);

        It should_set_2_to_line_with_supervisorA_and_supervisorA_as_responsible_for_Initial_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, SupervisorA).Single().InitialCount.ShouldEqual(2);

        It should_set_7_to_line_with_supervisorA_and_interviewerA1_as_responsible_for_Initial_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, InterviewerA1).Single().InitialCount.ShouldEqual(7);

        It should_set_11_to_line_with_supervisorA_and_interviewerA2_as_responsible_for_Initial_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, InterviewerA2).Single().InitialCount.ShouldEqual(11);

        It should_set_2_to_line_with_supervisorA_and_supervisorA_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, SupervisorA).Single().TotalCount.ShouldEqual(2);

        It should_set_7_to_line_with_supervisorA_and_interviewerA1_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, InterviewerA1).Single().TotalCount.ShouldEqual(7);

        It should_set_11_to_line_with_supervisorA_and_interviewerA2_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, InterviewerA2).Single().TotalCount.ShouldEqual(11);

        // Team B

        It should_return_1_line_item_for_SupervisorB_team = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Count()
                .ShouldEqual(1);

        It should_set_36_to_line_with_supervisorB_and_empty_responsible_for_Initial_property = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Single().InitialCount.ShouldEqual(13+23+0);

        It should_set_36_to_line_with_supervisorB_and_empty_responsible_for_Total_property = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Single().TotalCount.ShouldEqual(13+23+0);

        It should_set_13_to_line_with_supervisorB_and_supervisorB_as_responsible_for_Initial_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, SupervisorB).Single().InitialCount.ShouldEqual(13);
        
        It should_set_23_to_line_with_supervisorB_and_interviewerB1_as_responsible_for_Initial_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, InterviewerB1).Single().InitialCount.ShouldEqual(23);

        It should_set_13_to_line_with_supervisorB_and_supervisorB_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, SupervisorB).Single().TotalCount.ShouldEqual(13);

        It should_set_23_to_line_with_supervisorB_and_interviewerB1_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, InterviewerB1).Single().TotalCount.ShouldEqual(23);

        It should_be_absent_line_with_supervisorB_and_InterviewerB2_as_responsible_because_of_all_properties_are_0 = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, InterviewerB2).Count().ShouldEqual(0);

        Cleanup stuff = () =>
        {
            documentStore.Dispose();
            documentStore = null;
        };

        private static EmbeddableDocumentStore documentStore;
        private static SummaryItem[] resultItems;
    }
}
