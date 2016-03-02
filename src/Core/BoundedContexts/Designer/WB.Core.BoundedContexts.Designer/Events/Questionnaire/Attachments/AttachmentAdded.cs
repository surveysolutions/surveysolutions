using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments
{
    public class AttachmentAdded : QuestionnaireActiveEvent
    {
        public AttachmentAdded() { }

        public AttachmentAdded(Guid attachmentId, Guid responsibleId)
        {
            this.AttachmentId = attachmentId;
            this.ResponsibleId = responsibleId;
        }

        public Guid AttachmentId { get; set; }
    }
}
