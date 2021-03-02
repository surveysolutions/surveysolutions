using System;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

#nullable enable
namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class MapReportResolver
    {
        public MapReportView GetMapReport(
            Guid questionnaireId,
            long? questionnaireVersion,
            string variable,
            int zoom,
            int clientMapWidth,
            double east,
            double west,
            double north,
            double south,
            [Service] IMapReport mapReport)
        {
            return mapReport.Load(new MapReportInputModel()
            {
                ClientMapWidth = clientMapWidth,
                East = east,
                West = west,
                North = north,
                South = south,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Variable = variable,
                Zoom = zoom
            });
        }
    }
}
