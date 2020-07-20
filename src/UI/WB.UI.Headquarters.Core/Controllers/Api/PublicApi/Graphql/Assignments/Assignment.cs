using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Assignments;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class Assignment : ObjectType<Core.BoundedContexts.Headquarters.Assignments.Assignment>
    {
        protected override void Configure(IObjectTypeDescriptor<Core.BoundedContexts.Headquarters.Assignments.Assignment> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("Assignment");
            descriptor.Field(x => x.Id)
                .Type<NonNullType<IdType>>();
            descriptor.Field(x => x.ResponsibleId)
                .Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.CreatedAtUtc)
                .Type<NonNullType<DateTimeType>>();

            descriptor.Field(x => x.ReceivedByTabletAtUtc)
                .Description("Will return `null` when assignment is not received by tablet");
            descriptor.Field(x => x.Email);
            // descriptor.Field(x => x.Quantity)
            //     .Description("Will return `null` for unlimited assignments, and actual number when its not unlimited");
            descriptor.Field(x => x.InterviewsNeeded);
            descriptor.Field(x => x.Archived);
            descriptor.Field(x => x.WebMode)
                .Type<NonNullType<BooleanType>>();
        }
    }
}
