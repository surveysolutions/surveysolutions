using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;

using Newtonsoft.Json;

using WB.Core.Infrastructure.Backup;

namespace WB.UI.Capi.SharedPreferences
{
    public class SharedPreferencesBackupOperator : IBackupable
    {
        private const string PreferencesFileName = "Preferences.txt";

        private readonly string PreferencesFullPathToExport;

        public SharedPreferencesBackupOperator()
        {
            this.PreferencesFullPathToExport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                                               PreferencesFileName);
        }

        public void ExportSettingsToFile()
        {
            ISharedPreferences sharedPreferences = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);

            Dictionary<string, object> preferences = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> pair in sharedPreferences.All)
            {
                preferences.Add(pair.Key, pair.Value);
            }

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            string preferencesAsString = JsonConvert.SerializeObject(preferences, Formatting.None, settings);

            File.WriteAllText(this.PreferencesFullPathToExport, preferencesAsString);

        }

        public string GetPathToBackupFile()
        {
            this.ExportSettingsToFile();
            
            return this.PreferencesFullPathToExport;
        }

        public void RestoreFromBackupFolder(string path)
        {
            string pathToPreferences = Path.Combine(path, PreferencesFileName);

            var preferences = new Dictionary<string, object>();

            if (File.Exists(pathToPreferences))
            {
                var preferencesAsText = File.ReadAllText(pathToPreferences);

                preferences = JsonConvert.DeserializeObject<Dictionary<string, object>>(preferencesAsText,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
            }

            ISharedPreferences sharedPreferences = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = sharedPreferences.Edit();
            prefEditor.Clear();

            foreach (var item in preferences)
            {
                if (item.Value is bool)
                    prefEditor.PutBoolean(item.Key, (bool)item.Value);
                else if (item.Value is float)
                    prefEditor.PutFloat(item.Key, (float)item.Value);
                else if (item.Value is int)
                    prefEditor.PutInt(item.Key, (int)item.Value);
                else if (item.Value is long)
                    prefEditor.PutLong(item.Key, (long)item.Value);
                else if (item.Value is string)
                    prefEditor.PutString(item.Key, (string)item.Value);
            }

            prefEditor.Commit();
            
        }
    }
}
