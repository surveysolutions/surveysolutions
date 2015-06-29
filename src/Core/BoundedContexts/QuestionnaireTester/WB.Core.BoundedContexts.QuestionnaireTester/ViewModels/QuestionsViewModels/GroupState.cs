namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public struct GroupState
    {
        public int AnsweredQuestionsCount { get; set; }

        public int SubgroupsCount { get; set; }

        public int QuestionsCount { get; set; }

        public int InvalidAnswersCount { get; set; }

        public GroupStatus Status { get; set; }

    }
}