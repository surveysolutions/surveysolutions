using System;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupViewFactory : IViewFactory<GroupViewInputModel, GroupView>
    {
        private readonly IDenormalizerStorage<QuestionnaireDocument> store;

        public GroupViewFactory(IDenormalizerStorage<QuestionnaireDocument> store)
        {
            this.store = store;
        }
        public GroupView Load(GroupViewInputModel input)
        {
            var doc = store.GetByGuid(Guid.Parse(input.QuestionnaireId));
            var group = new Entities.Questionnaire(doc).Find<Entities.SubEntities.Group>(input.PublicKey);
            if (group == null)
                return null;
            return new GroupView(doc, group);
        }
    }
}
