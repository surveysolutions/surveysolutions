using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json.Serialization;

namespace AndroidApp.Core.Model.ProjectionStorage
{
    public class CriteriaContractResolver : DefaultContractResolver
    {
      //  private List<string> _properties;

       
        protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            IList<JsonProperty> filtered = new List<JsonProperty>();

            foreach (JsonProperty p in base.CreateProperties(type,memberSerialization))
                if (p!=null && !p.DeclaringType.FullName.Contains("Cirrious.MvvmCross"))
                    filtered.Add(p);

            return filtered;
        }
    }
}