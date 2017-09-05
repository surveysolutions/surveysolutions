using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Newtonsoft.Json;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Models
{
    [JsonConverter(typeof(TranslationModelConverter))]
    public class TranslationModel
    {
        public TranslationModel(params ResourceManager[] resources)
        {
            Add(resources);
        }

        public Dictionary<string, Dictionary<string, string>> Object { get; } = new Dictionary<string, Dictionary<string, string>>();

        public void Add(params ResourceManager[] resources)
        {
            foreach (var resource in resources)
            {
                IEnumerable<string> keys = resource
                    .GetResourceSet(CultureInfo.InvariantCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Select(entry => entry.Key)
                    .Cast<string>();

                var lastDot = resource.BaseName.LastIndexOf(@".", StringComparison.Ordinal);
                var @namespace = resource.BaseName.Substring(lastDot > 0 ? lastDot + 1 : 0);

                foreach (var key in keys)
                {
                    Add(@namespace, key, resource.GetString(key, CultureInfo.CurrentUICulture));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Object);
        }

        private void Add(string @namespace, string key, string value)
        {
            if (!Object.ContainsKey(@namespace))
            {
                Object.Add(@namespace, new Dictionary<string, string>());
            }

            Object[@namespace].Add(key, value);
        }
        
    }
}