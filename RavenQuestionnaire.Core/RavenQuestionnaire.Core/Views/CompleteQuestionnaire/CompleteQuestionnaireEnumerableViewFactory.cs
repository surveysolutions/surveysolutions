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
        private IIteratorContainer iteratorContainer;
        public CompleteQuestionnaireEnumerableViewFactory(IDocumentSession documentSession, ICompleteGroupFactory groupFactory, IIteratorContainer iteratorContainer)
        {
            this.documentSession = documentSession;
            this.groupFactory = groupFactory;
            this.iteratorContainer = iteratorContainer;
        }

        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc, iteratorContainer);
                RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup group = null;

                Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup> iterator =
                    new QuestionnaireScreenIterator(doc);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group =
                        completeQuestionnaireRoot.Find
                            <RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup>(
                                input.CurrentGroupPublicKey.Value);
                }
                else if (input.PreviousGroupPublicKey.HasValue)
                {

                    iterator.SetCurrent(completeQuestionnaireRoot.Find
                                            <RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup>(
                                                input.PreviousGroupPublicKey.Value));
                    group = input.IsReverse
                                ? iterator.Previous
                                : iterator.Next;
                }
                else
                {
                    group = input.IsReverse ? iterator.Last() : iterator.First();
                }
                return new CompleteQuestionnaireViewEnumerable(doc, group, this.groupFactory);
            }
            if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireViewEnumerable((CompleteQuestionnaireDocument)doc, this.groupFactory);
            }
            return null;

        }
    }
}
