using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireDocumentUpgrader
    {
        QuestionnaireDocument TranslatePropagatePropertiesToRosterProperties(QuestionnaireDocument document);
    }
}
