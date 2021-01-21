using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents
{
    public class CalendarEventObjectType : ObjectType<CalendarEvent>
    {
        protected override void Configure(IObjectTypeDescriptor<CalendarEvent> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("CalendarEvent");
            descriptor.Field(x => x.AssignmentId).Type<NonNullType<IntType>>();
            descriptor.Field(x => x.Comment)
                .Description("Comment of calendar Event")
                .Type<StringType>();
            descriptor.Field(x => x.CreatorUserId);
            
            descriptor.Field(x => x.InterviewId);
            descriptor.Field(x => x.InterviewKey).Type<StringType>();
            descriptor.Field(x => x.IsCompleted());
            descriptor.Field(x => x.PublicKey)
                .Type<NonNullType<UuidType>>()
                .Description("Id of Calendar Event");
            descriptor.Field(x => x.StartTimezone())
                .Type<NonNullType<StringType>>()
                .Description("Start Timezone of Event");

            descriptor.Field(x => x.StartUtc())
                .Type<NonNullType<DateTimeType>>()
                .Description("Start of calendar Event");
            
            descriptor.Field(x => x.UpdateDateUtc)
                .Type<NonNullType<DateTimeType>>();
        }   
    }
}
