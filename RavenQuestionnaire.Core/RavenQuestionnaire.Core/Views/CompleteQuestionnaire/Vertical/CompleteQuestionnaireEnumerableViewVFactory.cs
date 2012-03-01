#region

using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical
{
    public class CompleteQuestionnaireEnumerableViewFactoryV :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>
    {
        private readonly IDocumentSession documentSession;

        public CompleteQuestionnaireEnumerableViewFactoryV(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireViewV Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;

                Iterator<ICompleteGroup> iterator =
                    new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = completeQuestionnaireRoot.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }
                else if (input.PreviousGroupPublicKey.HasValue)
                {
                    iterator.SetCurrent(completeQuestionnaireRoot.Find<CompleteGroup>(input.PreviousGroupPublicKey.Value));
                    group = input.IsReverse
                                ? iterator.Previous
                                : iterator.Next;
                }
                else
                {
                    group = input.IsReverse ? iterator.Last() : iterator.First();
                }
                return new CompleteQuestionnaireViewV(doc, group);
            }
            if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireViewV((CompleteQuestionnaireDocument)doc);
            }
            return null;
        }

        #endregion
    }
}