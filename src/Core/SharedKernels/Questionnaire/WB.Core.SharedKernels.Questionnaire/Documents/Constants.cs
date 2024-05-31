using System;
using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Constants
    {
        public static readonly int MaxRosterRowCount = 200;
        public static readonly int MinRosterRowCount = 1;
        public static readonly int MaxMultiComboboxAnswersCount = 200;
        public static readonly int MaxLinkedQuestionAnsweredOptionsCount = 200;
        public static readonly string HtmlRemovalPattern = "<.*?>";
        public static readonly int ThrottlePeriod = 500;
        public const int MaxRosterPropagationLimit = 10000;
        public const int MaxInterviewsCountByAssignment = 10000;
        public const int MaxTotalRosterPropagationLimit = 80000;
        public const int DefaultCascadingAsListThreshold = 50;
        
        public static readonly Regex HtmlRemovalRegex = new Regex(HtmlRemovalPattern, RegexOptions.Compiled, TimeSpan.FromMinutes(2));
    }
}
