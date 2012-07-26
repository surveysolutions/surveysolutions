using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupViewFactory : IViewFactory<GroupViewInputModel, GroupView>
    {
        private readonly IViewSnapshot store;

        public GroupViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }
        public GroupView Load(GroupViewInputModel input)
        {
            var doc = store.ReadByGuid<QuestionnaireDocument>(Guid.Parse(input.QuestionnaireId));
            var group = new Entities.Questionnaire(doc).Find<Entities.SubEntities.Group>(input.PublicKey);
            if (group == null)
                return null;
            return new GroupView(doc, group);
        }
    }
}
