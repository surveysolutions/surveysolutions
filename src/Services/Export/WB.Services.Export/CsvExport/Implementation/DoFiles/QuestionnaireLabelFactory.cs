using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Utils;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    internal class QuestionnaireLabelFactory : IQuestionnaireLabelFactory
    {
        private QuestionnaireLevelLabels CreateLabelsForQuestionnaireLevel(
            QuestionnaireExportStructure structure,
            ValueVector<Guid> levelRosterVector)
        {
            var level = structure.HeaderToLevelMap[levelRosterVector];

            var variableLabels = new List<DataExportVariable>();

            var levelVariableValueLabel = Array.Empty<VariableValueLabel>();
            if (level.LevelLabels != null)
            {
                levelVariableValueLabel = level.LevelLabels.Select(x => new VariableValueLabel(x.Caption, x.Title?.RemoveHtmlTags())).ToArray();
            }

            if (levelRosterVector.Count == 0 && level.LevelIdColumnName == ServiceColumns.InterviewId) // main file
            {
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewId, "Unique 32-character long identifier of the interview", null, levelVariableValueLabel, ExportValueType.Unknown));
                variableLabels.Add(new DataExportVariable(ServiceColumns.Key, "Unique 8-digit long identifier of the interview", null, levelVariableValueLabel, ExportValueType.Unknown));
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewRandom, "Random number in the range 0..1 associated with interview", null, levelVariableValueLabel, ExportValueType.Unknown));
                variableLabels.Add(new DataExportVariable(ServiceColumns.HasAnyError, "Errors count in the interview", null, levelVariableValueLabel, ExportValueType.Unknown));
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewStatus, "Status of the interview", null, levelVariableValueLabel, ExportValueType.Unknown));
            }
            else
            {
                variableLabels.Add(new DataExportVariable(level.LevelIdColumnName, string.Empty, null, levelVariableValueLabel, ExportValueType.Unknown));
                variableLabels.Add(new DataExportVariable(ServiceColumns.Key, "Unique 8-digit long identifier of the interview", null, levelVariableValueLabel, ExportValueType.Unknown));
            }

            foreach (IExportedHeaderItem headerItem in level.HeaderItems.Values)
            {
                bool hasLabels = (headerItem as ExportedQuestionHeaderItem)?.Labels?.Count > 0 && ((ExportedQuestionHeaderItem)headerItem).QuestionType != QuestionType.MultyOption;

                foreach (var headerColumn in headerItem.ColumnHeaders)
                {
                    var variableValueLabel = Array.Empty<VariableValueLabel>();

                    if (hasLabels)
                    {
                        variableValueLabel = ((ExportedQuestionHeaderItem)headerItem).Labels.Select(label => new VariableValueLabel(label.Caption, label.Title?.RemoveHtmlTags() ?? string.Empty)).ToArray();
                    }

                    variableLabels.Add(new DataExportVariable(headerColumn.Name, headerColumn.Title?.RemoveHtmlTags() ?? string.Empty, headerItem.PublicKey, variableValueLabel, headerColumn.ExportType));
                }
            }

            if (level.IsTextListScope)
            {
                variableLabels.AddRange(
                    level.ReferencedNames.Select(
                        name => new DataExportVariable(name, string.Empty)));
            }

            for (int i = 0; i < levelRosterVector.Length; i++)
            {
                if (i == 0)
                    variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewId, "InterviewId"));
                else
                {
                    var parentRosterVector = new ValueVector<Guid>(levelRosterVector.Take(i));

                    if (!structure.HeaderToLevelMap.ContainsKey(parentRosterVector))
                        continue;

                    var parentRosterName = structure.HeaderToLevelMap[parentRosterVector].LevelName;

                    var parentColumnLabel = $"Id in \"{parentRosterName}\"";
                    variableLabels.Add(new DataExportVariable(string.Format(ServiceColumns.IdSuffixFormat, parentRosterName), parentColumnLabel));
                }
            }

            return new QuestionnaireLevelLabels(level.LevelName, variableLabels.ToArray());
        }

        public QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure)
        {
            return structure.HeaderToLevelMap.Values.Select(
                        x => this.CreateLabelsForQuestionnaireLevel(structure, x.LevelScopeVector)).ToArray();
        }
    }
}
