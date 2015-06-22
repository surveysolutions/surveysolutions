using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeRecord : IView
    {
        public QuestionnaireChangeRecord()
        {
            this.References = new HashSet<QuestionnaireChangeReference>();
        }
        public virtual string QuestionnaireChangeRecordId { get; set; }
        public virtual string QuestionnaireId { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual int Sequence { get; set; }
        public virtual QuestionnaireActionType ActionType { get; set; }
        public virtual QuestionnaireItemType TargetItemType { get; set; }
        public virtual Guid TargetItemId { get; set; }
        public virtual string TargetItemTitle { get; set; }
        public virtual ISet<QuestionnaireChangeReference> References { get; set; }
    }
}
