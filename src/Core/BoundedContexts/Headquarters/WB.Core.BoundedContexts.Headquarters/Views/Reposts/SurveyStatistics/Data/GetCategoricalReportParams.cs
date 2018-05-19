using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data
{
    public class GetCategoricalReportParams
    {
        public string QuestionnaireIdentity { get; set; }
        public bool Detailed { get; set; }
        public Guid Variable { get; set; }
        public bool Totals { get;  set; }
        public Guid? TeamLeadId { get; set; }
        public Guid? ConditionVariable { get; set; }
        public int[] Condition { get; set; }
        public int[] Exclude { get; set; }

        public GetCategoricalReportParams(string questionnaireId,
            bool detailed, Guid variable, Guid? teamLeadId = null,
            Guid? conditionVariable = null, int[] condition = null, int[] exclude = null)
        {
            this.QuestionnaireIdentity = questionnaireId;
            this.Detailed = detailed;
            this.Variable = variable;
            this.Totals = false;
            this.TeamLeadId = teamLeadId;
            this.ConditionVariable = conditionVariable;
            this.Condition = condition;
            this.Exclude = exclude;
        }

        public GetCategoricalReportParams AsTotals()
        {
            return new GetCategoricalReportParams(this.QuestionnaireIdentity, Detailed, Variable, TeamLeadId,
                ConditionVariable, Condition, Exclude)
            {
                Detailed = false,
                Totals = true
            };
        }
    }
}
