using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Main.Core.Entities.SubEntities;
using MarkerClustering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Supercluster;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Controllers.Services.Export;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Controllers.Api
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Interviewer, UserRoles.Supervisor)]
    [Route("api/[controller]/[action]")]
    public class MapDashboardApiController : ControllerBase
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;
        private readonly IAssignmentsService assignmentsService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IMemoryCache cache;

        public MapDashboardApiController(
            IInterviewFactory interviewFactory,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor,
            IAuthorizedUser authorizedUser,
            IMemoryCache cache,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems,
            IAssignmentsService assignmentsService)
        {
            this.interviewFactory = interviewFactory;
            this.questionnairesAccessor = questionnairesAccessor;
            this.authorizedUser = authorizedUser;
            this.cache = cache;
            this.questionnaireItems = questionnaireItems;
            this.assignmentsService = assignmentsService;
        }
        
        public enum MapMarkerType
        {
            Interview,
            Assignment,
            Cluster,
        }
        
        public abstract class MapMarker
        {
            public abstract MapMarkerType type { get; }
        }

        public class MapClusterMarker : MapMarker
        {
            public override MapMarkerType type => MapMarkerType.Cluster;
            public int count { get; set; }
            public int expand { get; set; }
        }

        public class MapInterviewMarker : MapMarker
        {
            public override MapMarkerType type => MapMarkerType.Interview;

            public Guid interviewId { get; set; }
            public string interviewKey { get; set; }
            public InterviewStatus status { get; set; }
        }

        public class MapAssignmentMarker : MapMarker
        {
            public override MapMarkerType type => MapMarkerType.Assignment;

            public int assignmentId { get; set; }
            public Guid publicKey { get; set; }
            public UserRoles responsibleRole { get; set; }
        }

        public class MapDashboardRequest
        {
            public Guid? QuestionnaireId { get; set; }
            public long? QuestionnaireVersion { get; set; }
            public Guid? ResponsibleId { get; set; }
            public int? AssignmentId { get; set; }

            public double East { get; set; }
            public double North { get; set; }

            public double West { get; set; }
            public double South { get; set; }

            public int Zoom { get; set; }
            public int ClientMapWidth { get; set; }
        }

        public class MapDashboardResult
        {
            public FeatureCollection FeatureCollection { get; set; }
            public int TotalPoint { get; set; }
            public GeoBounds Bounds { get; set; }
        }
        
        [HttpPost]
        public MapDashboardResult Markers([FromBody] MapDashboardRequest input)
        {
            double boundCoefficient = 0.5;
            input.East = Math.Min(input.East + ((input.East - input.West) * boundCoefficient), 180);
            input.West = Math.Max(input.West - ((input.East - input.West) * boundCoefficient), -180);
            input.North = Math.Min(input.North + ((input.North - input.South) * boundCoefficient), 180);
            input.South = Math.Max(input.South - ((input.North - input.South) * boundCoefficient), -180);
            
            var interviewGpsAnswers = this.interviewFactory.GetPrefilledGpsAnswers(
                input.QuestionnaireId, input.QuestionnaireVersion, 
                input.ResponsibleId, input.AssignmentId,
                input.East, input.North, input.West, input.South);

            var assignmentGpsData = this.assignmentsService.GetAssignmentsWithGpsAnswer(
                input.QuestionnaireId, input.QuestionnaireVersion, 
                input.ResponsibleId, input.AssignmentId,
                input.East, input.North, input.West, input.South);

            var mapPoints = 
                interviewGpsAnswers.Select(g =>
                    new MapPoint<MapMarker>(g.Longitude, g.Latitude, 
                        new MapInterviewMarker()
                        {
                            interviewId = g.InterviewId,
                            interviewKey = g.InterviewKey,
                            status = g.Status
                        }))
                .Concat(assignmentGpsData.Select(a =>
                    new MapPoint<MapMarker>(a.Longitude, a.Latitude, 
                        new MapAssignmentMarker()
                        {
                            assignmentId = a.AssignmentId,
                            responsibleRole = a.ResponsibleRoleId.ToUserRole(),
                            //publicKey = a.
                        })))
                .ToList();
            
            var clustering = new Clustering<MapMarker>(input.Zoom);
            var valueCollection = clustering.RunClustering(mapPoints).ToList();
            var featureCollection = new FeatureCollection(valueCollection.Select(g =>
            {
                string id = g.ID;
                MapMarker mapMarker = null;
                if (g.Count > 1)
                {
                    mapMarker = new MapClusterMarker()
                    {
                        count = g.Count,
                        expand = input.Zoom + 1,
                    };
                    id += $" {g.Count}";
                }
                else
                {
                    var mapPoint = g.Points[0];
                    mapMarker = mapPoint.Data;
                }
                
                return new Feature(new Point(
                        new Position(g.Y, g.X)),
                        mapMarker,
                        id);
            }).ToList());


            GeoBounds geoBounds = null;
            if (mapPoints.Any())
            {
                double south = mapPoints.Min(a => a.Y); 
                double west = mapPoints.Min(a => a.X); 
                double north  = mapPoints.Max(a => a.Y);
                double east = mapPoints.Max(a => a.X);
                geoBounds = new GeoBounds(south, west, north, east);
            }

                
            return new MapDashboardResult
            {
                FeatureCollection = featureCollection,
                Bounds = geoBounds,
                TotalPoint = valueCollection.Count
            };
        }


        [HttpPost]
        public MapDashboardResult Markers2([FromBody] MapDashboardRequest input)
        {
            var key = $"MapDashboard;{input.QuestionnaireId};{input.QuestionnaireVersion ?? 0L};{this.authorizedUser.Id}";

            var cacheLine = cache.Get(key);

            if (cacheLine == null)
            {
                lock (locker)
                {
                    cacheLine = cache.Get(key);

                    if (cacheLine == null)
                    {
                        var sw = Stopwatch.StartNew();
                        cacheLine = InitializeSuperCluster(input);
                        sw.Stop();

                        // cache for up to 10 minute depending on how long it took to read out map data
                        var cacheTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds * 5 + 1);

                        cache.Set(key, cacheLine, cacheTime);
                    }
                }
            }

            var map = (MapReportCacheLine) cacheLine;

            if (input.Zoom == -1)
            {
                input.Zoom = map.Bounds.ApproximateGoogleMapsZoomLevel(input.ClientMapWidth);
            }

            var result = map.Cluster.GetClusters(new GeoBounds(input.South, input.West, input.North, input.East), input.Zoom);

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

            return new MapDashboardResult
            {
                FeatureCollection = collection,
                Bounds = map.Bounds,
                TotalPoint = map.Total
            };
        }

        public string GetGpsQuestionByQuestionnaire(Guid? questionnaireId, long? version)
        {
            var questions = this.questionnaireItems.Query(q =>
            {
                if (questionnaireId.HasValue)
                {
                    if (version == null)
                    {
                        var questionnaire = questionnaireId.Value.FormatGuid();
                        q = q.Where(i => i.QuestionnaireIdentity.StartsWith(questionnaire));
                    }
                    else
                    {
                        var identity = new QuestionnaireIdentity(questionnaireId.Value, version.Value).ToString();
                        q = q.Where(i => i.QuestionnaireIdentity == identity);
                    }
                }

                q = q.Where(i => 
                    i.QuestionType == QuestionType.GpsCoordinates
                    && i.Featured == true
                );

                return q.Select(i => i.StataExportCaption).Distinct().ToList();
            });

            return questions.SingleOrDefault();
        }

        private static object locker = new object();

        private MapReportCacheLine InitializeSuperCluster(MapDashboardRequest input)
        {
            var gpsAnswers = this.interviewFactory.GetPrefilledGpsAnswers(
                input.QuestionnaireId, input.QuestionnaireVersion, 
                input.ResponsibleId, input.AssignmentId,
                input.East, input.North, input.West, input.South);

            var cluster = new SuperCluster();
            var bounds = GeoBounds.Closed;

            cluster.Load(gpsAnswers.Select(g =>
            {
                bounds.Expand(g.Latitude, g.Longitude);
                return new Feature(new Point(new Position(g.Latitude, g.Longitude)),
                    new MapInterviewMarker()
                    {
                        interviewId = g.InterviewId,
                        interviewKey = g.InterviewKey,
                        status = g.Status,
                    }
                    /*new Dictionary<string, object>
                    {
                        ["interviewId"] = g.InterviewId.ToString()
                    }*/);
            }));

            return new MapReportCacheLine
            {
                Cluster = cluster,
                Bounds = bounds,
                Total = gpsAnswers.Length
            };
        }

        [HttpGet]
        [ApiNoCache]
        public ComboboxModel<QuestionnaireVersionsComboboxViewItem> Questionnaires(string query = null)
        {
            var questionnaires = GetQuestionnaireIdentitiesWithGpsQuestions(query);
            var options = questionnaires.GetQuestionnaireComboboxViewItems().ToArray();
            return new ComboboxModel<QuestionnaireVersionsComboboxViewItem>(options);
        }

        private List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithGpsQuestions(string query)
        {
            string lowerCaseQuery = query?.ToLower();
            var questionnaireIds = this.questionnaireItems.Query(q =>
            {
                return q.Where(item => 
                        item.QuestionType == QuestionType.GpsCoordinates
                        //&& item.Featured == true
                        )
                    .Select(item => item.QuestionnaireIdentity)
                    .Distinct().ToList();
            });

            return this.questionnairesAccessor.Query(_ => _
                .Where(x => !x.IsDeleted 
                            && questionnaireIds.Contains(x.Id)
                            && (query == null || x.Title.ToLower().Contains(lowerCaseQuery)))
                .ToList());
        }

        private class MapReportCacheLine
        {
            public SuperCluster Cluster { get; set; }
            public GeoBounds Bounds { get; set; }
            public int Total { get; set; }
        }
    }
}