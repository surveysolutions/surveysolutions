using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    [ExtendObjectType(Name = "HeadquartersMutations")]
    public class CalendarEventsMutationExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersMutations");

            descriptor.Field<CalendarEventResolver>(t => 
                    t.DeleteCalendarEvent(default, default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer)
                })
                .Type<CalendarEventObjectType>()
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<NonNullType<UuidType>>());

            descriptor.Field<CalendarEventResolver>(t => 
                    t.AddOrUpdateCalendarEvent(default, default, default, default, 
                        default, default, default,default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer)
                })
                .Type<CalendarEventObjectType>()
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<UuidType>())
                .Argument("interviewId", ad => ad.Description("Interview id").Type<UuidType>())
                .Argument("interviewKey", ad=> ad.Description("Interview Key").Type<StringType>())
                .Argument("assignmentId", ad=>ad.Description("Assignment Id").Type<IntType>())
                .Argument("newStart", ad=>ad.Description("Start of event").Type<DateTimeType>())
                .Argument("comment", ad => ad.Description("Comment").Type<StringType>())
                .Argument("startTimezone", ad=> ad.Description("Start timezone").Type<StringType>());
        }
    }
}
