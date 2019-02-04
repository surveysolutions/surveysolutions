using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public abstract class PeriodicReportInputModelBase : ListViewModelBase
    {
        public DateTime From { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public int ColumnCount { get; set; }
        public int TimezoneOffsetMinutes { get; set; }
    }
}
