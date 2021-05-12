using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.UI.Headquarters.Configs;

namespace WB.UI.Headquarters.Services.Impl
{
    [Localizable(false)]
    public class DesignerQuestionnaireApiRestServiceSettings : IRestServiceSettings
    {
        private readonly IProductVersion productVersion;

        public DesignerQuestionnaireApiRestServiceSettings(IOptions<DesignerConfig> designerConfig, 
            IProductVersion productVersion)
        {
            this.designerConfig = designerConfig;
            this.productVersion = productVersion;
        }

        public string Endpoint => designerConfig.Value.DesignerAddress;

        public TimeSpan Timeout => new TimeSpan(0, 0, 0, designerConfig.Value.RestTimeout);

        public int BufferSize => 512;

        public bool AcceptUnsignedSslCertificate => false;

        private static string _userAgent = null;
        private IOptions<DesignerConfig> designerConfig;

        public string UserAgent
        {
            get
            {
                if (_userAgent != null) return _userAgent;

                var flags = new List<string>();

                if (AcceptUnsignedSslCertificate)
                {
                    flags.Add("UNSIGNED_SSL");
                }

                _userAgent = $"WB.Headquarters/{this.productVersion.ToString()} ({string.Join(@" ", flags)})";
                return _userAgent;
            }
        }

        public int MaxDegreeOfParallelism { get; } = 10;
    }
}
