namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IKeywordsProvider 
    {
        bool IsReservedKeyword(string keyword);
    }
}