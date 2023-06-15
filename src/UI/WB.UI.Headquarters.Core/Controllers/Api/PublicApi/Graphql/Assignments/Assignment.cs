using System.Linq;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class Assignment : ObjectType<Core.BoundedContexts.Headquarters.Assignments.Assignment>
    {
        protected override void Configure(IObjectTypeDescriptor<Core.BoundedContexts.Headquarters.Assignments.Assignment> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("Assignment");
            
            descriptor.Field(x => x.Archived);
            descriptor.Field(x => x.CreatedAtUtc).Type<NonNullType<DateTimeType>>();
            descriptor.Field(x => x.Email).Type<StringType>();
            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field(x => x.InterviewsNeeded);
            descriptor.Field(x => x.ReceivedByTabletAtUtc)
                .Description("Will return `null` when assignment is not received by tablet");
            
            descriptor.Field(x => x.ResponsibleId).Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.WebMode).Type<NonNullType<BooleanType>>();

            descriptor.Field("calendarEvent")
                .Description("Active Calendar Event associated with assignment")
                .Type<CalendarEventObjectType>()
                .Resolver(context =>
                {
                    var assignmentId = context.Parent<Core.BoundedContexts.Headquarters.Assignments.Assignment>().Id;
                    var unitOfWork = context.Service<IUnitOfWork>();
                  
                    var calendarEvent = unitOfWork.Session
                        .Query<CalendarEvent>()
                        .FirstOrDefault(x => x.AssignmentId == assignmentId 
                                             && x.CompletedAtUtc == null
                                             && x.DeletedAtUtc == null);
                    return calendarEvent;
                });
        }
    }
}
