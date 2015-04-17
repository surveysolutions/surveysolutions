using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ICapiExpressionsEngineVersionService
    {
        QuestionnnaireVersion GetExpressionsEngineSupportedVersion();
    }
}
