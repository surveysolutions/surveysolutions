using System;
using System.Collections.Generic;
using Core.Supervisor.DenormalizerStorageItem;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes;

namespace Core.Supervisor.Views.Survey
{
    using System.Linq;

    using Main.Core.View;

    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class SurveyUsersViewFactory : IViewFactory<SurveyUsersViewInputModel, SurveyUsersView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public SurveyUsersViewFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public SurveyUsersView Load(SurveyUsersViewInputModel input)
        {
            var items = Enumerable.Empty<StatisticsLineGroupedByUserAndTemplate>();
            if (input.ViewerStatus == ViewerStatus.Headquarter)
            {
                var indexName = typeof (HeadquarterReportsSurveysAndStatusesGroupByTeam).Name;
                items = indexAccessor
                    .Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                    .Where(x => x.ResponsibleId != Guid.Empty);
            }
            else if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                var indexName = typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Name;
                items = indexAccessor
                    .Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                    .Where(x => x.TeamLeadId == input.ViewerId && x.ResponsibleId != Guid.Empty);
            }

            return new SurveyUsersView
            {
                Items = items.ToList().Distinct(new SurveyItemByUserNameComparer())
                        .Select(x => new SurveyUsersViewItem
                                {
                                    UserId = x.ResponsibleId,
                                    UserName = x.ResponsibleName
                                })
            };
        }
    }

    public class SurveyItemByUserNameComparer : IEqualityComparer<StatisticsLineGroupedByUserAndTemplate>
    {
        public bool Equals(StatisticsLineGroupedByUserAndTemplate x, StatisticsLineGroupedByUserAndTemplate y)
        {
            if (Object.ReferenceEquals(y, null)) return false;

            if (Object.ReferenceEquals(x, y)) return true;

            return x.ResponsibleName.Equals(y.ResponsibleName);
        }

        public int GetHashCode(StatisticsLineGroupedByUserAndTemplate obj)
        {
            return obj.ResponsibleName.GetHashCode();
        }
    }
}
