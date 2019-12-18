using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Tests.Unit.Designer.Services
{
    [TestOf(typeof(CategoriesService))]
    internal class CategoriesServiceTests
    {
        private static CategoriesService CreateCategoriesService(DesignerDbContext dbContext = null, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage = null, 
            ICategoriesExportService categoriesExportService = null)
        {
            return new CategoriesService(
                dbContext: dbContext ?? Mock.Of<DesignerDbContext>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                categoriesExportService: categoriesExportService ?? Mock.Of<ICategoriesExportService>());
        }

        private static byte[] CreateExcelWithHeader(string[][] data)
        {
            var listOfData = data.ToList();
            listOfData.Insert(0, new[] {"id", "text", "parentid"});

            return CreateExcel(listOfData.ToArray());
        }

        private static byte[] CreateExcel(string[][] data)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Categories");

                for (var row = 0; row < data.Length; row++)
                for (var column = 0; column < data[row].Length; column++)
                    worksheet.Cells[row + 1, column + 1].Value = data[row][column];

                return package.GetAsByteArray();
            }
        }

        [Test]
        public void when_store_and_excel_file_has_more_than_15000_categories_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[15001].Select((x, i) => new[] {i.ToString(), $"opt {i}", ""}).ToArray();
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.Message, Contains.Substring("more than 15000 categories"));
        }

        [Test]
        public void when_store_and_excel_file_hasnt_id_column_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"text", "parentid"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcel(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("id was not found"));
        }

        [Test]
        public void when_store_and_excel_file_hasnt_text_column_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"id", "parentid"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcel(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("text was not found"));
        }

        [Test]
        public void when_store_excel_file_with_category_with_empty_id_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"", "option 1", "1"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("[A2] Empty value"));
        }

        [Test]
        public void when_store_excel_file_with_category_with_not_numeric_id_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"not numeric id", "option 1", "1"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("[A2] Invalid numeric value"));
        }

        [Test]
        public void when_store_excel_file_with_category_with_not_numeric_parent_id_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"1", "option 1", "not numeric id"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("[C2] Invalid numeric value"));
        }

        [Test]
        public void when_store_excel_file_with_category_with_empty_text_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"1", "", "1"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Contains.Substring("[B2] Empty text"));
        }

        [Test]
        public void when_store_excel_file_without_categories_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[0][];
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.Message, Contains.Substring("No categories"));
        }

        [Test]
        public void when_store_excel_file_with_1_category_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"1", "option 1", "1"}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.Message, Contains.Substring("at least 2 categories"));
        }

        [Test]
        public void when_store_excel_file_with_empty_category_rows_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][] {new[] {"", "", ""}, new[] {"", "", ""}};
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.Message, Contains.Substring("No categories"));
        }

        [Test]
        public void when_store_excel_file_with_category_text_more_than_250_chars_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][]
            {
                new[] {"1", "option 1", "1"}, new[] {"2", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "1"},
                new[] {"3", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "1"}
            };
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.Exactly(2).Items);
            Assert.That(exception.FoundErrors.Select(x => x.Message), Has.All.Contains("should be less than 250 characters"));
        }

        [Test]
        public void when_store_excel_file_with_2_categories_with_parentid_and_without_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][]
            {
                new[] {"1", "option 1", "1"},
                new[] {"2", "option 2", ""},
            };
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.Message, Contains.Substring("don't have a parent id"));
        }

        [Test]
        public void when_store_excel_file_with_2_categories_with_the_same_id_and_parentid_then_should_throw_excel_exception()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][]
            {
                new[] {"1", "option 1", "1"}, 
                new[] {"1", "option 2", "1"} 
            };
            var service = CreateCategoriesService();

            // act
            var exception = Assert.Throws<InvalidExcelFileException>(() =>
                service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data)));

            // assert
            Assert.That(exception.FoundErrors, Has.One.Items);
            Assert.That(exception.FoundErrors[0].Message, Is.EqualTo("Duplicated categories in rows: 2,3"));
        }

        [Test]
        public void when_store_excel_file_with_2_categories_and_1_empty_row_then_empty_row_should_be_ignored()
        {
            // arrange
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var categoriesId = Guid.Parse("22222222222222222222222222222222");
            var data = new string[][]
            {
                new[] {"1", "option 1", "1"}, 
                new[] {"", "", ""}, 
                new[] {"2", "option 2", "1"} 
            };
            var mockOfDbContext = new Mock<DesignerDbContext>();
            var service = CreateCategoriesService(dbContext: mockOfDbContext.Object);

            // act
            service.Store(questionnaireId, categoriesId, CreateExcelWithHeader(data));

            // assert
            mockOfDbContext.Verify(x => x.AddRange(Moq.It.Is<IEnumerable<CategoriesInstance>>(y => y.Count() == 2)), Times.Once);
        }
    }
}
