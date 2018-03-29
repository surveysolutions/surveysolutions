using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentsImportService : IAssignmentsImportService
    {
        private readonly ICsvReader csvReader;
        private readonly IArchiveUtils archiveUtils;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly string[] permittedFileExtensions = { TabExportFile.Extention, TextExportFile.Extension };

        private static readonly string[] ignoredPreloadingColumns =
            ServiceColumns.SystemVariables.Values.Select(x => x.VariableExportColumnName).ToArray();

        public AssignmentsImportService(ICsvReader csvReader, IArchiveUtils archiveUtils, 
            IQuestionnaireStorage questionnaireStorage, IUserViewFactory userViewFactory)
        {
            this.csvReader = csvReader;
            this.archiveUtils = archiveUtils;
            this.questionnaireStorage = questionnaireStorage;
            this.userViewFactory = userViewFactory;
        }

        public IEnumerable<AssignmentRow> GetAssignmentRows(QuestionnaireIdentity questionnaireIdentity, PreloadedFile file)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            foreach (var preloadingRow in file.Rows)
            {
                var assignmentAnswers = this.ToAssignmentAnswers(file.FileInfo.FileName,
                    preloadingRow, questionnaire);

                yield return new AssignmentRow
                {
                    Answers = assignmentAnswers.ToArray()
                };
            }
        }

        private IEnumerable<AssignmentValue> ToAssignmentAnswers(string fileName, PreloadingRow row, IQuestionnaire questionnaire)
        {
            foreach (var answer in row.Cells)
            {
                switch (answer)
                {
                    case PreloadingCompositeValue compositeCell:
                        yield return ToAssignmentAnswers(fileName, row, compositeCell, questionnaire);
                        break;
                    case PreloadingRosterInstanceIdValue rosterInstanceIdCell:
                        yield return this.ToAssignmentRosterInstanceId(fileName, row, rosterInstanceIdCell, questionnaire);
                        break;
                    case PreloadingValue regularCell:
                        switch (answer.VariableOrCodeOrPropertyName)
                        {
                            case ServiceColumns.ResponsibleColumnName:
                                yield return this.ToAssignmentResponsible(fileName, row.InterviewId, regularCell);
                                break;
                            case ServiceColumns.AssignmentsCountColumnName:
                                yield return ToAssignmentQuantity(fileName, row.InterviewId, regularCell);
                                break;
                            default:
                                yield return ToAssignmentAnswer(fileName, row, regularCell, questionnaire);
                                break;
                        }
                        break;
                }
            }

        }

        private static AssignmentAnswers ToAssignmentAnswers(string fileName, PreloadingRow row,
            PreloadingCompositeValue compositeValue, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(compositeValue.VariableOrCodeOrPropertyName);

            if (questionId.HasValue)
            {
                var questionType = GetQuestionType(questionId.Value, questionnaire);

                switch (questionType)
                {
                    case InterviewQuestionType.YesNo:
                    case InterviewQuestionType.TextList:
                    case InterviewQuestionType.Gps:
                    case InterviewQuestionType.MultiFixedOption:
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return new AssignmentAnswers
            {
                VariableName = compositeValue.VariableOrCodeOrPropertyName,
                Values = compositeValue.Values.Select(x => ToAssignmentAnswer(fileName, row, x, questionnaire))
                    .ToArray(),
            };
        }

        private AssignmentValue ToAssignmentRosterInstanceId(string fileName, PreloadingRow row,
            PreloadingRosterInstanceIdValue answer, IQuestionnaire questionnaire)
        {
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentRosterInstanceCode
            {
                Code = intValue,
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
            };
        }

        private static AssignmentAnswer ToAssignmentAnswer(string fileName, PreloadingRow row,
            PreloadingValue answer, IQuestionnaire questionnaire)
        {
            var questionId = questionnaire.GetQuestionIdByVariable(answer.VariableOrCodeOrPropertyName);

            if (questionId.HasValue)
            {
                var questionType = GetQuestionType(questionId.Value, questionnaire);

                switch (questionType)
                {
                    case InterviewQuestionType.Text:
                    case InterviewQuestionType.QRBarcode:
                    case InterviewQuestionType.TextList:
                        return ToAssignmentTextAnswer(questionId.Value, questionnaire, fileName, row, answer);
                    case InterviewQuestionType.Integer:
                        return ToAssignmentIntegerAnswer(questionId.Value, questionnaire, fileName, row, answer);
                    case InterviewQuestionType.SingleFixedOption:
                    case InterviewQuestionType.Cascading:
                    case InterviewQuestionType.Double:
                        return ToAssignmentDoubleAnswer(questionId.Value, fileName, row, answer);
                    case InterviewQuestionType.DateTime:
                        return ToAssignmentDateTimeAnswer(questionId.Value, fileName, row, answer);
                    default:
                        throw new NotSupportedException();
                }
            }

            return new AssignmentAnswer();
        }

        private static AssignmentAnswer ToAssignmentDoubleAnswer(Guid questionId, string fileName, PreloadingRow row, PreloadingValue answer)
        {
            double? doubleValue = null;
            if (double.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var doubleNumericValue))
                doubleValue = doubleNumericValue;

            return new AssignmentDoubleAnswer
            {
                Answer = doubleValue,
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
        };
        }

        private static AssignmentAnswer ToAssignmentDateTimeAnswer(Guid questionId, string fileName, PreloadingRow row, PreloadingValue answer)
        {
            DateTime? dataTimeValue = null;
            if (DateTime.TryParse(answer.Value, out var date))
                dataTimeValue = date;

            return new AssignmentDateTimeAnswer
            {
                Answer = dataTimeValue,
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
            };
        }

        private static AssignmentTextAnswer ToAssignmentTextAnswer(Guid questionId, IQuestionnaire questionnaire, string fileName, PreloadingRow row, PreloadingValue answer)
            => new AssignmentTextAnswer
            {
                Mask = questionnaire.GetTextQuestionMask(questionId),
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
            };

        private static AssignmentAnswer ToAssignmentIntegerAnswer(Guid questionId, IQuestionnaire questionnaire, string fileName, PreloadingRow row, PreloadingValue answer)
        {
            int? intValue = null;
            if (int.TryParse(answer.Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat,
                out var intNumericValue))
                intValue = intNumericValue;

            return new AssignmentIntegerAnswer
            {
                IsRosterSize = questionnaire.IsRosterSizeQuestion(questionId),
                IsRosterSizeForLongRoster = questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId),
                Answer = intValue,
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value,
            };
        }

        private static AssignmentQuantity ToAssignmentQuantity(string fileName, string interviewId, PreloadingValue answer)
        {
            int? quantityValue = null;

            if (int.TryParse(answer.Value, out var quantity))
                quantityValue = quantity;

            return new AssignmentQuantity
            {
                InterviewId = interviewId,
                Quantity = quantityValue,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value
            };
        }

        private readonly Dictionary<string, UserToVerify> users = new Dictionary<string, UserToVerify>();
        private AssignmentResponsible ToAssignmentResponsible(string fileName, string interviewId, PreloadingValue answer)
        {
            var responsible = new AssignmentResponsible
            {
                InterviewId = interviewId,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value
            };

            var responsibleName = answer.Value;
            if (!string.IsNullOrWhiteSpace(responsibleName))
            {
                if (!users.ContainsKey(responsibleName))
                    users.Add(responsibleName, this.userViewFactory.GetUsersByUserNames(new[] { responsibleName }).FirstOrDefault());

                responsible.Responsible = users[responsibleName];
            }

            return responsible;
        }

        private static InterviewQuestionType GetQuestionType(Guid questionId, IQuestionnaire questionnaire)
        {
            var questionType = questionnaire.GetQuestionType(questionId);
            bool isYesNo = questionnaire.IsQuestionYesNo(questionId);
            bool isDecimal = !questionnaire.IsQuestionInteger(questionId);
            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionId);
            Guid? sourceForLinkedQuestion = null;

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId);
            var isLinkedToListQuestion = questionnaire.IsLinkedToListQuestion(questionId);

            if (isLinkedToQuestion)
                sourceForLinkedQuestion = questionnaire.GetQuestionReferencedByLinkedQuestion(questionId);

            if (isLinkedToRoster)
                sourceForLinkedQuestion = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);

            switch (questionType)
            {
                case QuestionType.SingleOption:
                    {
                        return sourceForLinkedQuestion.HasValue
                            ? (isLinkedToListQuestion
                                ? InterviewQuestionType.SingleLinkedToList
                                : InterviewQuestionType.SingleLinkedOption)
                            : (cascadingParentQuestionId.HasValue
                                ? InterviewQuestionType.Cascading
                                : InterviewQuestionType.SingleFixedOption);
                    }
                case QuestionType.MultyOption:
                    {
                        return isYesNo
                            ? InterviewQuestionType.YesNo
                            : (sourceForLinkedQuestion.HasValue
                                ? (isLinkedToListQuestion
                                    ? InterviewQuestionType.MultiLinkedToList
                                    : InterviewQuestionType.MultiLinkedOption)
                                : InterviewQuestionType.MultiFixedOption);
                    }
                case QuestionType.DateTime:
                    return InterviewQuestionType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewQuestionType.Gps;
                case QuestionType.Multimedia:
                    return InterviewQuestionType.Multimedia;
                case QuestionType.Numeric:
                    return isDecimal ? InterviewQuestionType.Double : InterviewQuestionType.Integer;
                case QuestionType.QRBarcode:
                    return InterviewQuestionType.QRBarcode;
                case QuestionType.Area:
                    return InterviewQuestionType.Area;
                case QuestionType.Text:
                    return InterviewQuestionType.Text;
                case QuestionType.TextList:
                    return InterviewQuestionType.TextList;
                case QuestionType.Audio:
                    return InterviewQuestionType.Audio;
                default:
                    throw new NotSupportedException($"Not supported question type: {questionType}");
            }
        }

        private PreloadingRow ToRow(int rowIndex, ExpandoObject record)
        {
            var cells = new Dictionary<string, List<PreloadingValue>>();
            string interviewId = null;
            
            foreach (var kv in record)
            {
                var columnName = kv.Key.ToLower();
                var value = (string) kv.Value;

                if (ignoredPreloadingColumns.Contains(columnName)) continue;
                if (columnName == ServiceColumns.InterviewId)
                {
                    interviewId = value;
                    continue;
                }

                var compositeColumnValues = columnName.Split(new[] { QuestionDataParser.ColumnDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                var variableName = compositeColumnValues[0].ToLower();

                if (!cells.ContainsKey(variableName))
                    cells[variableName] = new List<PreloadingValue>();

                var isRosterInstanceIdValue = string.Format(ServiceColumns.IdSuffixFormat, variableName) == columnName;
                if (isRosterInstanceIdValue)
                {
                    cells[variableName].Add(new PreloadingRosterInstanceIdValue
                    {
                        VariableOrCodeOrPropertyName = columnName,
                        Row = rowIndex,
                        Column = kv.Key,
                        Value = value
                    });
                    continue;
                }

                cells[variableName].Add(new PreloadingValue
                {
                    VariableOrCodeOrPropertyName =
                        compositeColumnValues.Length > 1 ? compositeColumnValues[1] : variableName,
                    Row = rowIndex,
                    Column = kv.Key,
                    Value = value.Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingQuantityValue, string.Empty),
                });
            }

            return new PreloadingRow
            {
                InterviewId = interviewId,
                Cells = cells.Select(x => x.Value.Count == 1
                    ? x.Value[0]
                    : (PreloadingCell) new PreloadingCompositeValue
                    {
                        VariableOrCodeOrPropertyName = x.Key,
                        Values = x.Value.ToArray()
                    }).ToArray()
            };
        }

        public PreloadedFile ParseText(Stream inputStream, string fileName) => new PreloadedFile
        {
            FileInfo = new PreloadedFileInfo
            {
                FileName = fileName,
                QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName),
                Columns = this.csvReader.ReadHeader(inputStream, TabExportFile.Delimiter),
            },
            Rows = this.csvReader.GetRecords(inputStream, TabExportFile.Delimiter)
                .Select((record, rowIndex) => (PreloadingRow) this.ToRow(rowIndex + 1, record)).ToArray()
        };

        public IEnumerable<PreloadedFile> ParseZip(Stream inputStream)
        {
            if(!this.archiveUtils.IsZipStream(inputStream))
                yield break;
            
            foreach (var file in this.archiveUtils.GetFilesFromArchive(inputStream))
                yield return this.ParseText(new MemoryStream(file.Bytes), file.Name);
        }

        public IEnumerable<PreloadedFileMetaData> ParseZipMetadata(Stream inputStream)
        {
            if (!this.archiveUtils.IsZipStream(inputStream))
                yield break;

            foreach (var fileInfo in this.archiveUtils.GetArchivedFileNamesAndSize(inputStream))
            {
                yield return new PreloadedFileMetaData(fileInfo.Key, fileInfo.Value,
                    permittedFileExtensions.Contains(Path.GetExtension(fileInfo.Key)));
            }
        }
    }
}