using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation
{
    internal class PropertiesOnlyContractResolver : DefaultContractResolver
    {
        //this inheritance has to be validated and tested
        public PropertiesOnlyContractResolver()
            : base(true)
        {
            this.DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var result = base.GetSerializableMembers(objectType);
            return result.Where(x => x.MemberType == MemberTypes.Property).ToList();
        }
    }
}