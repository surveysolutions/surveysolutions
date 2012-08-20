using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Denormalizers;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyViewFactory : IViewFactory<SurveyViewInputModel, SurveyBrowseView>
    {
        private readonly IDenormalizerStorage<SurveyBrowseItem> documentItemSession;

        public SurveyViewFactory(IDenormalizerStorage<SurveyBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public SurveyBrowseView Load(SurveyViewInputModel input)
        {
            var count = documentItemSession.Query().Count();
            if (count == 0)
                return new SurveyBrowseView(input.Page, input.PageSize, count, new List<SurveyBrowseItem>());
            IQueryable<SurveyBrowseItem> query = documentItemSession.Query();
            if (input.Orders.Count>0)
            {
                    query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);
            }
            query = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new SurveyBrowseView( 
                input.Page,
                input.PageSize, count,
                query);
        }
    }
}
