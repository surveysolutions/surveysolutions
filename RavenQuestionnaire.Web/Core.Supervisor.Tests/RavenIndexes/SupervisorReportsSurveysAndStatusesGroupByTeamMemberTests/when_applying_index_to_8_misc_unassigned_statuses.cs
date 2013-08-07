using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.DenormalizerStorageItem;
using Machine.Specifications;
using Raven.Client.Embedded;

namespace Core.Supervisor.Tests.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMemberTests
{
    [Subject(typeof(Supervisor.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMember))]
    internal class when_applying_index_to_8_misc_unassigned_statuses : RavenIndexesTestContext
    {
        Establish context = () =>
        {
            IEnumerable<SummaryItem> generateStatisticDocumentsWithUnassignedCounts = GenerateStatisticDocuments(
                new SummaryItemSketch { Unassigned = 2, ResponsibleSupervisor = SupervisorA, Responsible = SupervisorA, Template = Template1 },
                new SummaryItemSketch { Unassigned = 0, ResponsibleSupervisor = SupervisorA, Responsible = InterviewerA1, Template = Template1 },
                new SummaryItemSketch { Unassigned = 0, ResponsibleSupervisor = SupervisorA, Responsible = InterviewerA2, Template = Template1 },
                new SummaryItemSketch { Unassigned = 3, ResponsibleSupervisor = SupervisorB, Responsible = SupervisorB, Template = Template1 },
                new SummaryItemSketch { Unassigned = 0, ResponsibleSupervisor = SupervisorB, Responsible = InterviewerB1, Template = Template1 },
                new SummaryItemSketch { Unassigned = 0, ResponsibleSupervisor = SupervisorB, Responsible = InterviewerB2, Template = Template1 }
            );

            documentStore = CreateDocumentStore(generateStatisticDocumentsWithUnassignedCounts);
        };

        Because of = () =>
            resultItems = QueryUsingIndex<SummaryItem>(documentStore, typeof(Supervisor.RavenIndexes.SupervisorReportsSurveysAndStatusesGroupByTeamMember));

        It should_return_4_line_items = () =>
            resultItems.Length.ShouldEqual(4);

        It should_return_1_line_item_for_SupervisorA_team = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Count()
                .ShouldEqual(1);

        It should_return_1_line_item_for_SupervisorB_team = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Count()
                .ShouldEqual(1);

        It should_set_0_to_Initial_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.InitialCount == 0);

        It should_set_0_to_Completed_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.CompletedCount == 0);

        It should_set_0_to_Approved_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.ApprovedCount == 0);

        It should_set_0_to_Errors_property_for_all_line_items = () =>
           resultItems.ShouldEachConformTo(lineItem => lineItem.CompletedWithErrorsCount == 0);

        It should_set_0_to_Redo_property_for_all_line_items = () =>
            resultItems.ShouldEachConformTo(lineItem => lineItem.RedoCount == 0);

        It should_set_10_to_Unassigned_property_for_sum_of_all_line_items = () =>
            resultItems.Sum(lineItem => lineItem.UnassignedCount).ShouldEqual(10);

        It should_group_each_Totals_to_be_equal_Unassigned_property_for_line_items = () =>
           resultItems.ShouldEachConformTo(lineItem => lineItem.TotalCount == lineItem.UnassignedCount);

        It should_set_10_to_Total_property_for_sum_of_all_line_items = () =>
            resultItems.Sum(lineItem => lineItem.TotalCount).ShouldEqual(10);
        
        It should_set_2_to_line_with_supervisorA_and_empty_responsible_for_Unassigned_property = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Single().UnassignedCount.ShouldEqual(2);

        It should_set_2_to_line_with_supervisorA_and_empty_responsible_for_Total_property = () =>
            GetLineItemForTeam(resultItems, SupervisorA).Single().TotalCount.ShouldEqual(2);

        It should_set_2_to_line_with_supervisorA_and_supervisorA_as_responsible_for_Unassigned_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, SupervisorA).Single().UnassignedCount.ShouldEqual(2);

        It should_set_2_to_line_with_supervisorA_and_supervisorA_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorA, SupervisorA).Single().TotalCount.ShouldEqual(2);

        It should_set_3_to_line_with_supervisorB_and_empty_responsible_for_Unassigned_property = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Single().UnassignedCount.ShouldEqual(3);

        It should_set_3_to_line_with_supervisorB_and_empty_responsible_for_Total_property = () =>
            GetLineItemForTeam(resultItems, SupervisorB).Single().TotalCount.ShouldEqual(3);

        It should_set_3_to_line_with_supervisorB_and_supervisorB_as_responsible_for_Unassigned_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, SupervisorB).Single().UnassignedCount.ShouldEqual(3);

        It should_set_3_to_line_with_supervisorB_and_supervisorB_as_responsible_for_Total_property = () =>
            GetLineItemForTeamMember(resultItems, SupervisorB, SupervisorB).Single().TotalCount.ShouldEqual(3);

        Cleanup stuff = () =>
        {
            documentStore.Dispose();
            documentStore = null;
        };

        private static EmbeddableDocumentStore documentStore;
        private static SummaryItem[] resultItems;
    }
}
