using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class ChapterInfoStatisticsView
    {
        private readonly IEnumerable<GroupInfoView> groups;
        private readonly IEnumerable<QuestionInfoView> questions;

        public ChapterInfoStatisticsView(IEnumerable<GroupInfoView> groups, IEnumerable<QuestionInfoView> questions)
        {
            this.groups = groups;
            this.questions = questions;
        }

        public int QuestionsCount
        {
            get { return this.questions.Count() + this.groups.Sum(group => @group.Statistics.QuestionsCount); }
        }

        public int GroupsCount
        {
            get { return this.groups.Count() + this.groups.Sum(group => @group.Statistics.GroupsCount); }
        }

        public int RostersCount
        {
            get
            {
                return this.groups.Count(group => @group.isRoster) +
                       this.groups.Sum(group => @group.Statistics.RostersCount);
            }
        }

    }
}