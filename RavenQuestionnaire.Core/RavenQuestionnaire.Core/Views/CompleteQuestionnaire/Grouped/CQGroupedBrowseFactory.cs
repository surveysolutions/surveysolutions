using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupedBrowseFactory : IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public CQGroupedBrowseFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }
        #region Implementation of IViewFactory<CQGroupedBrowseInputModel,CQGroupedBrowseView>

        public CQGroupedBrowseView Load(CQGroupedBrowseInputModel input)
        {
            var questionnaires = this.documentItemSession.Query();
            var templates = this.documentGroupSession.Query();

            var retval = new CQGroupedBrowseView(0, 100, 100, templates.ToList());
            foreach (CQGroupItem cqGroupItem in retval.Groups)
            {
                CQGroupItem item = cqGroupItem;
                var complete = questionnaires.Where(q => q.TemplateId == item.SurveyId).ToList();
                cqGroupItem.Items = complete;
            }
            return retval;
            
        }

        #endregion
    }
}
