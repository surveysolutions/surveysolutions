using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Infrastructure.Native.Storage
{
    public class CamelCasePropertyNamesContractResolverWithConverters : CamelCasePropertyNamesContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            var stringEnumConverter = new StringEnumConverter();
            
            // this will only be called once and then cached
            if (objectType == typeof(Identity))
            {
                contract.Converter = new IdentityJsonConverter();
            }
            if (objectType == typeof(RosterVector))
            {
                contract.Converter = new RosterVectorConverter();
            }
            if (stringEnumConverter.CanConvert(objectType))
            {
                contract.Converter = stringEnumConverter;
            }

            return contract;
        }
    }
}
