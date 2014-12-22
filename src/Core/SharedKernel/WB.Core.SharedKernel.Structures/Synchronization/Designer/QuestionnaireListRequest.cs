namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireListRequest
    {
        public string Filter { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortOrder { get; set; }
    }
}
