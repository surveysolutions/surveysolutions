using System;
using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.RavenIndexes;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Core.Supervisor.Tests.RavenIndexes
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
            public int Unassigned { get; set; }
            public TestUser ResponsibleSupervisor { get; set; }
            public TestUser Responsible { get; set; }
            public TestTemplate Template { get; set; }
            public int Total
            {
                get { return this.Unassigned + this.Initial + this.Complete + this.Approved + this.Error + this.Redo; }
            }

            public int Initial { get; set; }

            public int Complete { get; set; }

            public int Approved { get; set; }

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

        protected static EmbeddableDocumentStore CreateDocumentStore(IEnumerable<SummaryItem> documents)
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

        protected static IEnumerable<SummaryItem> GenerateStatisticDocuments(params SummaryItemSketch[] testSummaryItems)
        {
            return testSummaryItems.Select(x => new SummaryItem()
                {
                    TotalCount = x.Total,
                    UnassignedCount = x.Unassigned,
                    InitialCount = x.Initial,
                    CompletedCount = x.Complete,
                    RedoCount = x.Redo,
                    CompletedWithErrorsCount = x.Error,
                    ApprovedCount = x.Approved,
                    DeletedInterviews = new HashSet<Guid>(),
                    ResponsibleId = x.Responsible.Id,
                    ResponsibleName = x.Responsible.Name,
                    ResponsibleSupervisorId = x.ResponsibleSupervisor.Id,
                    ResponsibleSupervisorName = x.ResponsibleSupervisor.Name,
                    TemplateId = x.Template.Id,
                    TemplateName = x.Template.Title
                });
        }

        protected static IEnumerable<SummaryItem> GetLineItemForTeam(IEnumerable<SummaryItem> lineItems, TestUser teamSupervisor)
        {
            return lineItems.Where(lineItem => lineItem.ResponsibleSupervisorId == teamSupervisor.Id && lineItem.ResponsibleId == Guid.Empty);
        }
        protected static IEnumerable<SummaryItem> GetLineItemForTeamMember(IEnumerable<SummaryItem> lineItems, TestUser teamSupervisor, TestUser teamMember)
        {
            return lineItems.Where(lineItem => lineItem.ResponsibleSupervisorId == teamSupervisor.Id && lineItem.ResponsibleId == teamMember.Id);
        }
    }
}