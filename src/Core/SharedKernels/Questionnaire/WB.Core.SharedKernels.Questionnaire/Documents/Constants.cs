﻿namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Constants
    {
        public static readonly int MaxAmountOfItemsInLongRoster = 30;
        public static readonly int MaxRosterRowCount = 60;
        public static readonly int MaxLongRosterRowCount = 200;
        public static readonly int MinLongRosterRowCount = 1;
        public static readonly string HtmlRemovalPattern = "<.*?>";
        public static readonly int ThrottlePeriod = 500;
        public const int MaxRosterPropagationLimit = 10000;
        public const int MaxInterviewsCountByAssignment = 10000;
        public const int MaxTotalRosterPropagationLimit = 80000;
        public const int DefaultCascadingAsListThreshold = 50;
    }
}
