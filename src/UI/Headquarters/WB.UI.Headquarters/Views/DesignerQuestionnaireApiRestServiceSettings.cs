using System;

using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Views
{
    public class DesignerQuestionnaireApiRestServiceSettings : IRestServiceSettings
    {
        private readonly IConfigurationManager configurationManager;

        public DesignerQuestionnaireApiRestServiceSettings(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        public string Endpoint
        {
            get { return configurationManager.AppSettings["DesignerAddress"]; }
            set { throw new NotImplementedException(); }
        }

        public TimeSpan Timeout
        {
            get { return new TimeSpan(0, 0, 0, configurationManager.AppSettings["RestTimeout"].ToIntOrDefault(30)); }
            set { throw new NotImplementedException(); }
        }

        public int BufferSize
        {
            get { return 512; }
            set { throw new NotImplementedException(); }
        }

        public bool AcceptUnsignedSslCertificate
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }
    }
}