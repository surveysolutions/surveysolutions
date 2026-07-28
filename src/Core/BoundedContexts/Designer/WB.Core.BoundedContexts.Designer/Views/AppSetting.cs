using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Designer.Views
{
    [StoredIn(typeof(StoredAppSetting))]
    public class AppSetting : IStorableEntity
    {
        public static readonly string AssistantSettingsKey = "assistantsettings";
    }
    
    
    public class StoredAppSetting : KeyValueEntity
    {
    }
}
