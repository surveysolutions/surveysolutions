using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.Supervisor.DenormalizerStorageItem;

namespace Core.Supervisor.Views.Survey
{
    using System.Linq;

    using Main.Core.View;

    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class SurveyUsersViewFactory : IViewFactory<SurveyUsersViewInputModel, SurveyUsersView>
    {
        private readonly IQueryableReadSideRepositoryReader<SummaryItem> summary;

        public SurveyUsersViewFactory(IQueryableReadSideRepositoryReader<SummaryItem> summary)
        {
            this.summary = summary;
        }

        public SurveyUsersView Load(SurveyUsersViewInputModel input)
        {

            Expression<Func<SummaryItem, bool>> predicate = (s) => true;
            if (input.ViewerStatus == ViewerStatus.Headquarter)
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == null);
            }
            else if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == input.ViewerId);
            }

            return new SurveyUsersView()
                {
                    Items =
                        summary.QueryEnumerable(predicate).ToList().Distinct(new SurveyItemByUserNameComparer())
                               .Select(
                                   x =>
                                   new SurveyUsersViewItem()
                                       {
                                           UserId =
                                               x.ResponsibleId,
                                           UserName =
                                               x.ResponsibleName
                                       })
                };



        }
    }

    public class SurveyItemByUserNameComparer : IEqualityComparer<SummaryItem>
    {
        public bool Equals(SummaryItem x, SummaryItem y)
        {
            if (Object.ReferenceEquals(y, null)) return false;

            if (Object.ReferenceEquals(x, y)) return true;

            return x.ResponsibleName.Equals(y.ResponsibleName);
        }

        public int GetHashCode(SummaryItem obj)
        {
            return obj.ResponsibleName.GetHashCode();
        }
    }
}
