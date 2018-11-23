using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.API.PublicApi.Models.Statistics
{
    public class SurveyStatisticsQuery : DataTableRequest
    {
        /// <summary>
        /// Questionnaire Identity
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Question variable name or UUID
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Minimum value of answers to count in report
        /// </summary>
        /// <remarks>
        /// Only applicable for questions with <c>Type</c> equal to <c>Numeric</c>
        /// </remarks>
        public int? Min { get; set; }

        /// <summary>
        /// Maximum value of answers to count in report
        /// </summary>
        /// <remarks>
        /// Only applicable for questions with <c>Type</c> equal to <c>Numeric</c>
        /// </remarks>
        public int? Max { get; set; }

        /// <summary>
        /// Specify report format to ouput
        /// </summary>
        public ExportFileType? exportType { get; set; }

        /// <summary>
        /// Condition question variable name or UUID
        /// </summary>
        public string ConditionalQuestion { get; set; }

        /// <summary>
        /// List of condition question answers to filter on
        /// </summary>
        public long[] Condition { get; set; }

        /// <summary>
        /// Specify <c>true</c> if Pivot Table output needed. 
        /// </summary>
        /// <remarks>Only for Categorical questions (Single/Multy)</remarks>
        public bool Pivot { get; set; }

        /// <summary>
        /// Show detailed report with team members for each team
        /// </summary>
        /// <remarks>Ignored for supervisors</remarks>
        /// <remarks>Ignored for pivot table</remarks>
        public bool ExpandTeams { get; set; }
    }
}
