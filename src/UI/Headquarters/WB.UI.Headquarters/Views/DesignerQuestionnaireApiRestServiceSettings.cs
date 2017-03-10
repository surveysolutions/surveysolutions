using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Views
{
    public class DesignerQuestionnaireApiRestServiceSettings : IRestServiceSettings
    {
        private readonly IConfigurationManager configurationManager;
        private readonly IProductVersion productVersion;

        public DesignerQuestionnaireApiRestServiceSettings(IConfigurationManager configurationManager, IProductVersion productVersion)
        {
            this.configurationManager = configurationManager;
            this.productVersion = productVersion;
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

        private static string _userAgent = null;

        [Localizable(false)]
        public string UserAgent
        {
            get
            {
                if (_userAgent != null) return _userAgent;

                var flags = new List<string>();

                if (AppSettings.IsDebugBuilded)
                {
                    flags.Add("DEBUG");
                }

                if (HttpContext.Current?.IsDebuggingEnabled ?? false)
                {
                    flags.Add("WEB_DEBUG");
                }

                if (AcceptUnsignedSslCertificate)
                {
                    flags.Add("UNSIGNED_SSL");
                }

                _userAgent = $"WB.Headquarters/{this.productVersion.ToString()} ({string.Join(@" ", flags)})";
                return _userAgent;
            }
        }
    }
}