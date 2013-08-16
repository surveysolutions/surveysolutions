using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Main.Core.Utility;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using System.Linq;
using Main.Core.View;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamUsersAndQuestionnairesFactory : IViewFactory<TeamInterviewsInputModel, TeamUsersAndQuestionnairesView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public TeamUsersAndQuestionnairesFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public TeamUsersAndQuestionnairesView Load(TeamInterviewsInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> predicate = (i) => !i.IsDeleted;

            predicate = predicate.AndCondition(x => x.TeamLeadId != null && x.TeamLeadId == input.ViewerId);

            var interviewItems = this.interviews.Query(_ => _.Where(predicate)).ToList();

            var users = interviewItems.Select(x => new SurveyUsersViewItem { UserId = x.ResponsibleId, UserName = x.ResponsibleName }).Distinct(new SurveyUsersViewItemComparer());
            var questionnaires = interviewItems.Select(x => new SummaryTemplateViewItem { TemplateId = x.QuestionnaireId, TemplateName = x.QuestionnaireTitle }).Distinct(new SummaryTemplateItemComparer());

            return new TeamUsersAndQuestionnairesView
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
