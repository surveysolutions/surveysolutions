namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoriesExtractFactory
    {
        ICategoriesExtractService GetExtractService(CategoriesFileType type);
    }
}
