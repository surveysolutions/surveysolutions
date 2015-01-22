using Android.App;
using Android.Content;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class ApplicationSettings
    {
        private const string DesignerPathParameterName = "DesignerPath";

        public static void SetPathToDesigner(string path)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(Application.Context.Resources.GetString(Resource.String.ApplicationName),
                FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(DesignerPathParameterName, path);
            prefEditor.Commit();
        }

        public string GetPathToDesigner()
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(Application.Context.Resources.GetString(Resource.String.ApplicationName),
                FileCreationMode.Private);
            return prefs.GetString(DesignerPathParameterName, Application.Context.Resources.GetString(Resource.String.DesignerPath));
        }
    }
}
