using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupViewFactory : IViewFactory<SurveyGroupInputModel, SurveyGroupView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public SurveyGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public SurveyGroupView Load(SurveyGroupInputModel input)
        {
            var count = documentItemSession.Query().Where(x => x.TemplateId == input.Id).ToList().Count;
            if (count==0)
                return new SurveyGroupView(input.Page, input.PageSize, 0, new CompleteQuestionnaireBrowseItem[0]);
            var query = documentItemSession.Query().Where(input.Expression).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return new SurveyGroupView(input.Page, input.PageSize, 0, query);
        }
    }
}
