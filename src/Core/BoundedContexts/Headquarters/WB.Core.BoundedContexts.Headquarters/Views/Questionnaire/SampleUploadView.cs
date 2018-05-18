using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class SampleUploadView
    {
        public SampleUploadView(Guid questionnaireId, long questionnaireVersion,
            List<FeaturedQuestionItem> identifyingQuestions,
            List<string> hiddenQuestions, 
            List<string> rosterSizeQuestions)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.IdentifyingQuestions = identifyingQuestions;
            this.RosterSizeQuestions = rosterSizeQuestions ?? new List<string>();
            this.HiddenQuestions = hiddenQuestions ?? new List<string>();
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public List<FeaturedQuestionItem> IdentifyingQuestions { get; private set; }
        public IList<string> RosterSizeQuestions { get; }

        public IList<string> HiddenQuestions { get; private set; }
    }
}
