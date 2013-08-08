using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace Main.Core.Events.Questionnaire.Completed
{
    public class InterviewMetaInfoUpdated
    {
        public Guid TemplateId { get; set; }

        public string Title { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid StatusId { get; set; }

        public Guid PreviousStatusId { get; set; }

        public IEnumerable<FeaturedQuestionMeta> FeaturedQuestionsMeta { get; set; }
    }
}
