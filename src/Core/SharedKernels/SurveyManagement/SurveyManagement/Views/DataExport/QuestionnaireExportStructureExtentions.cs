using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public static class QuestionnaireExportStructureExtentions
    {
        public static void CollectLabels(this QuestionnaireExportStructure structure, out Dictionary<string, Dictionary<string, string>> labelsForServiceColumns, out Dictionary<string, string> labels, out Dictionary<string, Dictionary<double, string>> varValueLabels)
        {
            labels = new Dictionary<string, string>();
            varValueLabels = new Dictionary<string, Dictionary<double, string>>();
            labelsForServiceColumns = new Dictionary<string, Dictionary<string, string>>();

            foreach (var headerStructureForLevel in structure.HeaderToLevelMap.Values)
            {
                foreach (ExportedHeaderItem headerItem in headerStructureForLevel.HeaderItems.Values)
                {
                    bool hasLabels = headerItem.Labels != null && headerItem.Labels.Count > 0;

                    if (hasLabels)
                    {
                        string labelName = headerItem.VariableName;
                        if (!varValueLabels.ContainsKey(labelName))
                        {
                            var items = headerItem.Labels.Values.ToDictionary(item => Double.Parse(item.Caption), item => item.Title ?? string.Empty);
                            varValueLabels.Add(labelName, items);
                        }
                    }

                    for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                    {
                        if (!labels.ContainsKey(headerItem.ColumnNames[i]))
                            labels.Add(headerItem.ColumnNames[i], headerItem.Titles[i] ?? string.Empty);
                    }
                }
                var labelsForServiceColumnsForTheLevel = new Dictionary<string, string>();
                for (int i = 0; i < headerStructureForLevel.LevelScopeVector.Length; i++)
                {
                    string parentColumnLabel;
                    if (i == 0)
                    {
                        parentColumnLabel = "InterviewId";
                    }
                    else
                    {
                        parentColumnLabel = $"Id in \"{structure.HeaderToLevelMap[new ValueVector<Guid>(headerStructureForLevel.LevelScopeVector.Take(i))].LevelName}\"";
                    }

                    labelsForServiceColumnsForTheLevel.Add($"{ServiceColumns.ParentId}{headerStructureForLevel.LevelScopeVector.Length - i}",
                        parentColumnLabel);
                }
                labelsForServiceColumns.Add(headerStructureForLevel.LevelName, labelsForServiceColumnsForTheLevel);
                if (headerStructureForLevel.LevelLabels == null) continue;

                var levelLabelName = headerStructureForLevel.LevelIdColumnName;
                if (varValueLabels.ContainsKey(levelLabelName)) continue;

                var labelItems = headerStructureForLevel.LevelLabels.ToDictionary(item => Double.Parse(item.Caption), item => item.Title ?? String.Empty);
                varValueLabels.Add(levelLabelName, labelItems);
            }
        }
    }
}
