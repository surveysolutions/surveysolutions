#nullable enable

using System;
using System.Reflection;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class EntityTypeObjectType : EnumType<EntityType>
    {
        protected override void Configure(IEnumTypeDescriptor<EntityType> descriptor)
        {
            descriptor.BindValuesExplicitly();
            var values = Enum.GetNames(typeof(EntityType));
            foreach (var questionType in values)
            {
                bool isObsolete = typeof (EntityType).GetField(questionType)?
                    .GetCustomAttribute(typeof (ObsoleteAttribute)) != null;
                if (!isObsolete)
                {
                    descriptor.Value(Enum.Parse<EntityType>(questionType));
                }
            }
        }
    }
}