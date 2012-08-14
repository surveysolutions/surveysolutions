using System.Linq;
using System.Collections.Generic;
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
            var query = documentItemSession.Query().Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return new SurveyBrowseView( 
                input.Page,
                input.PageSize, count,
                query);
        }
    }
}
