using Android.App;
using Android.Content;
using Android.Preferences;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Services
{
    public class PreferencesStorage : IPreferencesStorage
    {
        private static ISharedPreferences SharedPreferences =>
            PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        public void Store(string key, string value)
        {
            var editor = SharedPreferences.Edit();
            editor.PutString(key, value);
            editor.Apply();
        }

        public string Get(string key) => SharedPreferences.GetString(key, null);
    }
}
