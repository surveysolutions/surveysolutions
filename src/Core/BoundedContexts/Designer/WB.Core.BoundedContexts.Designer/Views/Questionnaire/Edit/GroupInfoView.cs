using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class GroupInfoView : GroupMetaInfoView
    {
        internal readonly bool isRoster;

        public GroupInfoView(bool isRoster)
            : base(groups: new List<GroupInfoView>(), questions: new List<QuestionInfoView>())
        {
            this.isRoster = isRoster;
        }

        public List<QuestionInfoView> Questions
        {
            get { return base.questions; }
        }

        public List<GroupInfoView> Groups
        {
            get { return base.groups; }
        }
    }
}