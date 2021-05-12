#nullable enable
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Mutations
{
    [ExtendObjectType(Name = "HeadquartersMutations")]
    public class CalendarEventsMutationExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersMutation");

            descriptor.Field<CalendarEventResolver>(t => 
                    t.UpdateCalendarEvent(default,default, default!, default!, default!, 
                        default!, default!, default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer), nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<CalendarEventObjectType>()
                .Argument("comment", ad => ad.Description("Comment").Type<StringType>())
                .Argument("newStart", ad=>ad.Description("Start of event").Type<NonNullType<DateTimeType>>())
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<NonNullType<UuidType>>())
                .Argument("startTimezone", ad=> ad.Description("Start timezone").Type<NonNullType<StringType>>());

            
            descriptor.Field<CalendarEventResolver>(t => 
                    t.DeleteCalendarEvent(default,default!, default!, default!,default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer), nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<CalendarEventObjectType>()
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<NonNullType<UuidType>>());

            descriptor.Field<CalendarEventResolver>(t => 
                    t.AddAssignmentCalendarEvent(default,default, default!, default!, default!, 
                        default!, default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer), nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<CalendarEventObjectType>()
                .Argument("assignmentId", ad=>ad.Description("Assignment Id").Type<NonNullType<IntType>>())
                .Argument("comment", ad => ad.Description("Comment").Type<StringType>())
                .Argument("newStart", ad=>ad.Description("Start of event").Type<NonNullType<DateTimeType>>())
                .Argument("startTimezone", ad=> ad.Description("Start timezone").Type<NonNullType<StringType>>());


            descriptor.Field<CalendarEventResolver>(t => 
                    t.AddInterviewCalendarEvent(default,default, default!, default!, default!, 
                        default!, default!, default!))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer), nameof(UserRoles.ApiUser)
                })
                .HasWorkspace()
                .Type<CalendarEventObjectType>()
                .Argument("comment", ad => ad.Description("Comment").Type<StringType>())
                .Argument("interviewId", ad => ad.Description("Interview id").Type<NonNullType<UuidType>>())
                .Argument("newStart", ad=>ad.Description("Start of event").Type<NonNullType<DateTimeType>>())
                .Argument("startTimezone", ad=> ad.Description("Start timezone").Type<NonNullType<StringType>>());

        }
    }
}
