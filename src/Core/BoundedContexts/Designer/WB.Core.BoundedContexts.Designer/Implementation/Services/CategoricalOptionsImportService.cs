﻿using System;
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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class CategoricalOptionsImportService : ICategoricalOptionsImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        public CategoricalOptionsImportService(IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
        }

        public ImportCategoricalOptionsResult ImportOptions(Stream file, string questionnaireId, Guid categoricalQuestionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            var question = document?.Find<IQuestion>(categoricalQuestionId);

            if (question == null)
                return ImportCategoricalOptionsResult.Failed(string.Format(ExceptionMessages.QuestionCannotBeFound, categoricalQuestionId));

            var importedOptions = new List<QuestionnaireCategoricalOption>();
            var importErrors = new List<string>();
            var cfg = this.CreateCsvConfiguration();

            var isCascadingQuestion = question.CascadeFromQuestionId.HasValue;
            if (isCascadingQuestion)
            {
                var parentCascadingQuestion = document.Find<IQuestion>(question.CascadeFromQuestionId.Value);
                if (parentCascadingQuestion.Answers.Count == 0)
                {
                    return ImportCategoricalOptionsResult.Failed(
                        string.Format(ExceptionMessages.NoParentCascadingOptions, parentCascadingQuestion.VariableName));
                }

                var allValuesByAllParents = GetAllValuesByAllParents(document, parentCascadingQuestion.PublicKey);

                cfg.RegisterClassMap(new CascadingOptionMap(allValuesByAllParents, importedOptions));
            }
            else cfg.RegisterClassMap<CategoricalOptionMap>();

            using (var csvReader = new CsvReader(new StreamReader(file), cfg))
            {
                while (csvReader.Read())
                {
                    try
                    {
                        importedOptions.Add(csvReader.GetRecord<QuestionnaireCategoricalOption>());
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

            IQuestion cascadingQuestion = null;

            while (parentQuestionId != null)
            {
                cascadingQuestion = document.Find<IQuestion>(parentQuestionId.Value);

                result.Add(cascadingQuestion.VariableName,
                    cascadingQuestion.Answers.Select(x => ((int) x.GetParsedValue(), x.GetParsedParentValue())).ToArray());

                parentQuestionId = cascadingQuestion.CascadeFromQuestionId;
            }

            return result;
        }

        public Stream ExportOptions(string questionnaireId, Guid categoricalQuestionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            var question = document?.Find<IQuestion>(categoricalQuestionId);

            var options = question.Answers?.Select(option => new QuestionnaireCategoricalOption
            {
                Value = (int) option.GetParsedValue(),
                ParentValue = option.GetParsedParentValue(),
                Title = option.AnswerText
            }) ?? Enumerable.Empty<QuestionnaireCategoricalOption>();

            var sb = new StringBuilder();

            var cfg = this.CreateCsvConfiguration();

            if (question.CascadeFromQuestionId.HasValue)
                cfg.RegisterClassMap<CascadingOptionMap>();
            else
                cfg.RegisterClassMap<CategoricalOptionMap>();

            using (var csvWriter = new CsvWriter(new StringWriter(sb), cfg))
                csvWriter.WriteRecords(options);

            return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
        }
        
        private Configuration CreateCsvConfiguration() => new Configuration
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
            public CascadingOptionMap(): this(null, null) { }
            public CascadingOptionMap(Dictionary<string, (int value, int? parentValue)[]> allValuesByAllParents, List<QuestionnaireCategoricalOption> allImportedOptions)
            {
                var nearestParentValues = allValuesByAllParents.Values.First().Select(x => x.value).ToHashSet();

                Map(m => m.ParentValue).Index(2).TypeConverter(new ConvertToInt32AndCheckParentOptionValueOrThrow(nearestParentValues));
                Map(m => m.TitleWithParentIds).Ignore().ConvertUsing(x =>
                {
                    if (!x.TryGetField(1, out string title) || !x.TryGetField(2, out int? parentValue) || !parentValue.HasValue) return null;

                    var nearestParentValue = parentValue;
                    var titleWithParentIds = title;

                    foreach (var parentValues in allValuesByAllParents)
                    {
                        var parentValuesById = parentValues.Value.Where(y => y.value == parentValue).ToArray();
                        if (parentValuesById.Length > 1)
                        {
                            throw new CsvReaderException(x.Context.Row, 2,
                                string.Format(ExceptionMessages.ImportOptions_DuplicatedParentValues,
                                    parentValues.Key, parentValuesById.Length, parentValue));
                        }

                        titleWithParentIds += parentValue;

                        parentValue = parentValuesById.FirstOrDefault().parentValue;

                        if (!parentValue.HasValue) break;
                    }

                    if (allImportedOptions.Any(y => y.TitleWithParentIds == titleWithParentIds))
                        throw new CsvReaderException(x.Context.Row, 2,
                            string.Format(ExceptionMessages.ImportOptions_DuplicateByTitleAndParentIds, title, nearestParentValue));

                    return titleWithParentIds;
                });
            }

            private class ConvertToInt32AndCheckParentOptionValueOrThrow : ConvertToInt32OrThrow
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
            public CategoricalOptionMap()
            {
                Map(m => m.Value).Index(0).TypeConverter<ConvertToInt32OrThrow>();
                Map(m => m.Title).Index(1).TypeConverter<ConvertToStringOrThrow>();
            }

            private class ConvertToStringOrThrow : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    return !string.IsNullOrEmpty(text)
                        ? text
                        : throw new CsvReaderException(row.Context.Row, memberMapData.Index, ExceptionMessages.ImportOptions_EmptyValue);
                }
            }

            protected class ConvertToInt32OrThrow : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    if (string.IsNullOrEmpty(text))
                        throw new CsvReaderException(row.Context.Row, memberMapData.Index, ExceptionMessages.ImportOptions_EmptyValue);

                    var numberStyle = memberMapData.TypeConverterOptions.NumberStyle ?? NumberStyles.Integer;

                    return int.TryParse(text, numberStyle, memberMapData.TypeConverterOptions.CultureInfo, out var i)
                        ? i
                        : throw new CsvReaderException(row.Context.Row, memberMapData.Index,
                            string.Format(ExceptionMessages.ImportOptions_NotNumber, text));
                }
            }
        }

        private class CsvReaderException : Exception
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
