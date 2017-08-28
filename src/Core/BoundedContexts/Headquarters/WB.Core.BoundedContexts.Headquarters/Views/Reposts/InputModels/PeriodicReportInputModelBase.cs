using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public abstract class PeriodicReportInputModelBase : ListViewModelBase
    {
        public DateTime From { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public int ColumnCount { get; set; }
        public int? TimezoneOffsetMinutes { get; set; }

        public QuestionnaireIdentity Questionnaire()
        {
            return QuestionnaireId != Guid.Empty ? new QuestionnaireIdentity(QuestionnaireId, QuestionnaireVersion) : null;
        }

        public DateTime FromAdjastedToUsersTimezone => 
            TimezoneOffsetMinutes.HasValue ? this.From.Date.AddMinutes(TimezoneOffsetMinutes.Value) : this.From.Date;
    }
}