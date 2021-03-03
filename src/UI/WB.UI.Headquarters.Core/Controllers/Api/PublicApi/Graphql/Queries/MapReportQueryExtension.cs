using HotChocolate.Types;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class MapReportQueryExtension: ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");

            descriptor.Field<MapReportResolver>(x => x.GetMapReport(default, 
                    default,default, default,default,default,
                    default,default,default,default!,default!))
                .Authorize()
                .HasWorkspace()
                .WrapIntoHolder<MapReportType, GpsAnswerQuery>()
                .UseFiltering<GpsAnswerFilterInputType>()
                .Argument("questionnaireId", a => a.Description("Questionnaire id").Type<NonNullType<UuidType>>())
                .Argument("questionnaireVersion", a => a.Description("Questionnaire version").Type<LongType>())
                .Argument("variable", a => a.Description("Variable name for question").Type<StringType>())
                .Argument("zoom", a => a.Description("Zoom level").Type<NonNullType<IntType>>())
                .Argument("clientMapWidth", a => a.Description("Visible area width").Type<NonNullType<IntType>>())
                .Argument("east", a => a.Description("East coordinate").Type<NonNullType<FloatType>>())
                .Argument("west", a => a.Description("West coordinate").Type<NonNullType<FloatType>>())
                .Argument("north", a => a.Description("North coordinate").Type<NonNullType<FloatType>>())
                .Argument("south", a => a.Description("South coordinate").Type<NonNullType<FloatType>>());
        }
    }
}
