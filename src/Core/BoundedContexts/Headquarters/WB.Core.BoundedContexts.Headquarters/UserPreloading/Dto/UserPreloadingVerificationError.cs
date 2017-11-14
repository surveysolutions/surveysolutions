namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class UserPreloadingVerificationError
    {
        public virtual string Code { get; set; }
        public virtual int RowNumber { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual string CellValue { get; set; }
    }
}