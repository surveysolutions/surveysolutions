using System;
using System.Collections.Generic;
using System.Linq;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.UsersAndQuestionnaires
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

            var users = interviewItems.Select(x => new SurveyUsersViewItem { UserId = x.ResponsibleId, UserName = x.ResponsibleName }).Distinct(new SurveyUsersViewItemComparer());
            var questionnaires = interviewItems.Select(x => new SummaryTemplateViewItem { TemplateId = x.QuestionnaireId, TemplateName = x.QuestionnaireTitle }).Distinct(new SummaryTemplateItemComparer());

            return new AllUsersAndQuestionnairesView
            {
                Users = users,
                Questionnaires = questionnaires
            };
        }

        public class SummaryTemplateItemComparer : IEqualityComparer<SummaryTemplateViewItem>
        {
            public bool Equals(SummaryTemplateViewItem x, SummaryTemplateViewItem y)
            {
                if (ReferenceEquals(y, null)) return false;

                if (ReferenceEquals(x, y)) return true;

                return x.TemplateId.Equals(y.TemplateId) && x.TemplateName.Equals(y.TemplateName);
            }

            public int GetHashCode(SummaryTemplateViewItem x)
            {
                return 37 + x.TemplateId.GetHashCode() * 23 + x.TemplateName.GetHashCode() * 29;
            }
        }

        public class SurveyUsersViewItemComparer : IEqualityComparer<SurveyUsersViewItem>
        {
            public bool Equals(SurveyUsersViewItem x, SurveyUsersViewItem y)
            {
                if (ReferenceEquals(y, null)) return false;

                if (ReferenceEquals(x, y)) return true;

                return x.UserId.Equals(y.UserId) && x.UserName.Equals(y.UserName);
            }

            public int GetHashCode(SurveyUsersViewItem x)
            {
                return 41 + x.UserId.GetHashCode() * 37 + x.UserName.GetHashCode() * 17;
            }
        }
    }
}
