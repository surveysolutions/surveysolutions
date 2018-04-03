using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public abstract class PreloadedInterviewBaseRow
    {
        public string[] Data { get; }
        public RosterVector RosterVector { get; set; }

        protected PreloadedInterviewBaseRow(string[] data)
        {
            this.Data = data;
            this.RosterVector = RosterVector.Empty;
        }

        public Tuple<string, string>[] GetAnswersWithColumnNames(PreloadedQuestionMeta questionMeta)
        {
            var answersWithColumnName = new List<Tuple<string, string>>();
            for (int i = 0; i < questionMeta.Indexes.Length; i++)
            {
                var columnIndex = questionMeta.Indexes[i];
                var columnHeader = questionMeta.Headers[i];

                if (columnIndex < 0)
                    continue;

                if (!string.IsNullOrEmpty(Data[columnIndex]))
                    answersWithColumnName.Add(new Tuple<string, string>(columnHeader, Data[columnIndex]));
            }

            return answersWithColumnName.Count == 0 ? null : answersWithColumnName.ToArray();
        }
    }

    public class PreloadedInterviewQuestionnaireRow : PreloadedInterviewBaseRow
    {
        public PreloadedInterviewQuestionnaireRow(string[] data, int responsibleNameIndex, int quantityIndex,
            string interviewId) : base(data)
        {
            InterviewId = interviewId;
            ResponsibleName = GetResponsibleNameIfSet(responsibleNameIndex, data);
            Quantity = GetQuantityForLevelIfSet(quantityIndex, data);
        }

        private static int? GetQuantityForLevelIfSet(int quantityIndex, string[] data)
        {
            const int defaultQuantityValue = 1;

            if (quantityIndex < 0)
                return defaultQuantityValue;

            var quantityString = data[quantityIndex].Trim();
            if (quantityString == "-1" || quantityString == "INF")
                return null;

            return int.TryParse(quantityString, out int quantity) ? quantity : defaultQuantityValue;
        }

        private static string GetResponsibleNameIfSet(int responsibleNameIndex, string[] data)
        {
            return responsibleNameIndex >= 0 ? data[responsibleNameIndex] : string.Empty;
        }

        public string ResponsibleName { get; }

        public int? Quantity { get; }
        public string InterviewId { get; set; }
    }

    public class PreloadedInterviewRow : PreloadedInterviewBaseRow
    {
        public PreloadedInterviewRow(string[] data, int[] rosterVectorIndexes, int rowcodeColumnIndex,
            int? rowTitleColumnIndex)
            : base(data)
        {
            if (int.TryParse(data[rowcodeColumnIndex], out int rowcode))
                Rowcode = rowcode;

            if (rowTitleColumnIndex.HasValue)
                RosterTitle = data[rowTitleColumnIndex.Value];

            RosterVector = new RosterVector(rosterVectorIndexes.Select(i => data[i]).Select(int.Parse));
        }

        public int Rowcode { get; }

        public string RosterTitle { get; }
    }

    public class PreloadedInterviewLevel : PreloadedInterviewBaseLevel
    {
        public Dictionary<string, List<PreloadedInterviewRow>> DataByInterviewId { get; private set; } =
            new Dictionary<string, List<PreloadedInterviewRow>>();

        public Guid RosterId { get; private set; }
        public string Name { get; private set; }

        public PreloadedInterviewLevel(
            PreloadedDataByFile preloadedDataByFile,
            Guid rosterId,
            ValueVector<Guid> rosterScope,
            QuestionnaireExportStructure exportStructure)
        {
            var header = preloadedDataByFile.Header.Select(x => x.ToLower(CultureInfo.InvariantCulture)).ToList();

            this.RosterId = rosterId;
            

            IdColumnIndex = header.IndexOf(ServiceColumns.InterviewId.ToLowerInvariant());

            var levelStructure = exportStructure.HeaderToLevelMap[rosterScope];

            this.Name = levelStructure.LevelName;

            int[] rosterVectorColumnIndexes =
                GetRosterVectorColumnIndexes(rosterScope, exportStructure, header, levelStructure).ToArray();

            var rowcodeColumnIndex = GetRowcodeColumnIndex(levelStructure, header);

            var rowTitleColumnIndex = RowTitleColumnIndex(levelStructure, header);

            Questions = GetQuestionsOnCurrentLevel(header, levelStructure).ToArray();

            foreach (var row in preloadedDataByFile.Content)
            {
                var rowWithoutMissingValues = GetRowWithoutMissingValues(row);

                var interviewId = IdColumnIndex >= 0
                    ? rowWithoutMissingValues[IdColumnIndex]
                    : Guid.NewGuid().ToString();

                if (!DataByInterviewId.ContainsKey(interviewId))
                    DataByInterviewId.Add(interviewId, new List<PreloadedInterviewRow>());

                DataByInterviewId[interviewId].Add(new PreloadedInterviewRow(rowWithoutMissingValues,
                    rosterVectorColumnIndexes, rowcodeColumnIndex, rowTitleColumnIndex));
            }
        }

        protected int? RowTitleColumnIndex(HeaderStructureForLevel levelStructure, List<string> header)
        {
            var rowTitleColumnName = levelStructure.ReferencedNames?.FirstOrDefault()?.ToLowerInvariant();
            int? rowTitleColumnIndex = null;
            if (!string.IsNullOrWhiteSpace(rowTitleColumnName))
            {
                rowTitleColumnIndex = header.IndexOf(rowTitleColumnName);
            }

            return rowTitleColumnIndex;
        }

        protected int GetRowcodeColumnIndex(HeaderStructureForLevel levelStructure, List<string> header)
        {
            return header.IndexOf(levelStructure.LevelIdColumnName.ToLowerInvariant());
        }

        protected IEnumerable<int> GetRosterVectorColumnIndexes(ValueVector<Guid> rosterScope,
            QuestionnaireExportStructure exportStructure, List<string> header, HeaderStructureForLevel levelStructure)
        {
            var parentColumnsNames = exportStructure.GetAllParentColumnNamesForLevel(rosterScope)
                .Union(new[] {levelStructure.LevelIdColumnName})
                .Except(new[] {ServiceColumns.InterviewId, ServiceColumns.Key})
                .Select(x => x.ToLowerInvariant())
                .ToArray();

            return parentColumnsNames.Select(parentColumnName => header.IndexOf(parentColumnName));
        }

        public override bool HasDataForInterview(string interviewId)
        {
            return DataByInterviewId.ContainsKey(interviewId);
        }

        public override IEnumerable<PreloadedInterviewBaseRow> GetInterviewRows(string interviewId)
        {
            return DataByInterviewId[interviewId];
        }

        public TextListAnswerRow[] GetRowCodeAndTitlesPairs(string interviewId, RosterVector rosterVector)
        {
            if (!DataByInterviewId.ContainsKey(interviewId))
                return null;
            var rosterRows = DataByInterviewId[interviewId];

            return rosterRows
                .Where(x => x.RosterVector.Take(rosterVector.Length) == rosterVector)
                .Select(x => new TextListAnswerRow(x.Rowcode, x.RosterTitle)).ToArray();
        }
    }

    public abstract class PreloadedInterviewBaseLevel
    {
        public int IdColumnIndex { get; protected set; }

        public PreloadedQuestionMeta[] Questions { get; protected set; }

        private static readonly HashSet<QuestionType> QuestionTypesToSkip =
            new HashSet<QuestionType> {QuestionType.Area, QuestionType.Audio, QuestionType.Multimedia};

        private static readonly HashSet<QuestionSubtype> QuestionSubTypesToSkip =
            new HashSet<QuestionSubtype> {QuestionSubtype.SingleOption_Linked, QuestionSubtype.MultyOption_Linked};

        protected static IEnumerable<PreloadedQuestionMeta> GetQuestionsOnCurrentLevel(List<string> header,
            HeaderStructureForLevel exportStructureHeaderToLevel)
        {
            foreach (IExportedHeaderItem headerItem in exportStructureHeaderToLevel.HeaderItems.Values)
            {
                ExportedQuestionHeaderItem questionHeaderItem = headerItem as ExportedQuestionHeaderItem;

                if (questionHeaderItem == null)
                    continue;

                if (AnswerShouldBeSkipped(questionHeaderItem.QuestionType, questionHeaderItem.QuestionSubType))
                    continue;

                var indexes = new List<int>();

                foreach (var column in headerItem.ColumnHeaders)
                {
                    indexes.Add(header.IndexOf(column.Name.ToLowerInvariant()));
                }

                yield return new PreloadedQuestionMeta(headerItem.VariableName, indexes.ToArray(),
                    headerItem.ColumnHeaders.Select(x => x.Name).ToArray());
            }
        }

        protected static bool AnswerShouldBeSkipped(QuestionType questionType, QuestionSubtype? questionSubType)
        {
            if (QuestionTypesToSkip.Contains(questionType))
                return true;

            if (questionSubType.HasValue && QuestionSubTypesToSkip.Contains(questionSubType.Value))
                return true;

            return false;
        }

        protected static string[] GetRowWithoutMissingValues(string[] row)
        {
            return row.Select(v =>
                    v?.Replace(ExportFormatSettings.MissingNumericQuestionValue, string.Empty)
                        .Replace(ExportFormatSettings.MissingStringQuestionValue, string.Empty))
                .ToArray();
        }

        public abstract bool HasDataForInterview(string interviewId);

        public abstract IEnumerable<PreloadedInterviewBaseRow> GetInterviewRows(string interviewId);
    }

    public class PreloadedInterviewQuestionnaireLevel : PreloadedInterviewBaseLevel
    {
        public Dictionary<string, PreloadedInterviewQuestionnaireRow> DataByInterviewId { get; private set; } =
            new Dictionary<string, PreloadedInterviewQuestionnaireRow>();

        public IEnumerable<PreloadedInterviewQuestionnaireRow> Interviews => DataByInterviewId.Values;

        public PreloadedInterviewQuestionnaireLevel(PreloadedDataByFile preloadedDataByFile,
            QuestionnaireExportStructure exportStructure)
        {
            var header = preloadedDataByFile.Header.Select(x => x.ToLower(CultureInfo.InvariantCulture)).ToList();

            IdColumnIndex = header.IndexOf(ServiceColumns.InterviewId.ToLowerInvariant());

            var levelStructure = exportStructure.HeaderToLevelMap[ValueVector.Create(new Guid[0])];

            Questions = GetQuestionsOnCurrentLevel(header, levelStructure).ToArray();

            var responsibleNameIndex = header.IndexOf(ServiceColumns.ResponsibleColumnName.ToLowerInvariant());
            var quantityIndex = header.IndexOf(ServiceColumns.AssignmentsCountColumnName.ToLowerInvariant());

            foreach (var row in preloadedDataByFile.Content)
            {
                var rowWithoutMissingValues = GetRowWithoutMissingValues(row);

                var interviewId = IdColumnIndex >= 0
                    ? rowWithoutMissingValues[IdColumnIndex]
                    : Guid.NewGuid().ToString();

                if (!DataByInterviewId.ContainsKey(interviewId))
                    DataByInterviewId.Add(interviewId,
                        new PreloadedInterviewQuestionnaireRow(rowWithoutMissingValues, responsibleNameIndex,
                            quantityIndex, interviewId));
            }
        }

        public override bool HasDataForInterview(string interviewId)
        {
            return true;
        }

        public override IEnumerable<PreloadedInterviewBaseRow> GetInterviewRows(string interviewId)
        {
            return DataByInterviewId[interviewId].ToEnumerable();
        }
    }
}
