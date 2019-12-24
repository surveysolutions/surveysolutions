using System;

namespace WB.UI.Headquarters.Models.Template
{
    public class QuestionnaireListItemModel
    {
        public virtual string Id { get; set; }

        public virtual Guid QuestionnaireId { get; set; }

        public virtual long Version { get; set; }

        public virtual string Title { get; set; }
        
        public virtual bool AllowCensusMode { get; set; }

        public virtual bool IsDisabled { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual DateTime LastEntryDate { get; set; }

        public virtual DateTime? ImportDate { get; set; }

        public virtual bool IsAudioRecordingEnabled { get; set; }

        public bool WebModeEnabled { get; set; }
    }
}
