using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesExtractFactory : ICategoriesExtractFactory
    {
        private readonly ExcelCategoriesExtractService excelCategoriesExtractService;
        private readonly TsvCategoriesExtractService tsvCategoriesExtractService;

        public CategoriesExtractFactory(
            ExcelCategoriesExtractService excelCategoriesExtractService,
            TsvCategoriesExtractService tsvCategoriesExtractService)
        {
            this.excelCategoriesExtractService = excelCategoriesExtractService;
            this.tsvCategoriesExtractService = tsvCategoriesExtractService;
        }

        public ICategoriesExtractService GetExtractService(CategoriesFileType type)
        {
            switch (type)
            {
                case CategoriesFileType.Excel:
                    return excelCategoriesExtractService;
                case CategoriesFileType.Tsv:
                    return tsvCategoriesExtractService;
                default:
                    throw new NotSupportedException("Unknown file type with categories");
            }
        }
    }
}
