using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class ReferenceInfoForLinkedQuestions : IVersionedView
    {
        public ReferenceInfoForLinkedQuestions(Guid questionnaireId, long version,
            Dictionary<Guid, ReferenceInfoByQuestion> referencesOnLinkedQuestions)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.ReferencesOnLinkedQuestions = referencesOnLinkedQuestions;
        }

        public Guid QuestionnaireId { get; set; }
        public long Version { get; set; }
        public Dictionary<Guid, ReferenceInfoByQuestion> ReferencesOnLinkedQuestions { get; private set; }
    }
}
