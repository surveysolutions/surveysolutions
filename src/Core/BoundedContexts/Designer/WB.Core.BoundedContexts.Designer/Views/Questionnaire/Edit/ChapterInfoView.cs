using Main.Core.Entities.SubEntities;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class ChapterInfoView : GroupInfoView
    {
        public ChapterInfoView() : base(false)
        {
        }
    }

    public class GroupInfoView
    {
        public string GroupId { get; set; }
        public string Title { get; set; }

        internal readonly bool isRoster;
        public readonly ChapterInfoStatisticsView Statistics;

        public GroupInfoView(bool isRoster)
        {
            this.isRoster = isRoster;

            this.Questions = new List<QuestionInfoView>();
            this.Groups = new List<GroupInfoView>();
            this.Statistics = new ChapterInfoStatisticsView(groups: this.Groups, questions: this.Questions);
        }

        public List<QuestionInfoView> Questions { get; private set; }
        public List<GroupInfoView> Groups { get; private set; }
    }

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
            get { return this.questions.Count() + this.groups.Sum(group => group.Statistics.QuestionsCount); }
        }

        public int GroupsCount
        {
            get { return this.groups.Count() + this.groups.Sum(group => group.Statistics.GroupsCount); }
        }

        public int RostersCount
        {
            get
            {
                return this.groups.Count(group => @group.isRoster) +
                       this.groups.Sum(group => group.Statistics.RostersCount);
            }
        }

    }

    public class QuestionInfoView
    {
        public string QuestionId { get; set; }
        public string Title { get; set; }
        public string Variable { get; set; }
        public QuestionType Type { get; set; }
        public IEnumerable<string> LinkedVariables { get; set; }
        public IEnumerable<string> BrokenLinkedVariables { get; set; }
    }

}
