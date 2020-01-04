using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesExtractFactory : ICategoriesExtractFactory
    {
        private readonly ICategoriesVerifier verifier;

        public CategoriesExtractFactory(ICategoriesVerifier verifier)
        {
            this.verifier = verifier;
        }

        public ICategoriesExtractService GetExtractService(CategoriesFileType type)
        {
            switch (type)
            {
                case CategoriesFileType.Excel:
                    return new ExcelCategoriesExtractService(verifier);
                case CategoriesFileType.Tsv:
                    return new TsvCategoriesExtractService(verifier);
                default:
                    throw new NotSupportedException("Unknown file type with categories");
            }
        }
    }
}
