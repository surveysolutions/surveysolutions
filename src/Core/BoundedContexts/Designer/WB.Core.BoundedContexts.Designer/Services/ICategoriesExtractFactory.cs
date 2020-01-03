namespace WB.Core.BoundedContexts.Designer.Services
{
    internal interface ICategoriesExtractFactory
    {
        ICategoriesExtractService GetExtractService(CategoriesFileType type);
    }
}
