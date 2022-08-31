using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Documents;
using MissingFieldException = CsvHelper.MissingFieldException;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class CategoricalOptionsImportService : ICategoricalOptionsImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly ICategoriesService categoriesService;

        public CategoricalOptionsImportService(IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            ICategoriesService categoriesService)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.categoriesService = categoriesService;
        }

        public ImportCategoricalOptionsResult ImportOptions(Stream file, string questionnaireId, Guid categoricalQuestionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (document == null)
                return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionnaireCantBeFound, questionnaireId));
            
            var question = document?.Find<ICategoricalQuestion>(categoricalQuestionId);
            if (question == null)
                return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionCannotBeFound, categoricalQuestionId));

            var importedOptions = new List<QuestionnaireCategoricalOption>();
            
            var cfg = this.CreateCsvConfiguration();

            if (question.CascadeFromQuestionId != null)
            {
                var parentCascadingQuestion = document!.Find<ICategoricalQuestion>(question.CascadeFromQuestionId.Value);

                if (parentCascadingQuestion == null)
                    return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionCannotBeFound,
                        question.CascadeFromQuestionId.Value));

                if ((parentCascadingQuestion.CategoriesId.HasValue && 
                    !this.categoriesService.GetCategoriesById(document.PublicKey, parentCascadingQuestion.CategoriesId.Value).Any()) || 
                    (!parentCascadingQuestion.CategoriesId.HasValue && parentCascadingQuestion.Answers.Count == 0))
                {
                    return ImportCategoricalOptionsResult.Failed(
                        string.Format(ExceptionMessages.NoParentCascadingOptions, parentCascadingQuestion.VariableName));
                }

                var allValuesByAllParents = GetAllValuesByAllParents(document, parentCascadingQuestion.PublicKey);

                cfg.RegisterClassMap(new CascadingOptionMap(allValuesByAllParents, importedOptions));
            }
            else cfg.RegisterClassMap<CategoricalOptionMap>();

            return ReadCategories(file, cfg, importedOptions);
        }

        private static ImportCategoricalOptionsResult ReadCategories(Stream file, CsvConfiguration cfg,
            List<QuestionnaireCategoricalOption> importedOptions)
        {
            var importErrors = new List<string>();

            using (var csvReader = new CsvReader(new StreamReader(file), cfg))
            {
                while (csvReader.Read())
                {
                    try
                    {
                        importedOptions.Add(csvReader.GetRecord<QuestionnaireCategoricalOption>());
                    }
                    catch (MissingFieldException)
                    {
                        importErrors.Add(ExceptionMessages.ImportOptions_MissingRequiredColumns);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException is CsvReaderException csvReaderException)
                            importErrors.Add(
                                $"({ExceptionMessages.Column}: {csvReaderException.ColumnIndex + 1}, " +
                                $"{ExceptionMessages.Row}: {csvReaderException.RowIndex}) " +
                                $"{csvReaderException.Message}");
                        else
                            importErrors.Add(e.Message);
                    }
                }
            }

            return importErrors.Count > 0
                ? ImportCategoricalOptionsResult.Failed(importErrors.ToArray())
                : ImportCategoricalOptionsResult.Success(importedOptions.ToArray());
        }

        private Dictionary<string, (int value, int? parentValue)[]> GetAllValuesByAllParents(
            QuestionnaireDocument document, Guid? parentQuestionId)
        {
            var result = new Dictionary<string, (int value, int? parentValue)[]>();

            ICategoricalQuestion? cascadingQuestion = null;

            while (parentQuestionId != null)
            {
                cascadingQuestion = document.Find<ICategoricalQuestion>(parentQuestionId.Value);
                if(cascadingQuestion == null)
                    throw new InvalidOperationException($"Question was not found ({parentQuestionId}).");

                result.Add(cascadingQuestion.VariableName,
                    cascadingQuestion.CategoriesId.HasValue
                        ? this.categoriesService.GetCategoriesById(document.PublicKey, cascadingQuestion.CategoriesId.Value).ToList().Select(x => (x.Id, x.ParentId)).ToArray()
                        : cascadingQuestion.Answers.Select(x => ((int) x.GetParsedValue(), x.GetParsedParentValue())).ToArray());

                parentQuestionId = cascadingQuestion.CascadeFromQuestionId;
            }

            return result;
        }

        public Stream ExportOptions(string questionnaireId, Guid categoricalQuestionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            var question = document?.Find<IQuestion>(categoricalQuestionId);
            if (question == null)
                throw new InvalidOperationException(string.Format(ExceptionMessages.QuestionCannotBeFound, categoricalQuestionId));

            var options = question.Answers?.Select(option => new QuestionnaireCategoricalOption
            {
                Value = (int) option.GetParsedValue(),
                ParentValue = option.GetParsedParentValue(),
                Title = option.AnswerText,
                AttachmentName = option.AttachmentName
            }) ?? Enumerable.Empty<QuestionnaireCategoricalOption>();

            var sb = new StringBuilder();

            var cfg = this.CreateCsvConfiguration();

            if (question.CascadeFromQuestionId.HasValue)
                cfg.RegisterClassMap<CascadingOptionMap>();
            else
                cfg.RegisterClassMap<CategoricalOptionMap>();

            using (var csvWriter = new CsvWriter(new StringWriter(sb), cfg))
                csvWriter.WriteRecords(options);

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        
        private CsvConfiguration CreateCsvConfiguration() => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            IgnoreQuotes = false,
            Delimiter = "\t",
            // Skip if all fields are empty.
            ShouldSkipRecord = record => record.All(string.IsNullOrEmpty)

        };

        private class CascadingOptionMap : CategoricalOptionMap
        {
            public CascadingOptionMap(): this(null, new List<QuestionnaireCategoricalOption>()) { }
            public CascadingOptionMap(Dictionary<string, (int value, int? parentValue)[]>? allValuesByAllParents, List<QuestionnaireCategoricalOption> allImportedOptions)
            {
                var values = allValuesByAllParents?.Values?.FirstOrDefault()?.Select(x => x.value);
                var nearestParentValues = values == null ? new HashSet<int>() : new HashSet<int>(values);

                Map(m => m.ParentValue).Index(2).TypeConverter(new ConvertToInt32AndCheckParentOptionValueOrThrow(nearestParentValues));
                
                Map(m => m.ValueWithParentValues).Ignore().ConvertUsing(x =>
                  {
                      if (!x.TryGetField(1, out string title) || !x.TryGetField(2, out int? parentValue) || !parentValue.HasValue) return null;

                      if (allImportedOptions.Any(y => y.ParentValue == parentValue && y.Title == title))
                          throw new CsvReaderException(x.Context.Row, 2,
                              string.Format(ExceptionMessages.ImportOptions_DuplicateByTitleAndParentIds, title, parentValue));

                      if (!x.TryGetField(0, out int value)) return null;

                      var valueWithParentValues = new List<int> { value };

                      if (allValuesByAllParents != null)
                      {
                          foreach (var parentValues in allValuesByAllParents)
                          {
                              var parentValuesById = parentValues.Value.Where(y => y.value == parentValue).ToArray();
                              if (parentValuesById.Length > 1)
                              {
                                  throw new CsvReaderException(x.Context.Row, 2,
                                      string.Format(ExceptionMessages.ImportOptions_DuplicatedParentValues,
                                          parentValues.Key, parentValuesById.Length, parentValue));
                              }

                              valueWithParentValues.Add(parentValue.Value);

                              parentValue = parentValuesById.FirstOrDefault().parentValue;

                              if (!parentValue.HasValue) break;
                          }
                      }

                      if (allImportedOptions.Any(y => 
                                 y.ValueWithParentValues == null 
                              || (y.ValueWithParentValues!= null &&  y.ValueWithParentValues.SequenceEqual(valueWithParentValues))))
                          throw new CsvReaderException(x.Context.Row, 0, string.Format(ExceptionMessages.ImportOptions_ValueIsNotUnique, title, value));

                      return valueWithParentValues.ToArray();
                  });
            }

            private class ConvertToInt32AndCheckParentOptionValueOrThrow : ConvertToInt32OrThrowConverter
            {
                private readonly HashSet<int> cascadingParentValues;

                public ConvertToInt32AndCheckParentOptionValueOrThrow(HashSet<int> cascadingParentValues)
                {
                    this.cascadingParentValues = cascadingParentValues;
                }

                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    var convertedValue = (int)base.ConvertFromString(text, row, memberMapData);

                    if (!this.cascadingParentValues.Contains(convertedValue))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index,
                            string.Format(ExceptionMessages.ImportOptions_ParentValueNotFound, convertedValue));


                    return convertedValue;
                }
            }
        }
        
        private class CategoricalOptionMap : ClassMap<QuestionnaireCategoricalOption>
        {
            protected CategoricalOptionMap()
            {
                Map(m => m.Value).Index(0).TypeConverter<ConvertToInt32OrThrowConverter>();
                Map(m => m.Title).Index(1).TypeConverter<ValidateTitleOrThrowConverter>();
            }

            private class ValidateTitleOrThrowConverter : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    if(string.IsNullOrEmpty(text))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index, ExceptionMessages.ImportOptions_EmptyValue);

                    if (text.Length > AbstractVerifier.MaxOptionLength)
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index,
                            string.Format(ExceptionMessages.ImportOptions_TitleTooLong, AbstractVerifier.MaxOptionLength));

                    return text;
                }
            }

            protected class ConvertToInt32OrThrowConverter : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    if (string.IsNullOrEmpty(text))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index, ExceptionMessages.ImportOptions_EmptyValue);

                    var numberStyle = memberMapData.TypeConverterOptions.NumberStyles ?? NumberStyles.Integer;

                    return int.TryParse(text, numberStyle, memberMapData.TypeConverterOptions.CultureInfo, out var i)
                        ? i
                        : throw new CsvReaderException(row.Context.Row, memberMapData.Index,
                            string.Format(ExceptionMessages.ImportOptions_NotNumber, text));
                }
            }
        }

        [Serializable]
        public class CsvReaderException : Exception
        {
            public readonly int? RowIndex;
            public readonly int? ColumnIndex;

            public CsvReaderException(int? rowIndex, int? columnIndex, string message) : base(message)
            {
                this.RowIndex = rowIndex;
                this.ColumnIndex = columnIndex;
            }
        }
    }
}
