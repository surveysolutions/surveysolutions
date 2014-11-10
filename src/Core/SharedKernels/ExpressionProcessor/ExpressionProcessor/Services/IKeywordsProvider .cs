namespace WB.Core.SharedKernels.ExpressionProcessor.Services
{
    public interface IKeywordsProvider 
    {
        string[] GetAllReservedKeywords();
    }
}