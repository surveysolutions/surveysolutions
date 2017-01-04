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

        public virtual bool Disabled { get; set; }

        public virtual string CreationDate { get; set; }

        public virtual string LastEntryDate { get; set; }

        public virtual string ImportDate { get; set; }
    }
}