using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class CalendarEventObjectType : ObjectType<CalendarEvent>
    {
        protected override void Configure(IObjectTypeDescriptor<CalendarEvent> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("CalendarEvent");
        
            descriptor.Field(x => x.PublicKey)
                .Description("Id of Calendar Event");
            
            descriptor.Field(x => x.Comment)
                .Description("Comment of calendar Event")
                .Type<StringType>();
            
            descriptor.Field(x => x.Start)
                .Description("Start of calendar Event");
        }   
    }
}


