using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnairesResponse
    {
        public int TotalCount { get; set; }
        public IEnumerable<QuestionnaireMetaInfo> Items { get; set; }
    }
}
