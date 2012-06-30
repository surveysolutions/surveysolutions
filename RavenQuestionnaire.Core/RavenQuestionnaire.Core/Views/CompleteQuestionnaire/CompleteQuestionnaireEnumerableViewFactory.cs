using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireEnumerableViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>
    {
        private IDocumentSession documentSession;
        private ICompleteGroupFactory groupFactory;
        public CompleteQuestionnaireEnumerableViewFactory(IDocumentSession documentSession, ICompleteGroupFactory groupFactory)
        {
            this.documentSession = documentSession;
            this.groupFactory = groupFactory;
        }

        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
              //  var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;

                Iterator<ICompleteGroup> iterator =
                    new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group =
                        doc.Find
                            <RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup>(
                                input.CurrentGroupPublicKey.Value);
                }
               
                return new CompleteQuestionnaireViewEnumerable(doc, group, this.groupFactory);
            }
         
            return null;

        }
    }
}
