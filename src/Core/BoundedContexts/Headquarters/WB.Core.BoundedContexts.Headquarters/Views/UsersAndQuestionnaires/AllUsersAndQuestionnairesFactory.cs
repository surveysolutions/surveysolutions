using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public interface IAllUsersAndQuestionnairesFactory
    {
        AllUsersAndQuestionnairesView Load();
        List<TemplateViewItem> GetQuestionnaires();
        List<TemplateViewItem> GetOlderQuestionnairesWithPendingAssignments(Guid id, long version);
    }

    public class AllUsersAndQuestionnairesFactory : IAllUsersAndQuestionnairesFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader;
        private readonly IPlainStorageAccessor<Assignment> assignments;

        public AllUsersAndQuestionnairesFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader,
            IPlainStorageAccessor<Assignment> assignments)
        {
            this.questionnairesReader = questionnairesReader;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
        }

        public AllUsersAndQuestionnairesView Load()
        {
            var allUsers =
                this.interviewSummaryReader.Query(
                    _ => _.GroupBy(x => new { x.TeamLeadId, x.TeamLeadName })
                            .Where(x => x.Count() > 0)
                            .Select(x => new UsersViewItem { UserId = x.Key.TeamLeadId, UserName = x.Key.TeamLeadName })
                            .OrderBy(x => x.UserName).ToList());

            var allQuestionnaires =
                this.interviewSummaryReader.Query(
                    _ => _.GroupBy(x => new { x.QuestionnaireTitle, x.QuestionnaireId, x.QuestionnaireVersion })
                        .Where(x => x.Count() > 0)
                        .Select(questionnaire => new TemplateViewItem
                        {
                            TemplateId = questionnaire.Key.QuestionnaireId,
                            TemplateName = questionnaire.Key.QuestionnaireTitle,
                            TemplateVersion = questionnaire.Key.QuestionnaireVersion
                        })
                        .OrderBy(x => x.TemplateName)
                        .ThenBy(n => n.TemplateVersion)
                        .ToList());

            return new AllUsersAndQuestionnairesView
            {
                Users = allUsers,
                Questionnaires = allQuestionnaires
            };
        }

        public List<TemplateViewItem> GetQuestionnaires()
        {
            var questionnaires = this.questionnairesReader.Query(_ => _.Where(q => !q.IsDeleted)
                .Select(questionnaire => new TemplateViewItem
                {
                    TemplateId = questionnaire.QuestionnaireId,
                    TemplateName = questionnaire.Title,
                    TemplateVersion = questionnaire.Version
                }).OrderBy(x => x.TemplateName).ThenBy(n => n.TemplateVersion).ToList());
            return questionnaires;
        }

        public List<TemplateViewItem> GetOlderQuestionnairesWithPendingAssignments(Guid questionnaireId, long version)
        {
            var questionnaireIdentities = this.assignments
                .Query(_ => _
                    .Where(x => x.QuestionnaireId.QuestionnaireId == questionnaireId &&
                                x.QuestionnaireId.Version < version 
                                && !x.Archived
                                //&& (x.InterviewSummaries.Count - x.Quantity > 0 || x.Quantity == null) // do not work for some reason
                          )
                    .Select(x => new {x.QuestionnaireId.QuestionnaireId, x.QuestionnaireId.Version})
                    .Distinct()
                    .ToList());

            var questionnaireGuids = questionnaireIdentities.Select(x => x.QuestionnaireId).ToArray();
            var questionnaireVersions = questionnaireIdentities.Select(x => x.Version).ToArray();

            var questionnaires = this.questionnairesReader.Query(_ => (
                    from q in _
                    where !q.IsDeleted &&
                          questionnaireGuids.Contains(q.QuestionnaireId) &&
                          questionnaireVersions.Contains(q.Version)
                    orderby q.Title, q.Version
                    select new TemplateViewItem
                    {
                        TemplateId = q.QuestionnaireId,
                        TemplateName = q.Title,
                        TemplateVersion = q.Version
                    }
                )
                .ToList());
            return questionnaires;
        }
    }
}
