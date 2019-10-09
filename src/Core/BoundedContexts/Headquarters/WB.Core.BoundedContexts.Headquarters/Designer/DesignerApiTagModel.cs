using Refit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public class DesignerApiTagModel
    {
        public int HqTimeZone { get; set; }
        public string ImporterLogin { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Comment { get; set; }
        public string HqHost { get; internal set; }
    }

}
