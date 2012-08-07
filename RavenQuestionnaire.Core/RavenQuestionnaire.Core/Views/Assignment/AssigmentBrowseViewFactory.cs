using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.Views.Assignment
{
    public class AssigmentBrowseViewFactory : IViewFactory<AssigmentBrowseInputModel, AssigmentBrowseView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public AssigmentBrowseViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }
        
        public AssigmentBrowseView Load(AssigmentBrowseInputModel input)
        {
            var questionnaires = this.documentItemSession.Query().Where(x=>x.TemplateId==input.Id);
            return new AssigmentBrowseView() { Items = (List<AssigmentBrowseItem>) questionnaires };
        }
    }
}
