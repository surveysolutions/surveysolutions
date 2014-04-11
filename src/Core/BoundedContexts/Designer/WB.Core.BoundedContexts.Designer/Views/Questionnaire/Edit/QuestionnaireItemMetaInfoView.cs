using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireItemMetaInfoView
    {
        public string Title { get; set; }
        public readonly ChapterInfoStatisticsView Statistics;

        protected readonly List<GroupInfoView> groups;
        protected readonly List<QuestionInfoView> questions;

        public QuestionnaireItemMetaInfoView(List<GroupInfoView> groups, List<QuestionInfoView> questions)
        {
            this.groups = groups;
            this.questions = questions;
            this.Statistics = new ChapterInfoStatisticsView(groups, questions);
        }
    }
}