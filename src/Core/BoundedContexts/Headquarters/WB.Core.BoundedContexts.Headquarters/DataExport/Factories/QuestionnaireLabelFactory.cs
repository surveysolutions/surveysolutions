using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class QuestionnaireLabelFactory : IQuestionnaireLabelFactory
    {
        private QuestionnaireLevelLabels CreateLabelsForQuestionnaireLevel(
            QuestionnaireExportStructure structure,
            ValueVector<Guid> levelRosterVector)
        {
            var level = structure.HeaderToLevelMap[levelRosterVector];

            var variableLabels = new List<LabeledVariable>();

            var levelVariableValueLabel = new VariableValueLabel[0];
            if (level.LevelLabels != null)
            {
                levelVariableValueLabel= level.LevelLabels.Select(x => new VariableValueLabel(x.Caption, x.Title)).ToArray();
            }

            variableLabels.Add(new LabeledVariable(level.LevelIdColumnName, string.Empty, null, levelVariableValueLabel));

            foreach (ExportedHeaderItem headerItem in level.HeaderItems.Values)
            {
                bool hasLabels = headerItem.Labels != null && headerItem.Labels.Count > 0;

                for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                {
                    var variableValueLabel = new VariableValueLabel[0];

                    if (hasLabels)
                    {
                        variableValueLabel = headerItem.Labels.Values.Select(label => new VariableValueLabel(label.Caption, label.Title ?? string.Empty)).ToArray();
                    }

                    variableLabels.Add(new LabeledVariable(headerItem.ColumnNames[i],
                        headerItem.Titles[i] ?? string.Empty, headerItem.PublicKey, variableValueLabel));
                }
            }

            if (level.IsTextListScope)
            {
                variableLabels.AddRange(
                    level.ReferencedNames.Select(
                        name => new LabeledVariable(name, string.Empty)));
            }

            for (int i = 0; i < levelRosterVector.Length; i++)
            {
                string parentColumnLabel;
                if (i == 0)
                {
                    parentColumnLabel = "InterviewId";
                }
                else
                {
                    var parentRosterVector = new ValueVector<Guid>(levelRosterVector.Take(i));

                    if (!structure.HeaderToLevelMap.ContainsKey(parentRosterVector))
                        continue;
                    parentColumnLabel = $"Id in \"{structure.HeaderToLevelMap[parentRosterVector].LevelName}\"";
                }
                variableLabels.Add(new LabeledVariable($"{ServiceColumns.ParentId}{levelRosterVector.Length - i}", parentColumnLabel));
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