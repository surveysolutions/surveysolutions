using System;
using HotChocolate;
using HotChocolate.Types.Descriptors;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class CompatibilityNamingConvention : DefaultNamingConventions
    {
        public override NameString GetEnumValueName(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.ToString().ToUpperInvariant();
        }
    }
}
