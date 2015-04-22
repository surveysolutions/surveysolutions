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
        private readonly QuestionnnaireVersion questionnaireVersion = new QuestionnnaireVersion
        {
            Major = 6,
            Minor = 0,
            Patch = 0
        };

        public QuestionnnaireVersion GetExpressionsEngineSupportedVersion()
        {
            return questionnaireVersion;
        }
    }
}
