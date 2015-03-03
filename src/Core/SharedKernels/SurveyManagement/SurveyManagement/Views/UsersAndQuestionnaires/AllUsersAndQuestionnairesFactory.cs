using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class AllUsersAndQuestionnairesFactory : IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>
    {
       private readonly IReadSideRepositoryIndexAccessor indexAccessor;

       public AllUsersAndQuestionnairesFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

       public AllUsersAndQuestionnairesView Load(AllUsersAndQuestionnairesInputModel input)
        {
           var indexName = typeof (HeadquarterReportsTeamsAndStatusesGroupByTeam).Name;
           var items = this.indexAccessor.Query<StatisticsLineGroupedByUserAndTemplate>(indexName).Where(x => x.QuestionnaireId != Guid.Empty);

            var interviewItems = items.ToList();

            var users = interviewItems.Select(x => new UsersViewItem { UserId = x.ResponsibleId, UserName = x.ResponsibleName }).Distinct(new SurveyUsersViewItemComparer()).OrderBy(x => x.UserName);
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
