using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    public class WebInterviewConfig
    {
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Started { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}