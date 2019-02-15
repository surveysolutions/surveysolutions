using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Interview
{
    public static class InterviewQueryBuilder
    {
        public static string GetInterviewsQuery(Group group)
        {
            StringBuilder query = new StringBuilder($"select ");// data.{InterviewDatabaseConstants.InterviewId} as data__interview_id ");
            if (group.DoesSupportDataTable)
            {
                query.AppendFormat("data.{0} as data__interview_id ", InterviewDatabaseConstants.InterviewId);
                if (group.IsInsideRoster)
                {
                    query.AppendFormat(", data.{0} as data__roster_vector ", InterviewDatabaseConstants.RosterVector);
                }

                if (group.HasAnyExportableQuestions)
                {
                    query.Append(", ");
                    query.Append(BuildSelectColumns("data", @group, inculdeStaticText: false));
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
                    query.AppendFormat(" enablement.{0} as data__interview_id, ", InterviewDatabaseConstants.InterviewId);
                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat(" enablement.{0} as data__roster_vector, ", InterviewDatabaseConstants.RosterVector);
                    }
                }

                query.AppendFormat(" enablement.{0} as enablement__{0}{1}", InterviewDatabaseConstants.InstanceValue,
                    Environment.NewLine);

                if (group.HasAnyExportableQuestions)
                {
                    query.Append(',');
                    query.Append(BuildSelectColumns("enablement", group));

                }
            }

            if (group.DoesSupportValidityTable)
            {
                if (group.DoesSupportDataTable || group.DoesSupportEnablementTable)
                {
                    query.Append(", ");
                }
                else
                {
                    query.AppendFormat(" validity.{0} as data__interview_id ", InterviewDatabaseConstants.InterviewId);
                    if (group.IsInsideRoster)
                    {
                        query.AppendFormat(", validity.{0} as data__roster_vector ", InterviewDatabaseConstants.RosterVector);
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

        private static string BuildSelectColumns(string alias, Group @group, bool includeVariables = true, bool inculdeStaticText = true)
        {
            List<string> columnsCollector = new List<string>();

            foreach (var questionnaireEntity in @group.Children)
            {
                //2cd54b4e-4de1-582e-c327-a006eeebf89a
                if (questionnaireEntity.PublicKey == Guid.Parse("2cd54b4e-4de1-582e-c327-a006eeebf89a"))
                {
                    int a = 5;
                }
                if (questionnaireEntity is Question question)
                {
                    columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}__{question.ColumnName}\" ");
                }

                if (includeVariables && questionnaireEntity is Variable variable)
                {
                    columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}__{variable.ColumnName}\" ");
                }

                if (inculdeStaticText && questionnaireEntity is StaticText staticText)
                {
                    columnsCollector.Add($" {alias}.\"{staticText.ColumnName}\" as \"{alias}__{staticText.ColumnName}\" ");
                }
            }

            return string.Join(", ", columnsCollector);
        }
    }
}
