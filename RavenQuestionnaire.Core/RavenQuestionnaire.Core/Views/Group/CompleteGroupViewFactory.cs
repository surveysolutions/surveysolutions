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
        public CompleteGroupViewFactory(IDocumentSession documentSession, ICompleteGroupFactory groupFactory)
        {
            this.documentSession = documentSession;
            this.groupFactory = groupFactory;
        }

        public CompleteGroupView Load(CompleteGroupViewInputModel input)
        {
            var doc = documentSession.Load<CompleteQuestionnaireStoreDocument>(input.QuestionnaireId);
            CompleteGroup group;
            if (input.PublicKey.HasValue)
            {
                group = doc.Find<CompleteGroup>(input.PublicKey.Value);
            }
            else
            {
                group = new CompleteGroup()
                            {
                                Children = 
                                    doc.Children.Where(c=>c is ICompleteQuestion).ToList()};
                group.PublicKey = Guid.Empty;

            }
            return this.groupFactory.CreateGroup(doc, group);
        }
    }
}

