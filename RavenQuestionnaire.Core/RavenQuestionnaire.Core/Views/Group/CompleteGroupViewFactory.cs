using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupViewFactory : IViewFactory<CompleteGroupViewInputModel, CompleteGroupView>
    {
        private IDocumentSession documentSession;
        private ICompleteGroupFactory groupFactory;
        private IIteratorContainer iteratorContainer;
        public CompleteGroupViewFactory(IDocumentSession documentSession, ICompleteGroupFactory groupFactory, IIteratorContainer iteratorContainer)
        {
            this.documentSession = documentSession;
            this.groupFactory = groupFactory;
            this.iteratorContainer = iteratorContainer;
        }

        public CompleteGroupView Load(CompleteGroupViewInputModel input)
        {
            var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.QuestionnaireId);
            Entities.SubEntities.Complete.CompleteGroup group;
            if (input.PublicKey.HasValue)
            {
                group =
                    new Entities.CompleteQuestionnaire(doc, iteratorContainer).Find<Entities.SubEntities.Complete.CompleteGroup>(
                        input.PublicKey.Value);
            }
            else
            {
                group = new Entities.SubEntities.Complete.CompleteGroup()
                            {
                                Questions =
                                    doc.Questions.ToList()
                            };

            }
            return this.groupFactory.CreateGroup(doc, group);
        }
    }
}

