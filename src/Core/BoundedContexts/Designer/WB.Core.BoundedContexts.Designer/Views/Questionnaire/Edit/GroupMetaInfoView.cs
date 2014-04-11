using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class GroupMetaInfoView : QuestionnaireItemMetaInfoView
    {
        public string GroupId { get; set; }

        public GroupMetaInfoView(List<GroupInfoView> groups, List<QuestionInfoView> questions)
            : base(groups: groups, questions: questions)
        {
        }
    }
}