using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClosedXML.Graphics;
using SixLabors.Fonts;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.ReusableCategories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class ExcelCategoriesExtractService : ICategoriesExtractService
    {
        private readonly ICategoriesVerifier verifier;
        private readonly ICategoriesExportService categoriesExportService;

        public ExcelCategoriesExtractService(ICategoriesVerifier verifier,
            ICategoriesExportService categoriesExportService)
        {
            this.verifier = verifier;
            this.categoriesExportService = categoriesExportService;
        }
        
        public byte[] GetTemplateFile(bool isCascading)
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            using XLWorkbook excelPackage = new XLWorkbook(loadOptions);
            var worksheet = excelPackage.Worksheets.Add("Categories");

            void FormatCell(string address)
            {
                var cell = worksheet.Cells(address);
                cell.Style.Font.Bold = true;
            }
            
            worksheet.Cells("A1").Value = CategoriesConstants.ValueColumnName;
            FormatCell("A1");
            worksheet.Cells("B1").Value = CategoriesConstants.TitleColumnName;
            FormatCell("B1");

            if (isCascading)
            {
                worksheet.Cells("C1").Value = CategoriesConstants.ParentValueColumnName;
                FormatCell("C1");
                worksheet.Cells("D1").Value = CategoriesConstants.AttachmentNameColumnName;
                FormatCell("D1");
            }
            else
            {
                worksheet.Cells("C1").Value = CategoriesConstants.AttachmentNameColumnName;
                FormatCell("C1");
            }

            using var stream = new MemoryStream();
            excelPackage.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GetAsFile(List<CategoriesItem> items, bool isCascading, bool hqImport)
        {
            return categoriesExportService.GetAsExcelFile(items, isCascading, hqImport);
        }

        public List<CategoriesRow> Extract(Stream file)
        {
            return file.WrapStreamIntoTempFile(excel => ExtractCategoriesFromExcelFile(excel));
        }

        private List<CategoriesRow> ExtractCategoriesFromExcelFile(Stream xmlFile)
        {
            //non windows fonts
            var firstFont = SystemFonts.Collection.Families.First();
            var loadOptions = new LoadOptions { GraphicEngine = new DefaultGraphicEngine(firstFont.Name) };
            
            var categories = new List<CategoriesRow>();
            using XLWorkbook package = new XLWorkbook(xmlFile, loadOptions);
            var worksheet = package.Worksheets.First();
            var headers = GetHeaders(worksheet);
            int firstDataRow = 2;
            
            if (string.IsNullOrEmpty(headers.IdIndex) || string.IsNullOrEmpty(headers.TextIndex))
            {
                if (headers.IdIndex == null && headers.TextIndex == null)
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = ExceptionMessages.HeaderWasNotFound,
                            }
                        })
                    };

                if (headers.IdIndex == null)
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "value")
                            }
                        })
                    };

                if (headers.TextIndex == null)
                    throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors)
                    {
                        FoundErrors = new List<ImportValidationError>(new[]
                        {
                            new ImportValidationError
                            {
                                Message = string.Format(ExceptionMessages.RequiredHeaderWasNotFound, "title")
                            }
                        })
                    };
            }

            var lastRowUsed = worksheet.LastRowUsed();
            if (lastRowUsed == null)
                throw new InvalidFileException(ExceptionMessages.Excel_NoCategories);
            
            var rowsCount = lastRowUsed.RowNumber();
            if (rowsCount > AbstractVerifier.MaxOptionsCountInFilteredComboboxQuestion + firstDataRow - 1)
                throw new InvalidFileException(
                    ExceptionMessages.Excel_Categories_More_Than_Limit.FormatString(AbstractVerifier
                        .MaxOptionsCountInFilteredComboboxQuestion));

            if (rowsCount == firstDataRow - 1)
                throw new InvalidFileException(ExceptionMessages.Excel_NoCategories);

            var errors = new List<ImportValidationError>();
            for (int rowNumber = firstDataRow; rowNumber <= rowsCount; rowNumber++)
            {
                var row = GetRowValues(worksheet, headers, rowNumber);

                if (string.IsNullOrEmpty(row.Id) && string.IsNullOrEmpty(row.ParentId) &&
                    string.IsNullOrEmpty(row.Text)) continue;

                var error = this.verifier.Verify(row, headers);

                if (error != null) errors.Add(error);
                else categories.Add(row);

                if (errors.Count >= 10) break;
            }

            if (errors.Any())
                throw new InvalidFileException(ExceptionMessages.ProvidedFileHasErrors) {FoundErrors = errors};

            this.verifier.VerifyAll(categories, headers);

            return categories;
        }

        private CategoriesHeaderMap GetHeaders(IXLWorksheet worksheet)
        {
            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(worksheet.Cell("A1").GetString(), "A"),
                new Tuple<string, string>(worksheet.Cell("B1").GetString(), "B"),
                new Tuple<string, string>(worksheet.Cell("C1").GetString(), "C"),
                new Tuple<string, string>(worksheet.Cell("D1").GetString(), "D")
            }.Where(kv => !string.IsNullOrEmpty(kv.Item1)).ToList();

            var headerMap = new CategoriesHeaderMap();
            for (int i = 0; i < headers.Count; i++)
            {
                var rowValue = headers[i];

                switch (rowValue.Item1)
                {
                    case CategoriesConstants.TitleColumnName:       
                    case CategoriesConstants.OldTitleColumnName:       
                        headerMap.TextIndex = rowValue.Item2; break;
                    case CategoriesConstants.ValueColumnName:       
                    case CategoriesConstants.OldValueColumnName:       
                        headerMap.IdIndex = rowValue.Item2; break;
                    case CategoriesConstants.ParentValueColumnName: 
                    case CategoriesConstants.OldParentValueColumnName: 
                        headerMap.ParentIdIndex = rowValue.Item2; break;
                    case CategoriesConstants.AttachmentNameColumnName: 
                        headerMap.AttachmentNameIndex = rowValue.Item2; break;
                }
            }

            return headerMap;
        }

        private CategoriesRow GetRowValues(IXLWorksheet worksheet, CategoriesHeaderMap headers, int rowNumber) => new CategoriesRow
        {
            Id = worksheet.Cell($"{headers.IdIndex}{rowNumber}").GetString(),
            Text = worksheet.Cell($"{headers.TextIndex}{rowNumber}").GetString(),
            ParentId = headers.ParentIdIndex != null ? worksheet.Cell($"{headers.ParentIdIndex}{rowNumber}").GetString() : null,
            RowId = rowNumber,
            AttachmentName = headers.AttachmentNameIndex != null ? worksheet.Cell($"{headers.AttachmentNameIndex}{rowNumber}").GetString() : null,
        };
    }
}
