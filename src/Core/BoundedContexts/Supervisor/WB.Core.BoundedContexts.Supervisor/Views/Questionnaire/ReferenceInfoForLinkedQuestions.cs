using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class ReferenceInfoForLinkedQuestions : IVersionedView
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
