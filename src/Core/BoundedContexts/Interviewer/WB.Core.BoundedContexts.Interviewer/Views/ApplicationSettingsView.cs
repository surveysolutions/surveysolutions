using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class ApplicationSettingsView : EnumeratorSettingsView
    {
        public int GpsResponseTimeoutInSec { get; set; }
        public double? GpsDesiredAccuracy { get; set; }
        public bool? VibrateOnError { get; set; }
        public bool? ShowLocationOnMap { get; set; }
        public bool? AllowSyncWithHq { get; set; }
        public bool? IsOfflineSynchronizationDone { get; set; }
        public List<QuestionnaireIdentity>? QuestionnairesInWebMode { get; set; }
        public string? WebInterviewUriTemplate { get; set; }
    }
}
