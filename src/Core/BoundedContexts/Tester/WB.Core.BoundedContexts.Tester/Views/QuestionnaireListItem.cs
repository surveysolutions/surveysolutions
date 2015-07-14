using System;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Views
{
    public class QuestionnaireListItem : IPlainStorageEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime LastEntryDate { get; set; }
        public bool IsPublic { get; set; }
        public string OwnerName { get; set; }
    }
}
