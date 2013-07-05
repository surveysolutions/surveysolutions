namespace WB.UI.Designer
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;

    /// <summary>
    /// The web config helper.
    /// </summary>
    public abstract class WebConfigHelper
    {
        private readonly NameValueCollection settingsCollection;

        public WebConfigHelper(NameValueCollection settingsCollection)
        {
            this.settingsCollection = settingsCollection;
        }

        #region Methods

        /// <summary>
        /// The get app string.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string GetString(string key)
        {
            return settingsCollection[key];
        }

        /// <summary>
        /// The get app string.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="default">
        /// The default.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string GetString(string key, string @default)
        {
            return this.GetString(key) ?? @default;
        }

        /// <summary>
        /// The get boolean.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="default">
        /// The default.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool GetBoolean(string key, bool @default)
        {
            return this.GetString(key).ToBoolean(@default);
        }

        /// <summary>
        /// The get date time.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="default">
        /// The default.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        protected DateTime GetDateTime(string key, DateTime @default)
        {
            return this.GetString(key).ToDateTime(@default);
        }

        /// <summary>
        /// The get int.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        protected int GetInt(string key)
        {
            return this.GetInt(key, 0);
        }

        /// <summary>
        /// The get int.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="default">
        /// The default.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        protected int GetInt(string key, int @default)
        {
            return this.GetString(key).ToInt(@default);
        }

        /// <summary>
        /// The get section.
        /// </summary>
        /// <param name="sectionName">
        /// The section name.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T GetSection<T>(string sectionName)
        {
            return (T)ConfigurationManager.GetSection(sectionName);
        }

        #endregion
    }
}