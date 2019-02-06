using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                        columnsCollector.Add($" {alias}.\"{question.ColumnName}\" as \"{alias}_{question.ColumnName}\" ");
                    }

                    if (questionnaireEntity is Variable variable)
                    {
                        columnsCollector.Add($" {alias}.\"{variable.ColumnName}\" as \"{alias}_{variable.ColumnName}\" ");
                    }
                }

                return string.Join(", ", columnsCollector);
            }

            StringBuilder query = new StringBuilder($"select data.interview_id as data_interview_id, ");
            if (group.IsInsideRoster)
            {
                query.Append("data.roster_vector as data_roster_vector, ");
            }

            query.Append(BuildSelectColumns("data"));
            query.Append(", ");
            query.Append(BuildSelectColumns("enablement"));
            query.Append(", ");
            query.Append(BuildSelectColumns("validity"));

            query.AppendLine($" from ");
            query.AppendLine($"\"{tenant.Name}\".\"{group.TableName}\" data ");
                
            query.AppendFormat("    INNER JOIN \"{0}\".\"{1}\" enablement ON data.interview_id = enablement.interview_id{2}",
                tenant.Name, group.EnablementTableName, Environment.NewLine);
            query.AppendFormat("    INNER JOIN \"{0}\".\"{1}\" validity ON data.interview_id = validity.interview_id{2}",
                tenant.Name, group.ValidityTableName, Environment.NewLine);
            query.AppendFormat(" WHERE data.interview_id = ANY(@ids)");

            return query.ToString();
        }

    }
}