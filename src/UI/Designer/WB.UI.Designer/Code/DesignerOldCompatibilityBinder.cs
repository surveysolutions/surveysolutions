using System;
using System.Net.Http;
using System.Web;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Designer.Code
{
    public class DesignerOldCompatibilityBinder : OldToNewAssemblyRedirectSerializationBinder
    {
        private readonly Version headquartersVersion;

        public DesignerOldCompatibilityBinder(Version headquartersVersion)
        {
            this.headquartersVersion = headquartersVersion;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType.Namespace.StartsWith("WB.Core.SharedKernel.Structures"))
            {
                if (headquartersVersion == null || headquartersVersion < new Version(5, 22, 0))
                {
                    assemblyName = "WB.Core.SharedKernel.Structures";
                    typeName = serializedType.Name;

                    return;
                }
            }

            base.BindToName(serializedType, out assemblyName, out typeName);
        }
    }
}