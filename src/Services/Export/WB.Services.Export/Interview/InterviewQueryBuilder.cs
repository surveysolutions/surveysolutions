using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Interview
{
    public static class InterviewQueryBuilder
    {
        public static string GetEnabledQuestionAnswersQuery(Question question)
        {
            var parentGroup = question.GetParent() as Group;
            if(parentGroup == null)
                throw new InvalidOperationException("Parent was not found.");

            StringBuilder result = new StringBuilder("select ");
            result.AppendFormat("data.\"{1}\" as data__{1}, data.\"{0}\" as \"data__{0}\"",
                question.ColumnName, InterviewDatabaseConstants.InterviewId);

            if (parentGroup.IsInsideRoster)
            {
                result.AppendFormat(", data.{0} as data__{0}", InterviewDatabaseConstants.RosterVector);
            }

            result.AppendFormat("{1} from \"{0}\" data {1}", parentGroup.TableName, Environment.NewLine);
            result.AppendFormat("inner join \"{0}\" enablement {1} ", parentGroup.EnablementTableName, Environment.NewLine);
            result.AppendFormat("on data.{0} = enablement.{0}", InterviewDatabaseConstants.InterviewId);
            if (parentGroup.IsInsideRoster)
            {
                result.AppendFormat(" AND data.{0} = enablement.{0}{1}",
                    InterviewDatabaseConstants.RosterVector, Environment.NewLine);
            }

            result.AppendFormat("{2} where data.{1} = any(@ids) {2}    and enablement.\"{0}\" = TRUE {2}     and data.\"{0}\" is not null",
                question.ColumnName, InterviewDatabaseConstants.InterviewId, Environment.NewLine);
            return result.ToString();
        }

        public static string GetInterviewsQuery(Group group)
        {
            StringBuilder query = new StringBuilder($"select ");// data.{InterviewDatabaseConstants.InterviewId} as data__interview_id ");
            if (group.DoesSupportDataTable)
            {
                query.AppendFormat("data.{0} as data__{0} ", InterviewDatabaseConstants.InterviewId);
                if (group.IsInsideRoster)
                {
                    query.AppendFormat(", data.{0} as data__{0} ", InterviewDatabaseConstants.RosterVector);
                }

                if (group.HasAnyExportableChild)
                {
                    var columns = BuildSelectColumns("data", @group, includeStaticText: false);
                    if (!string.IsNullOrWhiteSpace(columns))
                    {
                        query.Append(", ");
                        query.Append(columns);
                    }
                }
            }

            if (group.DoesSupportEnablementTable)
            {
                if (group.DoesSupportDataTable)
                {
                    query.Append(", ");
                }
                else
                {
                    query.AppendFormat(" enablement.{0} as data__{0}, ", InterviewDatabaseConstants.InterviewId);
                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat(" enablement.{0} as data__{0}, ", InterviewDatabaseConstants.RosterVector);
                    }
                }

                query.AppendFormat(" enablement.\"{0}\" as \"enablement__{0}\"{1}", group.ColumnName,
                    Environment.NewLine);


                var enabledColumnsSelect = BuildSelectColumns("enablement", @group);
                if (!string.IsNullOrWhiteSpace(enabledColumnsSelect))
                {
                    query.Append(", ");
                }
                query.Append(enabledColumnsSelect);
            }

            if (group.DoesSupportValidityTable)
            {
                if (group.DoesSupportDataTable || group.DoesSupportEnablementTable)
                {
                    query.Append(", ");
                }
                else
                {
                    query.AppendFormat(" validity.{0} as data__{0} ", InterviewDatabaseConstants.InterviewId);
                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat(", validity.{0} as data__{0} ", InterviewDatabaseConstants.RosterVector);
                    }
                }

                query.Append(BuildSelectColumns("validity", group, false));
            }

            query.AppendLine($" from ");
            string wherePrefix = "";

            if (group.DoesSupportDataTable)
            {
                wherePrefix = "data";
                query.Append($"\"{group.TableName}\" data {Environment.NewLine}");
            }

            if (group.DoesSupportEnablementTable)
            {
                wherePrefix = "enablement";

                if (group.DoesSupportDataTable)
                {
                    query.Append(" INNER JOIN ");
                }

                query.Append($"    \"{group.EnablementTableName}\" enablement ");
                if (group.DoesSupportDataTable)
                {
                    query.AppendFormat("ON data.{0} = enablement.{0}{1}",
                        InterviewDatabaseConstants.InterviewId, Environment.NewLine);

                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat("    AND data.{0} = enablement.{0}{1}",
                            InterviewDatabaseConstants.RosterVector, Environment.NewLine);
                    }
                }
            }

            if (group.DoesSupportValidityTable)
            {
                wherePrefix = "validity";
                if (group.DoesSupportDataTable || group.DoesSupportEnablementTable)
                {
                    query.Append(" INNER JOIN ");

                    var joinToTable = @group.DoesSupportDataTable ? "data" : "enablement";
                    query.Append($"    \"{group.ValidityTableName}\" validity ON {joinToTable}.{InterviewDatabaseConstants.InterviewId} = validity.{InterviewDatabaseConstants.InterviewId}{Environment.NewLine}");

                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat("   AND {0}.{1} = validity.{1} {2}",
                            joinToTable,
                            InterviewDatabaseConstants.RosterVector,
                            Environment.NewLine);
                    }
                }
            }

            query.AppendFormat("WHERE {0}.{1} = ANY(@ids){2}",
                wherePrefix,
                InterviewDatabaseConstants.InterviewId,
                Environment.NewLine);
            if (group.IsInsideRoster)
            {
                query.AppendFormat("ORDER BY {0}.{1}", wherePrefix, InterviewDatabaseConstants.RosterVector);
            }

            return query.ToString();
        }

        private static string BuildSelectColumns(string alias, Group @group, bool includeVariables = true, bool includeStaticText = true)
        {
            List<string> columnsCollector = new List<string>();

            foreach (var questionnaireEntity in @group.Children.Where(c => c.IsExportable))
            {
                if (questionnaireEntity is Question question)
                {
                    columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}__{question.ColumnName}\" ");
                }

                if (includeVariables && questionnaireEntity is Variable variable)
                {
                    columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}__{variable.ColumnName}\" ");
                }

                if (includeStaticText && questionnaireEntity is StaticText staticText)
                {
                    columnsCollector.Add($" {alias}.\"{staticText.ColumnName}\" as \"{alias}__{staticText.ColumnName}\" ");
                }
            }

            return string.Join(", ", columnsCollector);
        }
    }
}
