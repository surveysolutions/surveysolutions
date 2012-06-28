#region

using System;
using System.Linq;
using Ncqrs.Eventing.Storage;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ViewSnapshot;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionnaireJsonViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireJsonViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireJsonView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc =
                    this.store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.CompleteQuestionnaireId));
                //var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
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
                return new CompleteQuestionnaireJsonView(doc, group);
            }
          /*  if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireJsonView((CompleteQuestionnaireDocument)doc);
            }*/
            return null;
        }

        #endregion
    }
}