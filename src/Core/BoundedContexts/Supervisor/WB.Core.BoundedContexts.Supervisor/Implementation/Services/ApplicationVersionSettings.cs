using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    public struct ApplicationVersionSettings
    {
        public int SupportedQuestionnaireVersionMajor { get; set; }

        public int SupportedQuestionnaireVersionMinor { get; set; }

        public int SupportedQuestionnaireVersionPatch { get; set; }
    }
}
