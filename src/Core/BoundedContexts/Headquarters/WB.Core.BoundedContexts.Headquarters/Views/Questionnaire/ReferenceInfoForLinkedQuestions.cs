using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class ReferenceInfoForLinkedQuestions : IReadSideRepositoryEntity
    {
        public ReferenceInfoForLinkedQuestions()
        {
            this.ReferencesOnLinkedQuestions = new Dictionary<Guid, ReferenceInfoByQuestion>();
        }

        public Guid QuestionnaireId { get; set; }
        public long Version { get; set; }
        public Dictionary<Guid, ReferenceInfoByQuestion> ReferencesOnLinkedQuestions { get; set; }
    }
}
