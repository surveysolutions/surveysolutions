using System.Collections.Generic;
using Newtonsoft.Json;
using Refit;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Storage;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester
{
    public class WebTesterModule : IModule
    {
        private static string DesignerAddress()
        {
            var baseAddress = System.Configuration.ConfigurationManager.AppSettings["DesignerAddress"];
            return $"{baseAddress.TrimEnd('/')}";
        }

        public void Load(IIocRegistry registry)
        {

            registry.BindToMethod<IDesignerWebTesterApi>(() => RestService.For<IDesignerWebTesterApi>(DesignerAddress(),
                new RefitSettings
                {
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
                        Binder = new OldToNewAssemblyRedirectSerializationBinder()
                    }
                }));
        }
    }
}