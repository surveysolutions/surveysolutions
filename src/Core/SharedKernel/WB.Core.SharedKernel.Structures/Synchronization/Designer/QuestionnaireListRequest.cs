using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    [Obsolete("remove after all HQ will be 5.11+")]
    public class QuestionnaireListRequest
    {
        public string Filter { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortOrder { get; set; }
    }
}
