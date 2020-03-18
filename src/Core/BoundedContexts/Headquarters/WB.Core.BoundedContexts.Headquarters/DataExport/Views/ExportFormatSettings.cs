﻿using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    [Obsolete("KP-11815")]
    public static class ExportFormatSettings
    {
        public const string ExportDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public const string ExportDateFormat = "yyyy-MM-dd";
        public const string DefaultDelimiter = "|";
        public const string DisableValue = "";
        public const string MissingNumericQuestionValue = "-999999999";
        public const string MissingStringQuestionValue = "##N/A##";
        public const string MissingQuantityValue = "INF";
    }
}
