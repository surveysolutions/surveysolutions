﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Survey
{
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
                items = this.indexAccessor
                    .Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                    .Where(x => x.ResponsibleId != Guid.Empty);
            }
            else if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                var indexName = typeof (SupervisorReportsSurveysAndStatusesGroupByTeamMember).Name;
                items = this.indexAccessor
                    .Query<StatisticsLineGroupedByUserAndTemplate>(indexName)
                    .Where(x => x.TeamLeadId == input.ViewerId && x.ResponsibleId != Guid.Empty);
            }

            return new SurveyUsersView
            {
                Items = items.ToList().Distinct(new SurveyItemByUserNameComparer())
                        .Select(x => new UsersViewItem
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
