#region

using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ViewSnapshot;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class CompleteQuestionnaireMabileViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireMabileViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.CompleteQuestionnaireId));
             //   var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;
                
                Iterator<ICompleteGroup> iterator = new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }
                else if (input.PreviousGroupPublicKey.HasValue)
                {
                    iterator.SetCurrent(doc.Find<CompleteGroup>(input.PreviousGroupPublicKey.Value));
                    group = input.IsReverse ? iterator.Previous : iterator.Next;
                }
                else
                {
                    group = input.IsReverse ? iterator.Last() : iterator.First();
                }
                return new CompleteQuestionnaireMobileView(doc, group);
            }
          /*  if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireMobileView((CompleteQuestionnaireDocument)doc);
            }*/
            return null;
        }

        #endregion
    }
}