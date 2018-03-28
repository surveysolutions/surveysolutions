using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
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
            var rosterId = questionnaire.GetRosterIdByVariableName(file.QuestionnaireOrRosterName, true);

            var rosterSizeQuestionColumns = rosterId.HasValue
                ? questionnaire.GetRostersFromTopToSpecifiedEntity(rosterId.Value)
                    .Select(questionnaire.GetRosterVariableName)
                    .Union(new[] {questionnaire.GetRosterVariableName(rosterId.Value)})
                    .Select(x => string.Format(ServiceColumns.IdSuffixFormat, x)).ToArray()
                : new string[0];

            foreach (var preloadingRow in file.Rows)
            {
                var assignmentAnswers = this.ToAssignmentAnswers(file.FileName, rosterSizeQuestionColumns, preloadingRow);

                yield return new AssignmentRow
                {
                    Answers = assignmentAnswers.ToArray()
                };
            }
        }

        private IEnumerable<AssignmentValue> ToAssignmentAnswers(string fileName, string[] rosterInstanceColumns, PreloadingRow row)
        {
            foreach (var answer in row.Cells)
            {
                if (rosterInstanceColumns.Contains(answer.VariableOrCodeOrPropertyName)) continue;

                switch (answer)
                {
                    case PreloadingCompositeValue compositeCell:
                        yield return ToAssignmentAnswers(fileName, row, compositeCell);
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
                                yield return ToAssignmentAnswer(fileName, row, regularCell);
                                break;
                        }

                        break;
                }
            }

        }

        private static AssignmentAnswers ToAssignmentAnswers(string fileName, PreloadingRow row,
            PreloadingCompositeValue preloadingCompositeValue) => new AssignmentAnswers
            {
                InterviewId = row.InterviewId,
                VariableName = preloadingCompositeValue.VariableOrCodeOrPropertyName,
                Values = preloadingCompositeValue.Values.Select(x => ToAssignmentAnswer(fileName, row, x)).ToArray(),
            };

        private static AssignmentAnswer ToAssignmentAnswer(string fileName, PreloadingRow row,
            PreloadingValue answer) => new AssignmentAnswer
            {
                InterviewId = row.InterviewId,
                VariableName = answer.VariableOrCodeOrPropertyName,
                FileName = fileName,
                Column = answer.Column,
                Row = answer.Row,
                Value = answer.Value
            };

        private static AssignmentQuantity ToAssignmentQuantity(string fileName, string interviewId, PreloadingValue answer)
        {
            int.TryParse(answer.Value, out var quantity);

            return new AssignmentQuantity
            {
                InterviewId = interviewId,
                Quantity = quantity,
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

        //private InterviewQuestion ToQuestion(Guid questionId, IQuestionnaire questionnaire)
        //{
        //    var questionType = questionnaire.GetQuestionType(questionId);
        //    bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionId);
        //    bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionId);
        //    Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionId);
        //    Guid? sourceForLinkedQuestion = null;

        //    var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionId);
        //    var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionId);
        //    var isLinkedToListQuestion = questionnaire.IsLinkedToListQuestion(questionId);

        //    if (isLinkedToQuestion)
        //        sourceForLinkedQuestion = questionnaire.GetQuestionReferencedByLinkedQuestion(questionId);

        //    if (isLinkedToRoster)
        //        sourceForLinkedQuestion = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);

        //    return new InterviewQuestion
        //    {
        //        Id = questionId,
        //        IsRosterSize = questionnaire.IsRosterSizeQuestion(questionId),
        //        IsRosterSizeForLongRoster = questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId),
        //        Variable = questionnaire.GetQuestionVariableName(questionId).ToLower(),
        //        Type = GetQuestionType(questionType, cascadingParentQuestionId, isYesNoQuestion, isDecimalQuestion,
        //            isLinkedToListQuestion, sourceForLinkedQuestion)
        //    };
        //}
        //private static InterviewQuestionType GetQuestionType(
        //    QuestionType questionType,
        //    Guid? cascadingParentQuestionId,
        //    bool isYesNo,
        //    bool isDecimal,
        //    bool isLinkedToListQuestion,
        //    Guid? linkedSourceId = null)

        //{
        //    switch (questionType)
        //    {
        //        case QuestionType.SingleOption:
        //            {
        //                return linkedSourceId.HasValue
        //                    ? (isLinkedToListQuestion
        //                        ? InterviewQuestionType.SingleLinkedToList
        //                        : InterviewQuestionType.SingleLinkedOption)
        //                    : (cascadingParentQuestionId.HasValue
        //                        ? InterviewQuestionType.Cascading
        //                        : InterviewQuestionType.SingleFixedOption);
        //            }
        //        case QuestionType.MultyOption:
        //            {
        //                return isYesNo
        //                    ? InterviewQuestionType.YesNo
        //                    : (linkedSourceId.HasValue
        //                        ? (isLinkedToListQuestion
        //                            ? InterviewQuestionType.MultiLinkedToList
        //                            : InterviewQuestionType.MultiLinkedOption)
        //                        : InterviewQuestionType.MultiFixedOption);
        //            }
        //        case QuestionType.DateTime:
        //            return InterviewQuestionType.DateTime;
        //        case QuestionType.GpsCoordinates:
        //            return InterviewQuestionType.Gps;
        //        case QuestionType.Multimedia:
        //            return InterviewQuestionType.Multimedia;
        //        case QuestionType.Numeric:
        //            return isDecimal ? InterviewQuestionType.Double : InterviewQuestionType.Integer;
        //        case QuestionType.QRBarcode:
        //            return InterviewQuestionType.QRBarcode;
        //        case QuestionType.Area:
        //            return InterviewQuestionType.Area;
        //        case QuestionType.Text:
        //            return InterviewQuestionType.Text;
        //        case QuestionType.TextList:
        //            return InterviewQuestionType.TextList;
        //        case QuestionType.Audio:
        //            return InterviewQuestionType.Audio;
        //        default:
        //            throw new NotSupportedException($"Not supported question type: {questionType}");
        //    }
        //}

        private PreloadingRow ToRow(int rowIndex, ExpandoObject record)
        {
            var cells = new Dictionary<string, List<PreloadingValue>>();
            string interviewId = null;

            string GetVariable(string[] compositeColumnValues, string variableName) 
                => compositeColumnValues.Length > 1 && string.Format(ServiceColumns.IdSuffixFormat, variableName) != variableName
                ? compositeColumnValues[1]
                : variableName;

            foreach (var kv in record)
            {
                var variableName = kv.Key.ToLower();
                var value = (string) kv.Value;

                if (ignoredPreloadingColumns.Contains(variableName)) continue;
                if (variableName == ServiceColumns.InterviewId)
                {
                    interviewId = value;
                    continue;
                }

                var compositeColumnValues = kv.Key.Split(new[] { QuestionDataParser.ColumnDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                variableName = compositeColumnValues[0].ToLower();

                if (!cells.ContainsKey(variableName))
                    cells[variableName] = new List<PreloadingValue>();

                cells[variableName].Add(new PreloadingValue
                {
                    VariableOrCodeOrPropertyName = GetVariable(compositeColumnValues, variableName),
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
            FileName = fileName,
            QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName),
            Columns = this.csvReader.ReadHeader(inputStream, TabExportFile.Delimiter),
            Rows = this.csvReader.GetRecords(inputStream, TabExportFile.Delimiter)
                .Select((record, rowIndex) => (PreloadingRow)this.ToRow(rowIndex + 1, record)).ToArray()
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