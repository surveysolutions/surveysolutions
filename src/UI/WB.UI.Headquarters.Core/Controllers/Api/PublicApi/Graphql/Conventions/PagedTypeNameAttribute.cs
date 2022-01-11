using System;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Conventions
{
    public class PagedTypeNameAttribute : Attribute
    {
        public string Name { get; }

        public PagedTypeNameAttribute(string name)
        {
            Name = name;
        }
    }
}
