using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using KDBush;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Clustering;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private readonly IAuthorizedUser authorizedUser;

        public MapReport(IInterviewFactory interviewFactory, 
            IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor, 
            IAuthorizedUser authorizedUser)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnairesAccessor = questionnairesAccessor;
            this.authorizedUser = authorizedUser;
        }

        public List<string> GetGpsQuestionsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity)
                .Find<GpsCoordinateQuestion>().Select(question => question.StataExportCaption).ToList();

        protected static Cache Cache => System.Web.HttpContext.Current?.Cache;

        public MapReportView Load(MapReportInputModel input)
        {
            var key = $"{input.QuestionnaireIdentity};{input.Variable};{this.authorizedUser.Id}";

            var cacheLine = Cache?.Get(key);
            
            if (cacheLine == null)
            {
                cacheLine = InitializeSuperCluster(input);

                Cache?.Add(key, cacheLine, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15),
                    CacheItemPriority.Default, null);
            }

            var map = (MapReportCacheLine) cacheLine;

            List<Point<Cluster>> GetClusters(int zoom)
            {
                return map.Cluster.GetClusters(new GeoBounds(input.South, input.West, input.North, input.East), zoom);
            }

            if (input.Zoom == -1)
            {
                input.Zoom = map.Bounds.ApproximateGoogleMapsZoomLevel(input.ClientMapWidth);
            }

            var result = GetClusters(input.Zoom);

            var collection = new FeatureCollection();
            collection.Features.AddRange(result.Select(p =>
            {
                var props = p.UserData.Props ?? new Dictionary<string, object>();

                if (p.UserData.NumPoints > 1)
                {
                    props["count"] = p.UserData.NumPoints;
                    props["expand"] = map.Cluster.GetClusterExpansionZoom(p.UserData.Index);
                }

                return new Feature(
                    new Point(new Position(p.Latitude, p.Longitude)),
                    props, id: p.UserData.Index.ToString("X"));
            }));
            
            return new MapReportView
            {
                FeatureCollection = collection,
                InitialBounds = map.Bounds,
                TotalPoint = map.Total
            };
        }

        private MapReportCacheLine InitializeSuperCluster(MapReportInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(input.QuestionnaireIdentity, null);
            var gpsQuestionId = questionnaire.GetQuestionIdByVariable(input.Variable);

            if (!gpsQuestionId.HasValue) throw new ArgumentNullException(nameof(gpsQuestionId));
            
            var gpsAnswers = this.interviewFactory.GetGpsAnswers(
                input.QuestionnaireIdentity,
                gpsQuestionId.Value, null, GeoBounds.Open,
                this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?) null);

            var cluster = new SuperCluster();
            var bounds = GeoBounds.Closed;

            cluster.Load(gpsAnswers.Select(g =>
            {
                bounds.Expand(g.Latitude, g.Longitude);
                return new Feature(new Point(new Position(g.Latitude, g.Longitude)),
                    new Dictionary<string, object> {["interviewId"] = g.InterviewId.ToString()});
            }));

            return new MapReportCacheLine
            {
                Cluster = cluster,
                Bounds = bounds,
                Total = gpsAnswers.Length
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints() =>
            this.questionnairesAccessor.Query(_ => _.Where(x => !x.IsDeleted).ToList());

        private struct MapReportCacheLine
        {
            public SuperCluster Cluster { get; set; }
            public GeoBounds Bounds { get; set; }
            public int Total { get; set; }
        }
    }
}
