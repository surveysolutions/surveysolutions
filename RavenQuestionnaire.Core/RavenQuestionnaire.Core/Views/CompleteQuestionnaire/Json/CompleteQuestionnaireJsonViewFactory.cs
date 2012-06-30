#region

using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.Storage;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Entities.Extensions;
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
                
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
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


    public class CompleteQuestionnaireMobileViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireMobileViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteGroupMobileView Load(CompleteQuestionnaireViewInputModel input)
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
                    group = doc.FindGroupByKey(input.CurrentGroupPublicKey.Value, input.PropagationKey);
                }
                if (input.PropagationKey.HasValue)
                    return new PropagatedGroupMobileView(doc, group);
                return new CompleteGroupMobileView(doc, (CompleteGroup) group, new List<ScreenNavigation>(0));
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