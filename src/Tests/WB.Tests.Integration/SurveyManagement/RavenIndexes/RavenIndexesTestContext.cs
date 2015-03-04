using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Integration.SurveyManagement.RavenIndexes
{
    internal class RavenIndexesTestContext
    {
        protected class TestTemplate
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
        }

        protected class TestUser
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        protected class SummaryItemSketch
        {
            public int SupervisorAssignedCount { get; set; }
            public TestUser ResponsibleSupervisor { get; set; }
            public TestUser Responsible { get; set; }
            public TestTemplate Template { get; set; }
            public int Total
            {
                get { return this.SupervisorAssignedCount + this.InterviewerAssignedCount + this.Complete + this.ApprovedBySupervisorCount + this.Error + this.Redo; }
            }

            public int InterviewerAssignedCount { get; set; }

            public int Complete { get; set; }

            public int ApprovedBySupervisorCount { get; set; }

            public int Error { get; set; }

            public int Redo { get; set; }
        }

        protected static readonly TestUser SupervisorA = new TestUser { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "supervisorA" };
        protected static readonly TestUser SupervisorB = new TestUser { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "supervisorB" };
        protected static readonly TestUser InterviewerA1 = new TestUser { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "intrviewerA1" };
        protected static readonly TestUser InterviewerA2 = new TestUser { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "intrviewerA2" };
        protected static readonly TestUser InterviewerB1 = new TestUser { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "intrviewerB1" };
        protected static readonly TestUser InterviewerB2 = new TestUser { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "intrviewerB2" };
        protected static readonly TestTemplate Template1 = new TestTemplate { Id = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), Title = "The first template" };
        protected static readonly TestTemplate Template2 = new TestTemplate { Id = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"), Title = "The second template" };

        protected static EmbeddableDocumentStore CreateDocumentStore(IEnumerable<InterviewSummary> documents = null, IEnumerable<QuestionnaireBrowseItem> questionnaireBrowseItems = null)
        {
            var documentStore = new EmbeddableDocumentStore
                {
                    Configuration =
                        {
                            RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
                            RunInMemory = true
                        }
                };

            documentStore.Initialize();

            // Create Default Index
            var defaultIndex = new RavenDocumentsByEntityName();
            defaultIndex.Execute(documentStore);

            // Create Custom Indexes
            new SupervisorReportsSurveysAndStatusesGroupByTeamMember().Execute(documentStore);
            new SupervisorReportsTeamMembersAndStatusesGroupByTeamMember().Execute(documentStore);

            new HeadquarterReportsSurveysAndStatusesGroupByTeam().Execute(documentStore);
            new HeadquarterReportsTeamsAndStatusesGroupByTeam().Execute(documentStore);

            new QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex().Execute(documentStore);

            // Insert Documents from Abstract Property
            using (var bulkInsert = documentStore.BulkInsert())
            {
                foreach (var document in (documents ?? new InterviewSummary[0]))
                {
                    bulkInsert.Store(document);
                }
           
                foreach (var document in (questionnaireBrowseItems ?? new QuestionnaireBrowseItem[0]))
                {
                    bulkInsert.Store(document);
                }
            }
            
            return documentStore;
        }

        protected static EmbeddableDocumentStore CreateDocumentStore<T>(IEnumerable<T> documents, AbstractIndexCreationTask task)
        {
            var documentStore = new EmbeddableDocumentStore
            {
                Configuration =
                {
                    RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
                    RunInMemory = true
                }
            };

            documentStore.Initialize();

            // Create Default Index
            var defaultIndex = new RavenDocumentsByEntityName();
            defaultIndex.Execute(documentStore);

            // Create Custom Indexes
            task.Execute(documentStore);

            // Insert Documents from Abstract Property
            using (var bulkInsert = documentStore.BulkInsert())
            {
                foreach (var document in documents)
                {
                    bulkInsert.Store(document);
                }
            }

            return documentStore;
        }

        protected static T[] QueryUsingIndex<T>(EmbeddableDocumentStore documentStore, Type indexType)
        {
            
                using (var session = documentStore.OpenSession())
                {
                    return session.Query<T>(indexType.Name)
                                  .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                                  .ToArray();
                }
            
        }

        private static InterviewSummary CreateInterviewSummaryInStatus(SummaryItemSketch summaryItemSketch, InterviewStatus status)
        {
            return new InterviewSummary()
            {
                InterviewId = Guid.NewGuid(),
                QuestionnaireId = summaryItemSketch.Template.Id,
                TeamLeadId = summaryItemSketch.ResponsibleSupervisor.Id,
                ResponsibleId = summaryItemSketch.Responsible.Id,
                Status = status
            };
        }

        protected static IEnumerable<InterviewSummary> InterviewSummaryDocuments(params SummaryItemSketch[] testSummaryItems)
        {
            var result = new List<InterviewSummary>();
            //new SummaryItemSketch { InterviewerAssignedCount = 2, ResponsibleSupervisor = SupervisorA, Responsible = SupervisorA, Template = Template1 }
            foreach (var summaryItemSketch in testSummaryItems)
            {
                for (int i = 0; i < summaryItemSketch.InterviewerAssignedCount; i++)
                {
                    result.Add(CreateInterviewSummaryInStatus(summaryItemSketch, InterviewStatus.InterviewerAssigned));
                }
                for (int i = 0; i < summaryItemSketch.Redo; i++)
                {
                    result.Add(CreateInterviewSummaryInStatus(summaryItemSketch, InterviewStatus.RejectedBySupervisor));
                }
                for (int i = 0; i < summaryItemSketch.Complete + summaryItemSketch.Error; i++)
                {
                    result.Add(CreateInterviewSummaryInStatus(summaryItemSketch, InterviewStatus.Completed));
                }
                for (int i = 0; i < summaryItemSketch.ApprovedBySupervisorCount; i++)
                {
                    result.Add(CreateInterviewSummaryInStatus(summaryItemSketch, InterviewStatus.ApprovedBySupervisor));
                }
                for (int i = 0; i < summaryItemSketch.SupervisorAssignedCount; i++)
                {
                    result.Add(CreateInterviewSummaryInStatus(summaryItemSketch, InterviewStatus.SupervisorAssigned));
                }
            }
            return result;
        }

        protected static IEnumerable<StatisticsLineGroupedByUserAndTemplate> GetLineItemForTeam(IEnumerable<StatisticsLineGroupedByUserAndTemplate> lineItems, TestUser teamSupervisor)
        {
            return lineItems.Where(lineItem => lineItem.TeamLeadId == teamSupervisor.Id && lineItem.ResponsibleId == Guid.Empty);
        }
        protected static IEnumerable<StatisticsLineGroupedByUserAndTemplate> GetLineItemForTeamMember(IEnumerable<StatisticsLineGroupedByUserAndTemplate> lineItems, TestUser teamSupervisor, TestUser teamMember)
        {
            return lineItems.Where(lineItem => lineItem.TeamLeadId == teamSupervisor.Id && lineItem.ResponsibleId == teamMember.Id);
        }
    }
}