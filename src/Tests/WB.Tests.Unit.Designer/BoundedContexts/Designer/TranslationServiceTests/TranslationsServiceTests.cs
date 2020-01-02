using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    [TestOf(typeof(TranslationsService))]
    internal class TranslationsServiceTests : TranslationsServiceTestsContext
    {
        [Test]
        public void when_storing_translations_with_html_from_excel_file()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslationsWithHtml.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);

            byte[] fileStream;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("11111111111111111111111111111111"), title:"Section Text"),
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.TranslationInstances.First().Value, Is.EqualTo("Текст секции"));
        }

        [Test]
        public void when_storing_translations_with_permitted_tags_from_excel_file()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            Guid sectionId = Guid.Parse("d1111111111111111111111111111111");
            Guid Question1Id = Guid.Parse("a1111111111111111111111111111111");
            Guid StaticText2Id = Guid.Parse("b1111111111111111111111111111111");
            Guid Question3Id = Guid.Parse("c1111111111111111111111111111111");
            Guid Roster1Id = Guid.Parse("d1111111111111111111111111111119");

            var testType = typeof(TranslationsServiceTests);
            var readResourceFile = testType.Namespace + ".testTranslationsWithPermittedTags.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);

            byte[] fileStream;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Group(groupId: sectionId, title:"Section Text",
                    children:new IComposite[]
                    {
                        Create.NumericIntegerQuestion(Question1Id, options:new Option[]{new Option(){Value = "1"} }),
                        Create.StaticText(StaticText2Id, validationConditions:new ValidationCondition[]{new ValidationCondition("1==1","Test")}),
                        Create.Roster(Roster1Id),
                        Create.SingleOptionQuestion(Question3Id, 
                            answers: new List<Answer>()
                                {
                                    new Answer(){AnswerCode = 1, AnswerValue = "1", AnswerText = "o1"},
                                    new Answer(){AnswerCode = 2, AnswerValue = "2", AnswerText = "o2"}
                                })
                    })
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.TranslationInstances.Count(), Is.EqualTo(13));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x=> x.QuestionnaireEntityId == sectionId && x.Type == TranslationType.Title).Value, Is.EqualTo("Section title"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Question1Id && x.Type == TranslationType.Title).Value, Is.EqualTo("<b>Question title</b>"));
            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Question1Id && 
                                                                      x.Type == TranslationType.ValidationMessage && 
                                                                      x.TranslationIndex == "1").Value, Is.EqualTo("Question validation"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Question1Id && 
                                                                      x.Type == TranslationType.SpecialValue && 
                                                                      x.TranslationIndex == "1").Value, Is.EqualTo("Special value"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == StaticText2Id && 
                                                                      x.Type == TranslationType.Title).Value, Is.EqualTo("<b>Static text title</b>"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == StaticText2Id &&
                                                                      x.Type == TranslationType.ValidationMessage &&
                                                                      x.TranslationIndex == "1").Value, Is.EqualTo("Static text validation"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Roster1Id && 
                                                                      x.Type == TranslationType.Title).Value, Is.EqualTo("Test"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Roster1Id &&
                                                                      x.Type == TranslationType.FixedRosterTitle &&
                                                                      x.TranslationIndex == "1").Value, Is.EqualTo("Test1"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Question3Id && 
                                                                      x.Type == TranslationType.OptionTitle &&
                                                                      x.TranslationIndex == "1").Value, Is.EqualTo("Option1"));

            Assert.That(plainStorageAccessor.TranslationInstances.Single(x => x.QuestionnaireEntityId == Question3Id &&
                                                                      x.Type == TranslationType.Instruction).Value, Is.EqualTo("<b>Instruction</b>"));

        }

        [Test]
        public void when_storing_translations_from_excel_file_without_variable_column()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            Guid sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            byte[] fileStream = GetEmbendedFileContent("testTranslationsWithoutVariableColumn.xlsx");

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Group(groupId: sectionId, title:"Section Text")
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.TranslationInstances.Count().Equals(1));

            var translationInstance = plainStorageAccessor.TranslationInstances.First();
            Assert.That(translationInstance.Value, Is.EqualTo("title"));
            Assert.That(translationInstance.QuestionnaireEntityId, Is.EqualTo(sectionId));
            Assert.That(translationInstance.Type, Is.EqualTo(TranslationType.Title));
            Assert.That(translationInstance.QuestionnaireId, Is.EqualTo(questionnaireId));
        }

        [Test]
        public void when_storing_translations_from_excel_file_with_columns_in_random_order()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            Guid sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            byte[] fileStream = GetEmbendedFileContent("testTranslationsWithRandomOrderColumns.xlsx");

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Group(groupId: sectionId, title:"Section Text")
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.TranslationInstances.Count(), Is.EqualTo(1));

            var translationInstance = plainStorageAccessor.TranslationInstances.First();
            Assert.That(translationInstance.Value, Is.EqualTo("title"));
            Assert.That(translationInstance.QuestionnaireEntityId, Is.EqualTo(sectionId));
            Assert.That(translationInstance.Type, Is.EqualTo(TranslationType.Title));
            Assert.That(translationInstance.QuestionnaireId, Is.EqualTo(questionnaireId));
        }


        [Test]
        public void when_verifying_translations_from_excel_file_without_entity_id_column()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            Guid sectionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            byte[] fileStream = GetEmbendedFileContent("testTranslationsWithoutEntityIdColumn.xlsx");

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new IComposite[]
            {
                Create.Group(groupId: sectionId, title:"Section Text")
            });

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            var exception = Assert.Throws<InvalidFileException>(() => service.Store(questionnaireId, translationId, fileStream));

            //assert
            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.FoundErrors.Count, Is.EqualTo(1));

            var validationError = exception.FoundErrors.Single();
            Assert.That(validationError.Message.Contains(TranslationExcelOptions.EntityIdColumnName), Is.True);
        }

        [Test]
        public void when_verifying_translations_from_excel_file_with_categories_and_without_translation_column()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var categoriesId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var categoriesName = "mycat";


            byte[] fileStream = CreateExcel(categoriesName, new[]
            {
                new[]
                {
                    "Index", "Original text"
                }
            });

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId);
            questionnaire.Categories = new List<Categories>
            {
                new Categories{ Id = categoriesId, Name = categoriesName}
            };

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            var exception = Assert.Throws<InvalidFileException>(() => service.Store(questionnaireId, translationId, fileStream));

            //assert
            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.FoundErrors.Count, Is.EqualTo(1));

            var validationError = exception.FoundErrors.Single();
            Assert.That(validationError.Message.Contains(TranslationExcelOptions.TranslationTextColumnName), Is.True);
        }

        [Test]
        public void when_verifying_translations_from_excel_file_with_categories_and_without_index_column()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var categoriesId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var categoriesName = "mycat";


            byte[] fileStream = CreateExcel(categoriesName, new[]
            {
                new[]
                {
                    "Original text", "Translation"
                }
            });

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId);
            questionnaire.Categories = new List<Categories>
            {
                new Categories{ Id = categoriesId, Name = categoriesName}
            };

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            var exception = Assert.Throws<InvalidFileException>(() => service.Store(questionnaireId, translationId, fileStream));

            //assert
            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.FoundErrors.Count, Is.EqualTo(1));

            var validationError = exception.FoundErrors.Single();
            Assert.That(validationError.Message.Contains(TranslationExcelOptions.OptionValueOrValidationIndexOrFixedRosterIdIndexColumnName), Is.True);
        }

        [Test]
        public void when_verifying_translations_from_excel_file_with_categories_and_index_cell_is_empty()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var categoriesId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var categoriesName = "mycat";


            byte[] fileStream = CreateExcelWithHeader(categoriesName, new[]
            {
                new[]
                {
                    "", "original text", "translation"
                }
            });

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId);
            questionnaire.Categories = new List<Categories>
            {
                new Categories{ Id = categoriesId, Name = categoriesName}
            };

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            var exception = Assert.Throws<InvalidFileException>(() => service.Store(questionnaireId, translationId, fileStream));

            //assert
            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.FoundErrors.Count, Is.EqualTo(1));

            var validationError = exception.FoundErrors.Single();
            Assert.That(validationError.Message.Contains("invalid index at [A2]"), Is.True);
        }

        [Test]
        public void when_storing_translations_from_excel_file_with_categories()
        {
            //assert
            Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var categoriesId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var categoriesName = "mycat";

            byte[] fileStream = CreateExcelWithHeader(categoriesName, new[]
            {
                new[]
                {
                    "1$1", "original text", "translation"
                }
            });

            var plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(questionnaireId);
            questionnaire.Categories = new List<Categories>
            {
                new Categories{ Id = categoriesId, Name = categoriesName}
            };

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            //act
            service.Store(questionnaireId, translationId, fileStream);

            //assert
            Assert.That(plainStorageAccessor.TranslationInstances.Count(), Is.EqualTo(1));

            var translationInstance = plainStorageAccessor.TranslationInstances.First();
            Assert.That(translationInstance.Value, Is.EqualTo("translation"));
            Assert.That(translationInstance.QuestionnaireEntityId, Is.EqualTo(categoriesId));
            Assert.That(translationInstance.Type, Is.EqualTo(TranslationType.Categories));
            Assert.That(translationInstance.QuestionnaireId, Is.EqualTo(questionnaireId));
            Assert.That(translationInstance.TranslationIndex, Is.EqualTo("1$1"));
        }

        private byte[] GetEmbendedFileContent(string fileName)
        {
            var testType = typeof(TranslationsServiceTests);
            var readResourceFile = testType.Namespace + "." + fileName;
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static byte[] CreateExcelWithHeader(string categoriesName, string[][] data)
        {
            var listOfData = data.ToList();
            listOfData.Insert(0, new[] {"Index", "Original text", "Translation"});

            return CreateExcel(categoriesName, listOfData.ToArray());
        }

        private static byte[] CreateExcel(string categoriesName, string[][] data)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add($"@@@_{categoriesName}");

                for (var row = 0; row < data.Length; row++)
                for (var column = 0; column < data[row].Length; column++)
                    worksheet.Cells[row + 1, column + 1].Value = data[row][column];

                return package.GetAsByteArray();
            }
        }
    }
}
