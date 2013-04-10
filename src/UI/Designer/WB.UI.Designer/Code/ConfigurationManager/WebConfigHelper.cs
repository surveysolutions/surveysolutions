using System;
using System.Configuration;

namespace WB.UI.Designer
{
    public abstract class WebConfigHelper
    {
        protected T GetSection<T>(string sectionName)
        {
            return (T) ConfigurationManager.GetSection(sectionName);
        }

        protected string GetString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        protected string GetString(string key, string @default)
        {
            return ConfigurationManager.AppSettings[key];
        }

        protected int GetInt(string key)
        {
            return GetInt(key, 0);
        }

        protected int GetInt(string key, int @default)
        {
            return GetString(key).ToInt(@default);
        }

        protected bool GetBoolean(string key, bool @default)
        {
            return GetString(key).ToBoolean(@default);
        }

        protected DateTime GetDateTime(string key, DateTime @default)
        {
            return GetString(key).ToDateTime(@default);
        }
    }
}