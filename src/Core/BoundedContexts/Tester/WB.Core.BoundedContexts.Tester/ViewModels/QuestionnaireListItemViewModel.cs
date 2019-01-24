using System;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public enum QuestionnairesType
    {
        My = 1,
        SharedWithMe,
        Public
    }
    public class QuestionnaireListItemViewModel
    {
        public string Id { get; set; }
        public string SearchTerm { get; set; }
        public string Title { get; set; }
        public DateTime LastEntryDate { get; set; }
        public bool IsOwner { get; set; }
        public bool IsPublic { get; set; }
        public bool IsShared { get; set; }
        public string OwnerName { get; set; }
        public QuestionnairesType Type { get; set; }
    }
}
