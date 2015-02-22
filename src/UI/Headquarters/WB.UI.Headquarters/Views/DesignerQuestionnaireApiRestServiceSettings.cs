using System;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.Shared.Web.Configuration;

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
            get { return new TimeSpan(0, 0, 0, 30); }
            set { throw new NotImplementedException(); }
        }

        public int BufferSize
        {
            get { return 512; }
            set { throw new NotImplementedException(); }
        }
    }
}