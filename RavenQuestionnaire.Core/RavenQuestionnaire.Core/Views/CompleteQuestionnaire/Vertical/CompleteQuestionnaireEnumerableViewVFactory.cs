#region

using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical
{
    public class CompleteQuestionnaireEnumerableViewFactoryV :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireDocument> documentItemSession;

        public CompleteQuestionnaireEnumerableViewFactoryV(IDenormalizerStorage<CompleteQuestionnaireDocument> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireViewV Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentItemSession.Query().FirstOrDefault(i => i.Id == input.CompleteQuestionnaireId);
             //   var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;

                Iterator<ICompleteGroup> iterator =
                    new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }
                return new CompleteQuestionnaireViewV(doc, group);
            }
         
            return null;
        }

        #endregion
    }
}