﻿using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IQueryableReadSideRepositoryReader<MapReportPoint> answersByVariableStorage;
        private const int MAXCOORDINATESCOUNTLIMIT = 50000;

        public MapReport(IQueryableReadSideRepositoryReader<MapReportPoint> answersByVariableStorage)
        {
            this.answersByVariableStorage = answersByVariableStorage;
        }

        public MapReportView Load(MapReportInputModel input)
        {
            var points = this.answersByVariableStorage.Query(_ =>
                 this.AddMapFilterCondition(_, input).Where(x => x.QuestionnaireId == input.QuestionnaireId &&
                                                                       x.QuestionnaireVersion ==
                                                                       input.QuestionnaireVersion &&
                                                                       x.Variable == input.Variable)
                                                                       .Select(x => new { x.InterviewId, x.Latitude, x.Longitude })
                                                                       .Take(MAXCOORDINATESCOUNTLIMIT)
                                                                       .ToList());



            var mapPointViews = points.GroupBy(x => x.InterviewId).Select(x => new MapPointView
            {
                Id = x.Key.ToString(),
                Answers = string.Join("|", x.Select(val => $"{val.Latitude};{val.Longitude}"))
            }).ToArray();
            return new MapReportView
            {
                Points = mapPointViews
            };


        }

        private IQueryable<MapReportPoint> AddMapFilterCondition(IQueryable<MapReportPoint> _, MapReportInputModel input)
        {
            IQueryable<MapReportPoint> mapPoints;
            if (input.NorthEastCornerLongtitude >= input.SouthWestCornerLongtitude)
            {
                mapPoints = _.Where(x => x.Longitude > input.SouthWestCornerLongtitude &&
                                         x.Longitude < input.NorthEastCornerLongtitude);
            }
            else
            {
                mapPoints = _.Where(x => x.Longitude > input.SouthWestCornerLongtitude ||
                                         x.Longitude < input.NorthEastCornerLongtitude);
            }
            mapPoints = mapPoints.Where(x => x.Latitude > input.SouthWestCornerLatitude &&
                                             x.Latitude < input.NorthEastCornerLatitude);

            return mapPoints;
        }
    }
}
