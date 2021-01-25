using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

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
            var predefinedLabels = level.ReusableLabels
                .Select(o => new DataExportValue(
                    o.Value.Name, 
                    o.Key, 
                    o.Value.Labels.Select(label => new VariableValueLabel(label.Value, label.Title.RemoveHtmlTags() ?? string.Empty)).ToArray()))
                .ToArray();

            var levelVariableValueLabel = Array.Empty<VariableValueLabel>();
            if (level.LevelLabels != null)
            {
                levelVariableValueLabel = level.LevelLabels.Select(x => new VariableValueLabel(x.Value, x.Title.RemoveHtmlTags())).ToArray();
            }

            if (levelRosterVector.Count == 0 && level.LevelIdColumnName == ServiceColumns.InterviewId) // main file
            {
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewId, "Unique 32-character long identifier of the interview", null, levelVariableValueLabel, ExportValueType.String));
                variableLabels.Add(new DataExportVariable(ServiceColumns.Key, "Interview key (identifier in XX-XX-XX-XX format)", null, levelVariableValueLabel, ExportValueType.String));
                variableLabels.Add(new DataExportVariable(ServiceColumns.AssignmentId, "Assignment id (identifier in numeric format)", null, levelVariableValueLabel, ExportValueType.NumericInt));
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewRandom, "Random number in the range 0..1 associated with interview", null, levelVariableValueLabel, ExportValueType.Numeric));
                variableLabels.Add(new DataExportVariable(ServiceColumns.HasAnyError, "Errors count in the interview", null, levelVariableValueLabel, ExportValueType.NumericInt));
                variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewStatus, "Status of the interview", null,
                    Enum.GetValues(typeof(InterviewStatus))
                        .Cast<InterviewStatus>().
                        Select(x => new VariableValueLabel(((int)x).ToString(), x.ToString())).ToArray(), 
                    ExportValueType.NumericInt));
            }
            else
            {
                variableLabels.Add(new DataExportVariable(level.LevelIdColumnName, $"Id in {level.LevelName}", null, levelVariableValueLabel, ExportValueType.NumericInt));

                variableLabels.Add(new DataExportVariable(ServiceColumns.Key, "Interview key (identifier in XX-XX-XX-XX format)", null, Array.Empty<VariableValueLabel>(), ExportValueType.String));
            }

            foreach (IExportedHeaderItem headerItem in level.HeaderItems.Values)
            {
                var isMultiOptionQuestionWithOptionsInEntity = (headerItem is ExportedQuestionHeaderItem questionHeaderItem) 
                                            && questionHeaderItem.QuestionType == QuestionType.MultyOption 
                                            && questionHeaderItem.QuestionSubType != QuestionSubtype.MultyOption_Combobox;
                var isNeedSaveLabelsByEntity = (headerItem as ExportedQuestionHeaderItem)?.Labels?.Count > 0 && !isMultiOptionQuestionWithOptionsInEntity;
                var labelReferenceId = (headerItem as ExportedQuestionHeaderItem)?.LabelReferenceId;

                foreach (var headerColumn in headerItem.ColumnHeaders)
                {
                    DataExportValue? value = null;

                    if (labelReferenceId != null)
                    {
                        var labels = level.ReusableLabels.First(l => l.Key == labelReferenceId.Value).Value;
                        value = new DataExportValue(labels.Name, labelReferenceId.Value);
                    }
                    else if (isNeedSaveLabelsByEntity)
                    {
                        var variableValueLabel = ((ExportedQuestionHeaderItem)headerItem).Labels
                            .Select(label => new VariableValueLabel(label.Value, label.Title?.RemoveHtmlTags() ?? string.Empty))
                            .ToArray();
                        value = new DataExportValue(headerColumn.Name, headerItem.PublicKey, variableValueLabel);
                    }
                    else
                    {
                        value = new DataExportValue(headerColumn.Name, headerItem.PublicKey, Array.Empty<VariableValueLabel>());
                    }

                    var variableLabel = headerColumn.Title?.RemoveHtmlTags() ?? string.Empty;
                    variableLabels.Add(new DataExportVariable(headerColumn.Name, variableLabel, headerItem.PublicKey, headerColumn.ExportType, value));
                }
            }

            if (level.IsTextListScope)
            {
                variableLabels.AddRange(
                    level.ReferencedNames.Select(
                        name => new DataExportVariable(name, "Roster list question", null, Array.Empty<VariableValueLabel>(), ExportValueType.String)));
            }

            for (int i = 0; i < levelRosterVector.Length; i++)
            {
                if (i == 0)
                {
                    variableLabels.Add(new DataExportVariable(ServiceColumns.InterviewId, "Unique 32-character long identifier of the interview", null, Array.Empty<VariableValueLabel>(), ExportValueType.String));
                }
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

            return new QuestionnaireLevelLabels(level.LevelName, variableLabels.ToArray(), predefinedLabels);
        }

        public QuestionnaireLevelLabels[] CreateLabelsForQuestionnaire(QuestionnaireExportStructure structure)
        {
            return structure.HeaderToLevelMap.Values.Select(x => 
                this.CreateLabelsForQuestionnaireLevel(structure, x.LevelScopeVector)).ToArray();
        }
    }
}
