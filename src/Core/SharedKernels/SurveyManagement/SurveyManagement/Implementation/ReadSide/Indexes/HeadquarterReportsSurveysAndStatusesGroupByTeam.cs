using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class HeadquarterReportsSurveysAndStatusesGroupByTeam : AbstractMultiMapIndexCreationTask<StatisticsLineGroupedByUserAndTemplate>
    {
        public HeadquarterReportsSurveysAndStatusesGroupByTeam()
        {
            this.AddMap<StatisticsLineGroupedByUserAndTemplate>(docs => from doc in docs
                                                                   where doc.TeamLeadId != null
                                                                   select new StatisticsLineGroupedByUserAndTemplate
                                                                       {
                                                                           ResponsibleId = (Guid) doc.TeamLeadId,
                                                                           ResponsibleName = doc.TeamLeadName,

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
                                                                   select new StatisticsLineGroupedByUserAndTemplate
                                                                       {
                                                                           ResponsibleId = Guid.Empty,
                                                                           ResponsibleName = string.Empty,

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

            this.Reduce = results => from result in results
                                group result by new {result.ResponsibleId, result.QuestionnaireId, result.QuestionnaireVersion}
                                into g
                                where g.Sum(x => x.TotalCount) > 0
                                select new StatisticsLineGroupedByUserAndTemplate
                                    {
                                        ResponsibleId = g.Key.ResponsibleId,
                                        ResponsibleName = g.First().ResponsibleName,

                                        QuestionnaireId = g.Key.QuestionnaireId,
                                        QuestionnaireVersion = g.Key.QuestionnaireVersion,
                                        QuestionnaireTitle = g.First().QuestionnaireTitle,

                                        TeamLeadId = g.First().TeamLeadId,
                                        TeamLeadName = g.First().TeamLeadName,

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
                                        TotalCount = g.Sum(x => x.TotalCount)
                                    };
            this.Index(x => x.TotalCount, FieldIndexing.Analyzed);
            this.Index(x => x.TeamLeadId, FieldIndexing.Analyzed);
            this.Index(x => x.QuestionnaireId, FieldIndexing.Analyzed);
            this.Index(x => x.QuestionnaireVersion, FieldIndexing.Analyzed);
        }
    }
}
