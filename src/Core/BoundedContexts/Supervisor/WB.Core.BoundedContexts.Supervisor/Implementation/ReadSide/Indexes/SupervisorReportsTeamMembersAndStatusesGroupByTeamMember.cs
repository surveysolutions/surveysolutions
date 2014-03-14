using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide.Indexes
{
    public class SupervisorReportsTeamMembersAndStatusesGroupByTeamMember : AbstractMultiMapIndexCreationTask<StatisticsLineGroupedByUserAndTemplate>
    {
        public SupervisorReportsTeamMembersAndStatusesGroupByTeamMember()
        {
            this.AddMap<StatisticsLineGroupedByUserAndTemplate>(docs => from doc in docs
                                                                   where doc.TeamLeadId != null
                                                                   select new StatisticsLineGroupedByUserAndTemplate
                                                                   {
                                                                       ResponsibleId = doc.ResponsibleId,
                                                                       ResponsibleName = doc.ResponsibleName,

                                                                       TeamLeadId = doc.TeamLeadId,
                                                                       TeamLeadName = doc.TeamLeadName,

                                                                       QuestionnaireId = doc.QuestionnaireId,
                                                                       QuestionnaireVersion = doc.QuestionnaireVersion,
                                                                       QuestionnaireTitle = doc.QuestionnaireTitle,

                                                                       CreatedCount = doc.CreatedCount,
                                                                       SupervisorAssignedCount = doc.SupervisorAssignedCount,
                                                                       InterviewerAssignedCount = doc.InterviewerAssignedCount,
                                                                       SentToCapiCount = doc.SentToCapiCount,
                                                                       CompletedCount = doc.CompletedCount,
                                                                       ApprovedBySupervisorCount = doc.ApprovedBySupervisorCount,
                                                                       RejectedBySupervisorCount = doc.RejectedBySupervisorCount,
                                                                       ApprovedByHeadquartersCount = doc.ApprovedByHeadquartersCount,
                                                                       RejectedByHeadquartersCount = doc.RejectedByHeadquartersCount,
                                                                       RestoredCount = doc.RestoredCount,
                                                                       TotalCount = doc.TotalCount
                                                                   });

            this.AddMap<StatisticsLineGroupedByUserAndTemplate>(docs => from doc in docs
                                                                   where doc.TeamLeadId != null
                                                                   select new StatisticsLineGroupedByUserAndTemplate
                                                                   {
                                                                       ResponsibleId = doc.ResponsibleId,
                                                                       ResponsibleName = doc.ResponsibleName,

                                                                       TeamLeadId = doc.TeamLeadId,
                                                                       TeamLeadName = doc.TeamLeadName,

                                                                       QuestionnaireId = Guid.Empty,
                                                                       QuestionnaireVersion = 0,
                                                                       QuestionnaireTitle = "",

                                                                       CreatedCount = doc.CreatedCount,
                                                                       SupervisorAssignedCount = doc.SupervisorAssignedCount,
                                                                       InterviewerAssignedCount = doc.InterviewerAssignedCount,
                                                                       SentToCapiCount = doc.SentToCapiCount,
                                                                       CompletedCount = doc.CompletedCount,
                                                                       ApprovedBySupervisorCount = doc.ApprovedBySupervisorCount,
                                                                       RejectedBySupervisorCount = doc.RejectedBySupervisorCount,
                                                                       ApprovedByHeadquartersCount = doc.ApprovedByHeadquartersCount,
                                                                       RejectedByHeadquartersCount = doc.RejectedByHeadquartersCount,

                                                                       RestoredCount = doc.RestoredCount,
                                                                       TotalCount = doc.TotalCount
                                                                   });

            this.Reduce = results => from result in results
                                group result by new { result.ResponsibleId, result.QuestionnaireId, result.QuestionnaireVersion } into g
                                where g.Sum(x => x.TotalCount) > 0
                                select new StatisticsLineGroupedByUserAndTemplate
                                {
                                    ResponsibleId = g.Key.ResponsibleId,
                                    ResponsibleName = g.First().ResponsibleName,

                                    TeamLeadId = g.First().TeamLeadId,
                                    TeamLeadName = g.First().TeamLeadName,

                                    QuestionnaireId = g.Key.QuestionnaireId,
                                    QuestionnaireVersion = g.Key.QuestionnaireVersion,
                                    QuestionnaireTitle = g.First().QuestionnaireTitle,

                                    CreatedCount = g.Sum(x => x.CreatedCount),
                                    SupervisorAssignedCount = g.Sum(x => x.SupervisorAssignedCount),
                                    InterviewerAssignedCount = g.Sum(x => x.InterviewerAssignedCount),
                                    SentToCapiCount = g.Sum(x => x.SentToCapiCount),
                                    CompletedCount = g.Sum(x => x.CompletedCount),
                                    ApprovedBySupervisorCount = g.Sum(x => x.ApprovedBySupervisorCount),
                                    RejectedBySupervisorCount = g.Sum(x => x.RejectedBySupervisorCount),
                                    ApprovedByHeadquartersCount = g.Sum(x => x.ApprovedByHeadquartersCount),
                                    RejectedByHeadquartersCount = g.Sum(x => x.RejectedByHeadquartersCount),

                                    RestoredCount = g.Sum(x => x.RestoredCount),
                                    TotalCount = g.Sum(x => x.TotalCount),

                                };
            this.Index(x => x.TeamLeadId, FieldIndexing.Analyzed);
            this.Index(x => x.QuestionnaireId, FieldIndexing.Analyzed);
            this.Index(x => x.QuestionnaireVersion, FieldIndexing.Analyzed);
        }

    }
}
