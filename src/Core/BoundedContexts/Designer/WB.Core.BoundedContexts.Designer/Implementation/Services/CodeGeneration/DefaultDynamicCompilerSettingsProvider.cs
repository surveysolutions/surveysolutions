using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DefaultDynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        DefaultDynamicCompilerSettings dynamicCompilerSettings = new DefaultDynamicCompilerSettings();

        public DefaultDynamicCompilerSettings DynamicCompilerSettings
        {
            get { return this.dynamicCompilerSettings; }
            set { this.dynamicCompilerSettings = value; }
        }

        public IDynamicCompilerSettings GetSettings(Version apiVersion)
        {
            return this.dynamicCompilerSettings;
        }
    }
}
