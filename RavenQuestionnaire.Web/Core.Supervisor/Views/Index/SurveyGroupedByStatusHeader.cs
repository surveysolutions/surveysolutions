namespace Core.Supervisor.Views.Index
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SurveyGroupedByStatusHeader : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> headers;

        public SurveyGroupedByStatusHeader(Dictionary<string, string> header)
        {
            headers = header;
        }

        public Dictionary<string, string> Header
        {
            get { return headers; }
        }

        public string GetItemValue<T>(T item, string key)
        {
            if (!headers.ContainsKey(key))
                throw new ArgumentException(string.Format("name {0} wasn't defined in header", key));
            var property = typeof(T).GetProperty(key);
            if (property == null)
                throw new ArgumentException(string.Format("property with name {0} wasn't defined in class", key));
            return property.GetValue(item, new object[0]).ToString();
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
