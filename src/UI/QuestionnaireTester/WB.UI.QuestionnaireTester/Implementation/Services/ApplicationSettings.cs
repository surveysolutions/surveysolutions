using System;
using Android.App;
using Android.Content;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class ApplicationSettings
    {
        private const string DesignerPathParameterName = "DesignerPath";
        private const string HttpResponseTimeout = "HttpResponseTimeout";
        
        private static ISharedPreferences sharedPreferences
        {
            get
            {
                return Application.Context.GetSharedPreferences(Application.Context.Resources.GetString(Resource.String.ApplicationName),
                        FileCreationMode.Private);
            }
        }

        public static void SetPathToDesigner(string path)
        {
            ISharedPreferencesEditor prefEditor = sharedPreferences.Edit();
            prefEditor.PutString(DesignerPathParameterName, path);
            prefEditor.Commit();
        }

        public string GetPathToDesigner()
        {
            return sharedPreferences.GetString(DesignerPathParameterName, Application.Context.Resources.GetString(Resource.String.DesignerPath));
        }

        public TimeSpan GetHttpTimeout()
        {
            string stringTimeout = Application.Context.Resources.GetString(Resource.String.HttpResponseTimeout);

            return new TimeSpan(0, 0, 0, string.IsNullOrEmpty(stringTimeout) ? 30 : int.Parse(stringTimeout));
        }
    }
}
