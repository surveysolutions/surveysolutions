using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories.Indexes;
using WB.Core.BoundedContexts.Headquarters.Reports.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories.Implementation
{
    internal class AllUsersAndQuestionnairesFactory :
        IAllUsersAndQuestionnairesFactory
    {
       private readonly IReadSideRepositoryIndexAccessor indexAccessor;

       public AllUsersAndQuestionnairesFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

       public AllUsersAndQuestionnairesView Load()
        {
           var indexName = typeof (HeadquarterReportsTeamsAndStatusesGroupByTeam).Name;
           var items = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName).Where(x => x.QuestionnaireId != Guid.Empty);

            var interviewItems = items.ToList();

            var users = interviewItems.Select(x => new UsersViewItem { UserId = x.ResponsibleId, UserName = x.ResponsibleName }).Distinct(new SurveyUsersViewItemComparer());
            var questionnaires = interviewItems.Select(x => new TemplateViewItem { TemplateId = x.QuestionnaireId, TemplateName = x.QuestionnaireTitle, TemplateVersion = x.QuestionnaireVersion }).Distinct(new SummaryTemplateItemComparer());

            return new AllUsersAndQuestionnairesView
            {
                Users = users,
                Questionnaires = questionnaires
            };
        }

        public class SummaryTemplateItemComparer : IEqualityComparer<TemplateViewItem>
        {
            public bool Equals(TemplateViewItem x, TemplateViewItem y)
            {
                if (ReferenceEquals(y, null)) return false;

                if (ReferenceEquals(x, y)) return true;

                return x.TemplateId.Equals(y.TemplateId) && x.TemplateName.Equals(y.TemplateName);
            }

            public int GetHashCode(TemplateViewItem x)
            {
                return 37 + x.TemplateId.GetHashCode() * 23 + x.TemplateName.GetHashCode() * 29 + x.TemplateVersion.GetHashCode() * 47;
            }
        }

        public class SurveyUsersViewItemComparer : IEqualityComparer<UsersViewItem>
        {
            public bool Equals(UsersViewItem x, UsersViewItem y)
            {
                if (ReferenceEquals(y, null)) return false;

                if (ReferenceEquals(x, y)) return true;

                return x.UserId.Equals(y.UserId) && x.UserName.Equals(y.UserName);
            }

            public int GetHashCode(UsersViewItem x)
            {
                return 41 + x.UserId.GetHashCode() * 37 + x.UserName.GetHashCode() * 17;
            }
        }
    }
}
