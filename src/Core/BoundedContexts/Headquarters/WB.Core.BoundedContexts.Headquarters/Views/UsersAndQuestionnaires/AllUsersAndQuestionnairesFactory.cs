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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public interface IAllUsersAndQuestionnairesFactory
    {
        AllUsersAndQuestionnairesView Load();
        List<TemplateViewItem> GetQuestionnaires();
        List<TemplateViewItem> GetOlderQuestionnairesWithPendingAssignments(Guid id, long version);
        /// <summary>
        /// Return bindable on UI list of questionnaires
        /// </summary>
        /// <param name="questionnaireBrowseItems"></param>
        /// <returns></returns>
        List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaireComboboxViewItems();

        List<QuestionnaireIdentity> GetQuestionnaires(Guid? id, long? version);
    }

    public class AllUsersAndQuestionnairesFactory : IAllUsersAndQuestionnairesFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader;
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignments;

        public AllUsersAndQuestionnairesFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignments)
        {
            this.questionnairesReader = questionnairesReader;
            this.interviewSummaryReader = interviewSummaryReader;
            this.assignments = assignments;
        }

        public AllUsersAndQuestionnairesView Load()
        {
            var allUsers =
                this.interviewSummaryReader.Query(
                    _ => _.GroupBy(x => new { TeamLeadId = x.SupervisorId, TeamLeadName = x.SupervisorName })
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

        public List<QuestionnaireVersionsComboboxViewItem> GetQuestionnaireComboboxViewItems()
        {
            return this.questionnairesReader.Query(_ =>
                _.Where(q => !q.IsDeleted).ToList()).GetQuestionnaireComboboxViewItems();
        }

        public List<QuestionnaireIdentity> GetQuestionnaires(Guid? id, long? version)
        {
            return this.questionnairesReader.Query(_ =>
            {
                var query = _.Where(q => q.IsDeleted == false);

                if (id != null)
                {
                    query = query.Where(q => q.QuestionnaireId == id.Value);

                    if (version != null)
                    {
                        query = query.Where(q => q.Version == version.Value);
                    }
                }

                return query.Select(q => q.Identity()).ToList();
            });
        }

        public List<TemplateViewItem> GetOlderQuestionnairesWithPendingAssignments(Guid questionnaireId, long version)
        {
            var questionnaireIdentities = this.assignments
                .Query(_ => _
                    .Where(x => x.QuestionnaireId.QuestionnaireId == questionnaireId &&
                                x.QuestionnaireId.Version < version
                                && !x.Archived
                          //&& (x.InterviewSummaries.Count - x.Quantity > 0 || x.Quantity == null) // does not work for some reason
                          )
                    .Select(x => new { x.QuestionnaireId.QuestionnaireId, x.QuestionnaireId.Version })
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
