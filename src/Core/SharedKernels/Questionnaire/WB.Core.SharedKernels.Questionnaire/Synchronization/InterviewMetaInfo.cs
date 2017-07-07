using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class InterviewMetaInfo
    {
        public InterviewMetaInfo()
        {
        }

        public Guid PublicKey { get; set; }

        public Guid TemplateId { get; set; }

        public long TemplateVersion { get; set; }

        public string Title { get; set; }

        public Guid ResponsibleId { get; set; }
        
        public int Status { get; set; }

        public DateTime? RejectDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }

        public IEnumerable<FeaturedQuestionMeta> FeaturedQuestionsMeta { get; set; }

        public string Comments { get; set; }

        public bool Valid { get; set; }

        public bool? CreatedOnClient { get; set; }
    }

    public class FeaturedQuestionMeta
    {
        public FeaturedQuestionMeta(Guid publicKey, string title, string value)
        {
            PublicKey = publicKey;
            Title = title;
            Value = value;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public string Value { get; private set; }
    }

    public class SurveyStatusMeta
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class QuestionnaireMetadata
    {
        public QuestionnaireMetadata(Guid questionnaireId, long version, bool allowCensusMode)
        {
            this.QuestionnaireId = questionnaireId;
            this.AllowCensusMode = allowCensusMode;
            this.Version = version;
        }
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }
        public bool AllowCensusMode { get; private set; }
    }

    public class QuestionnaireAssemblyMetadata
    {
        public QuestionnaireAssemblyMetadata(Guid questionnaireId, long version)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
        }
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }

    }
}
