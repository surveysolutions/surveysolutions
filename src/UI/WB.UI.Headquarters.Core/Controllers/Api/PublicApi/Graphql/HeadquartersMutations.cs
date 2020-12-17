using System;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersMutations : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            #region CalendarEvents

            descriptor.Field<CalendarEventResolver>(t => 
                    t.AddOrUpdateCalendarEvent(default, default, default, default, 
                        default, default, default,default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer)
                })
                .Type<CalendarEventObjectType>()
                .Argument("assignmentId", ad=>ad.Description("Assignment Id").Type<IntType>())
                .Argument("comment", ad => ad.Description("Comment").Type<StringType>())
                .Argument("interviewId", ad => ad.Description("Interview id").Type<UuidType>())
                .Argument("interviewKey", ad=> ad.Description("Interview Key").Type<StringType>())
                .Argument("newStart", ad=>ad.Description("Start of event").Type<DateTimeType>())
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<UuidType>())
                .Argument("startTimezone", ad=> ad.Description("Start timezone").Type<StringType>());

            
            descriptor.Field<CalendarEventResolver>(t => 
                    t.DeleteCalendarEvent(default, default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Interviewer)
                })
                .Type<CalendarEventObjectType>()
                .Argument("publicKey", a => a.Description("Calendar event publicKey").Type<NonNullType<UuidType>>());

            #endregion
            
            #region Maps
            
            descriptor.Field<MapsResolver>(t => t.DeleteMap(default, default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.ApiUser)
                })
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>());

            descriptor.Field<MapsResolver>(t => t.DeleteUserFromMap(default, default, default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.Supervisor),
                    nameof(UserRoles.ApiUser)
                })
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());

            descriptor.Field<MapsResolver>(t => t.AddUserToMap(default, default, default))
                .Use<TransactionMiddleware>()
                .Authorize(roles: new[]
                {
                    nameof(UserRoles.Administrator),
                    nameof(UserRoles.Headquarter),
                    nameof(UserRoles.Supervisor),
                    nameof(UserRoles.ApiUser)
                })
                .Type<Map>()
                .Argument("fileName", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());
            
            #endregion
        }
    }
}
