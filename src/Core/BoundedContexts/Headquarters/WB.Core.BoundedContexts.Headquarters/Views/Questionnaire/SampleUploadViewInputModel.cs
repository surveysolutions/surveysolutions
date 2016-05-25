using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class SampleUploadViewInputModel
    {
        public SampleUploadViewInputModel(Guid id, long version)
        {
            this.QuestionnaireId = id;
            this.Version = version;
        }
       
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }
    }
}