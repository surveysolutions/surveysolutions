using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class CapiExpressionsEngineVersionService : ICapiExpressionsEngineVersionService
    {
        private readonly QuestionnaireVersion questionnaireVersion = new QuestionnaireVersion
        {
            Major = 6,
            Minor = 0,
            Patch = 0
        };

        public QuestionnaireVersion GetExpressionsEngineSupportedVersion()
        {
            return questionnaireVersion;
        }
    }
}
