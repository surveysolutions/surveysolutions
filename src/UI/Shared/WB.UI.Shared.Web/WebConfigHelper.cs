using System;
using System.Collections.Specialized;
using System.Configuration;

namespace WB.UI.Shared.Web
{
    public abstract class WebConfigHelper
    {
        private readonly NameValueCollection settingsCollection;

        protected WebConfigHelper(NameValueCollection settingsCollection)
        {
            this.settingsCollection = settingsCollection;
        }

        protected string GetString(string key)
        {
            return this.settingsCollection[key];
        }

        protected string GetString(string key, string @default)
        {
            return this.GetString(key) ?? @default;
        }
       
        protected bool GetBoolean(string key, bool @default)
        {
            return this.GetString(key).ToBoolean(@default);
        }
    
        protected DateTime GetDateTime(string key, DateTime @default)
        {
            return this.GetString(key).ToDateTime(@default);
        }
     
        protected int GetInt(string key)
        {
            return this.GetInt(key, 0);
        }
       
        protected int GetInt(string key, int @default)
        {
            return this.GetString(key).ToInt(@default);
        }
        
        public static T GetSection<T>(string sectionName)
        {
            return (T)ConfigurationManager.GetSection(sectionName);
        }
    }
}