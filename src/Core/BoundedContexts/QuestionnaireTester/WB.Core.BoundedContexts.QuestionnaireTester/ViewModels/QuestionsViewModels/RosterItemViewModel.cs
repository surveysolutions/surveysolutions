using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterItemViewModel : MvxNotifyPropertyChanged
    {
        public string QuestionnaireRosterTitle { get; set; }
        public string InterviewRosterTitle { get; set; }
        public int AnsweredQuestionsCount { get; set; }
        public int SubgroupsCount { get; set; }
        public int QuestionsCount { get; set; }
    }
}