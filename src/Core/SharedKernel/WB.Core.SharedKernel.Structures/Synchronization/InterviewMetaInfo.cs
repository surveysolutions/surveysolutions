using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    using System.Collections.Generic;

    public class InterviewMetaInfo
    {
        public InterviewMetaInfo()
        {
            ResponsibleId = null;
        }

        public Guid PublicKey { get; set; }

        public Guid TemplateId { get; set; }

        public string Title { get; set; }

        public Guid? ResponsibleId { get; set; }

        public SurveyStatusMeta Status { get; set; }

        public IEnumerable<FeaturedQuestionMeta> FeaturedQuestionsMeta { get; set; }

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

}
