using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireEnumerableViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireDocument> documentItemSession;
        private ICompleteGroupFactory groupFactory;
        public CompleteQuestionnaireEnumerableViewFactory(IDenormalizerStorage<CompleteQuestionnaireDocument> documentItemSession, ICompleteGroupFactory groupFactory)
        {
            this.documentItemSession = documentItemSession;
            this.groupFactory = groupFactory;
        }

        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentItemSession.Query().FirstOrDefault(i => i.Id == input.CompleteQuestionnaireId);
                ICompleteGroup group = null;

                Iterator<ICompleteGroup> iterator =
                    new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group =doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }
               
                return new CompleteQuestionnaireViewEnumerable(doc, group, this.groupFactory);
            }
         
            return null;

        }
    }
}
