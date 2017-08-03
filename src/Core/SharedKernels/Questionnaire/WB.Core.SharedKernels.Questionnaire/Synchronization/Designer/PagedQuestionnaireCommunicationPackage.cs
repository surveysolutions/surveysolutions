using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class PagedQuestionnaireCommunicationPackage
    {
        public IEnumerable<QuestionnaireListItem> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
