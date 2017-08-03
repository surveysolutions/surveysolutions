using System;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedMetaDataView
    {
        public PreloadedMetaDataView(Guid questionnaireId, long version, string questionnaireTitle, PreloadedContentMetaData metaData)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.MetaData = metaData;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedContentMetaData MetaData { get; private set; }
        public string QuestionnaireTitle { get; set; }
    }
}
