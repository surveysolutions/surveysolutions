using Android.App;
using Android.Content;
using WB.Core.GenericSubdomains.Utils.Services.Rest;

namespace WB.UI.QuestionnaireTester
{
    public class RestServiceSettings : IRestServiceSettings
    {
        private const string DesignerPath = "DesignerPath";

        public string BaseAddress()
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(Application.Context.Resources.GetString(Resource.String.ApplicationName),
                FileCreationMode.Private);
            return prefs.GetString(DesignerPath, Application.Context.Resources.GetString(Resource.String.DesignerPath));
        }
    }
}