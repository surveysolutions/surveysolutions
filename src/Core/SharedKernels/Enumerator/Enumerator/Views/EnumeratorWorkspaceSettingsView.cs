using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public abstract class EnumeratorWorkspaceSettingsView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public bool? PartialSynchronizationEnabled { get; set; } 
        public DateTime? LastSync { get; set; }
        public bool? LastSyncSucceeded { get; set; }
        public bool? DashboardViewsUpdated { get; set; }
        public string LastOpenedMapName { get; set; }
        public string WebInterviewUriTemplate { get; set; }
        public int? GeographyQuestionAccuracyInMeters { get; set; }
        public int? GeographyQuestionPeriodInSeconds { get; set; }
    }
}
