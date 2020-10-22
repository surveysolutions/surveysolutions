using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data
{
    public class GetCategoricalReportParams
    {
        public string QuestionnaireIdentity => $"{QuestionnaireId}${Version?.ToString() ?? "%"}";
        public string QuestionnaireId { get; set; }
        public long? Version { get; set; }
        public bool Detailed { get; set; }
        public Guid Variable { get; set; }
        public bool Totals { get;  set; }
        public Guid? TeamLeadId { get; set; }
        public Guid? ConditionVariable { get; set; }
        public long[] Condition { get; set; }
        public InterviewStatus[] Statuses { get; set; }

        public GetCategoricalReportParams(string questionnaireId, long? version,
            bool detailed, Guid variable, Guid? teamLeadId = null,
            Guid? conditionVariable = null, long[] condition = null)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Detailed = detailed;
            this.Variable = variable;
            this.Totals = false;
            this.TeamLeadId = teamLeadId;
            this.ConditionVariable = conditionVariable;
            this.Condition = condition;;
        }

        public GetCategoricalReportParams AsTotals()
        {
            return new GetCategoricalReportParams(this.QuestionnaireId, this.Version, Detailed, Variable, TeamLeadId,
                ConditionVariable, Condition)
            {
                Detailed = false,
                Totals = true
            };
        }
    }
}
