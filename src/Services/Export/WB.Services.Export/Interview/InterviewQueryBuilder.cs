using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Interview
{
    public static class InterviewQueryBuilder
    {
        public static string GetInterviewsQuery(TenantInfo tenant, Group group)
        {
            if (!group.Children.Any()) throw new ArgumentException("Cannot build query for group without questions");

            string BuildSelectColumns(string alias)
            {
                List<string> columnsCollector = new List<string>();

                foreach (var questionnaireEntity in group.Children)
                {
                    if (questionnaireEntity is Question question)
                    {
                        columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}__{question.ColumnName}\" ");
                    }

                    if (questionnaireEntity is Variable variable)
                    {
                        columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}__{variable.ColumnName}\" ");
                    }
                }

                return string.Join(", ", columnsCollector);
            }

            StringBuilder query = new StringBuilder($"select data.{InterviewDatabaseConstants.InterviewId} as data__interview_id, ");
            if (group.IsInsideRoster)
            {
                query.AppendFormat("data.{0} as data__roster_vector, ", InterviewDatabaseConstants.RosterVector);
            }

            query.Append(BuildSelectColumns("data"));
            query.Append(", ");
            query.Append(BuildSelectColumns("enablement"));
            query.Append(", ");
            query.Append(BuildSelectColumns("validity"));

            query.AppendLine($" from ");
            query.AppendLine($"\"{tenant.Name}\".\"{group.TableName}\" data ");
                
            query.AppendFormat("    INNER JOIN \"{0}\".\"{1}\" enablement ON data.{2} = enablement.{2}{3}",
                tenant.Name, group.EnablementTableName, InterviewDatabaseConstants.InterviewId, Environment.NewLine);
            if (group.IsInsideRoster)
            {
                query.AppendFormat("    AND data.{0} = enablement.{0}{1}", InterviewDatabaseConstants.RosterVector, Environment.NewLine);
            }

            query.AppendFormat("    INNER JOIN \"{0}\".\"{1}\" validity ON data.{2} = validity.{2}{3}",
                tenant.Name, group.ValidityTableName, InterviewDatabaseConstants.InterviewId, Environment.NewLine);

            if (group.IsInsideRoster)
            {
                query.AppendFormat("   AND data.{0} = validity.{0} {1}", InterviewDatabaseConstants.RosterVector, Environment.NewLine);
            }

            query.AppendFormat("WHERE data.{0} = ANY(@ids)", InterviewDatabaseConstants.InterviewId);

            return query.ToString();
        }

    }
}
