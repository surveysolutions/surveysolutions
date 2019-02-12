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
            string BuildSelectColumns(string alias, bool includeVariables = true)
            {
                List<string> columnsCollector = new List<string>();

                foreach (var questionnaireEntity in group.Children)
                {
                    if (questionnaireEntity is Question question)
                    {
                        columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}__{question.ColumnName}\" ");
                    }

                    if (includeVariables && questionnaireEntity is Variable variable)
                    {
                        columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}__{variable.ColumnName}\" ");
                    }
                }

                return string.Join(", ", columnsCollector);
            }

            StringBuilder query = new StringBuilder($"select data.{InterviewDatabaseConstants.InterviewId} as data__interview_id ");
            if (group.IsInsideRoster)
            {
                query.AppendFormat(", data.{0} as data__roster_vector ", InterviewDatabaseConstants.RosterVector);
            }

            query.AppendFormat(", enablement.{0} as enablement__{0}{1}", InterviewDatabaseConstants.InstanceValue, Environment.NewLine);
            if (group.HasAnyExportableQuestions)
            {
                query.Append(",");
            }

            query.Append(BuildSelectColumns("enablement"));

            if (group.HasAnyExportableQuestions)
            {
                query.Append(", ");
                query.Append(BuildSelectColumns("data"));
            }

            if (group.HasAnyExportableQuestions)
            {
                query.Append(", ");
                query.Append(BuildSelectColumns("validity", false));
            }

            query.AppendLine($" from ");
            query.AppendLine($"\"{group.TableName}\" data ");
                
            query.Append($"    INNER JOIN \"{@group.EnablementTableName}\" enablement ON data.{InterviewDatabaseConstants.InterviewId} = enablement.{InterviewDatabaseConstants.InterviewId}{Environment.NewLine}");
            if (group.IsInsideRoster)
            {
                query.AppendFormat("    AND data.{0} = enablement.{0}{1}", InterviewDatabaseConstants.RosterVector, Environment.NewLine);
            }

            if (group.HasAnyExportableQuestions)
            {
                query.AppendFormat("    INNER JOIN \"{0}\" validity ON data.{1} = validity.{1}{2}",
                    group.ValidityTableName, InterviewDatabaseConstants.InterviewId, Environment.NewLine);

                if (group.IsInsideRoster)
                {
                    query.AppendFormat("   AND data.{0} = validity.{0} {1}", InterviewDatabaseConstants.RosterVector,
                        Environment.NewLine);
                }
            }

            query.AppendFormat("WHERE data.{0} = ANY(@ids){1}", InterviewDatabaseConstants.InterviewId, Environment.NewLine);
            if (group.IsInsideRoster)
            {
                query.AppendFormat("ORDER BY data.{0}", InterviewDatabaseConstants.RosterVector);
            }

            return query.ToString();
        }

    }
}
