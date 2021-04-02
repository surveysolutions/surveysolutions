using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    class MapReportHolder : ObjectType<IMapReportHolder>, IMapReportHolder
    {
        public MapReportHolder() : base(descriptor => Configure(descriptor))
        {
        }

        public MapReportHolder(IList nodes)
        {
            Nodes = nodes;
        }

        public IList Nodes { get; }

        private new static void Configure(IObjectTypeDescriptor<IMapReportHolder> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);
            descriptor.Name("MapReportHolder");

            descriptor.Field("report")
                .Description("Map report")
                .Type<NonNullType<MapReportType>>()
                .Resolve(ctx =>
                {
                    var nodes = ctx.Parent<IMapReportHolder>().Nodes as IList<GpsAnswerQuery>;
                    if (nodes == null)
                        return "empty";

                    ctx.Variables.TryGetVariable("zoom", out int zoom);
                    ctx.Variables.TryGetVariable("clientMapWidth", out int clientMapWidth);
                    ctx.Variables.TryGetVariable("east", out double east);
                    ctx.Variables.TryGetVariable("north", out double north);
                    ctx.Variables.TryGetVariable("west", out double west);
                    ctx.Variables.TryGetVariable("south", out double south);

                    var mapReport = ctx.Service<IMapReport>();
                    var report = mapReport.GetReport(
                        nodes.Select(x =>
                            new PositionPoint()
                            {
                                Latitude = x.Answer.Latitude,
                                Longitude = x.Answer.Longitude,
                                InterviewId = x.InterviewSummary.InterviewId
                            }).ToList(), zoom, clientMapWidth, south, north, east,west);
                    return report;
                });
        }
    }
}
