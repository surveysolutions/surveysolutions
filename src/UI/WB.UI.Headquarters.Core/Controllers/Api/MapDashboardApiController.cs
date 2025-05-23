﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
        private readonly IPlainStorageAccessor<InterviewSummary> interviewSummaryReader;
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsStorage;
        private readonly IAssignmentsService assignmentsService;
        private readonly IMapStorageService mapStorageService;
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;
        private readonly IAuthorizedUser authorizedUser;

        public MapDashboardApiController(
            IInterviewFactory interviewFactory,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems,
            IAssignmentsService assignmentsService,
            IMapStorageService mapStorageService,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IPlainStorageAccessor<InterviewSummary> interviewSummaryReader,
            IAuthorizedUser authorizedUser,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsStorage)
        {
            this.interviewFactory = interviewFactory;
            this.questionnairesAccessor = questionnairesAccessor;
            this.questionnaireItems = questionnaireItems;
            this.assignmentsService = assignmentsService;
            this.mapStorageService = mapStorageService;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.interviewSummaryReader = interviewSummaryReader;
            this.authorizedUser = authorizedUser;
            this.assignmentsStorage = assignmentsStorage;
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
            
            public int? interviewsCount { get; set; }
            public int? assignmentsCount { get; set; }
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
            public int MaxZoom { get; set; }
            public int ClientMapWidth { get; set; }
        }

        public class MapDashboardResult
        {
            public FeatureCollection FeatureCollection { get; set; }
            public int TotalPoint { get; set; }
            public GeoBounds Bounds { get; set; }
        }
        
        [HttpPost]
        public ActionResult<MapDashboardResult> Markers([FromBody] MapDashboardRequest input)
        {
            if (input == null || !ModelState.IsValid)
                return StatusCode((int)HttpStatusCode.NotAcceptable);
            
            IncreaseBound(input, 0.2);
            
            var bounds = GeoBounds.Closed;

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
                    new MapPoint<MapMarker>(
                        g.Longitude, 
                        g.Latitude >= 0 ? Math.Min(g.Latitude, 85.05) : Math.Max(g.Latitude, -85.05), 
                        new MapInterviewMarker()
                        {
                            interviewId = g.InterviewId,
                            interviewKey = g.InterviewKey,
                            status = g.Status
                        }))
                .Concat(assignmentGpsData.Select(a =>
                    new MapPoint<MapMarker>(
                        a.Longitude, 
                        a.Latitude >= 0 ? Math.Min(a.Latitude, 85.05) : Math.Max(a.Latitude, -85.05), 
                        new MapAssignmentMarker()
                        {
                            assignmentId = a.AssignmentId,
                            responsibleRole = a.ResponsibleRoleId.ToUserRole(),
                        })))
                .ToList();
            
            mapPoints.ForEach(m => bounds.Expand(m.Y, m.X));
            
            if (input.Zoom == -1)
                input.Zoom = Math.Max(bounds.ApproximateGoogleMapsZoomLevel(input.ClientMapWidth), 3);

            var clustering = new Clustering<MapMarker>(input.Zoom);
            var valueCollection = clustering.RunClustering(mapPoints);
            var featureCollection = new FeatureCollection(valueCollection.Select(g =>
            {
                string id = g.ID;
                MapMarker mapMarker = null;
                if (g.Count > 1)
                {
                    var clusterMarker = new MapClusterMarker()
                    {
                        count = g.Count,
                        expand = input.Zoom + 1,
                    };
                    clusterMarker.interviewsCount = g.Points.Count(p => p.Data is MapInterviewMarker);
                    clusterMarker.assignmentsCount = clusterMarker.count - clusterMarker.interviewsCount;

                    mapMarker = clusterMarker;
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


            /*GeoBounds geoBounds = null;
            if (mapPoints.Any())
            {
                double south = mapPoints.Min(a => a.Y); 
                double west = mapPoints.Min(a => a.X); 
                double north  = mapPoints.Max(a => a.Y);
                double east = mapPoints.Max(a => a.X);
                geoBounds = new GeoBounds(south, west, north, east);
            }*/
                
            return new MapDashboardResult
            {
                FeatureCollection = featureCollection,
                //Bounds = geoBounds,
                Bounds = bounds,
                TotalPoint = mapPoints.Count
            };
        }

        private static void IncreaseBound(MapDashboardRequest input, double boundCoefficient)
        {
            //map is a cylinder, so we need to handle special cases when the day line is on the map
            input.East = Math.Min(input.East + (10 * boundCoefficient), 180);
            input.West = Math.Max(input.West - (10 * boundCoefficient), -180);
            
            input.North = Math.Min(input.North + ((input.North - input.South) * boundCoefficient), 180);
            input.South = Math.Max(input.South - ((input.North - input.South) * boundCoefficient), -180);
        }


        [HttpPost]
        public MapDashboardResult Markers2([FromBody] MapDashboardRequest input)
        {
            IncreaseBound(input, 1);

            var cluster = new SuperCluster();
            var bounds = GeoBounds.Closed;

            var gpsAnswers = this.interviewFactory.GetPrefilledGpsAnswers(
                input.QuestionnaireId, input.QuestionnaireVersion, 
                input.ResponsibleId, input.AssignmentId,
                input.East, input.North, input.West, input.South);

            var assignmentGpsData = this.assignmentsService.GetAssignmentsWithGpsAnswer(
                input.QuestionnaireId, input.QuestionnaireVersion, 
                input.ResponsibleId, input.AssignmentId,
                input.East, input.North, input.West, input.South);

            var features = gpsAnswers.Select(g =>
            {
                bounds.Expand(g.Latitude, g.Longitude);
                return new Feature(new Point(new Position(g.Latitude, g.Longitude)),
                    new MapInterviewMarker()
                    {
                        interviewId = g.InterviewId,
                        interviewKey = g.InterviewKey,
                        status = g.Status,
                    });
            }).Concat(assignmentGpsData.Select(g =>
            {
                bounds.Expand(g.Latitude, g.Longitude);
                return new Feature(new Point(new Position(g.Latitude, g.Longitude)),
                    new MapAssignmentMarker()
                    {
                        assignmentId = g.AssignmentId,
                        responsibleRole = g.ResponsibleRoleId.ToUserRole(),
                    });
            })).ToList();

            cluster.Load(features);

            if (input.Zoom == -1)
            {
                input.Zoom = bounds.ApproximateGoogleMapsZoomLevel(input.ClientMapWidth);
            }

            var result = cluster.GetClusters(new GeoBounds(input.South, input.West, input.North, input.East), input.Zoom);

            var collection = new FeatureCollection();
            collection.Features.AddRange(result.Select(p =>
            {
                if (p.UserData.NumPoints > 1)
                {
                    var mapMarker = new MapClusterMarker()
                    {
                        count = p.UserData.NumPoints,
                        expand = cluster.GetClusterExpansionZoom(p.UserData.Index),
                    };
                    return new Feature(
                        new Point(new Position(p.Latitude, p.Longitude)),
                        mapMarker, id: p.UserData.Index.ToString("X") + ":" + p.UserData.NumPoints);
                }

                string id = p.UserData.Index.ToString("X");
                if (p.UserData.Props.TryGetValue("interviewId", out var interviewId))
                    id = interviewId.ToString();
                else if (p.UserData.Props.TryGetValue("assignmentId", out var assignmentId))
                    id = assignmentId.ToString();

                return new Feature(
                        new Point(new Position(p.Latitude, p.Longitude)),
                        p.UserData.Props, 
                        id: id
                        );
            }));

            return new MapDashboardResult
            {
                FeatureCollection = collection,
                Bounds = bounds,
                TotalPoint = features.Count
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
                        && item.Featured == true
                        )
                    .Select(item => item.QuestionnaireIdentity)
                    .Distinct().ToList();
            });

            if (authorizedUser.IsInterviewer)
            {
                //get all questionnaires available for interviewer over all interviews
                var queryInterviewsResult = this.interviewSummaryReader.Query(_ =>
                {
                    var filter = _.Where(summary => summary.ResponsibleId == this.authorizedUser.Id);

                    return filter
                        .Select(s => new
                        {
                            s.QuestionnaireId,
                            s.QuestionnaireVersion
                        })
                        .Distinct().ToList();
                }).Select(q => 
                    new QuestionnaireIdentity(q.QuestionnaireId, q.QuestionnaireVersion).Id).ToArray();

                //get all questionnaires available for interviewer over all assignments
                var queryAssignmentsResult = this.assignmentsStorage.Query(_ =>
                {
                    var filter = _.Where(assignment => assignment.ResponsibleId == this.authorizedUser.Id
                            && assignment.Archived != true);

                    return filter
                        .Select(s => new
                        {
                            s.QuestionnaireId
                        })
                        .Distinct().ToList();
                }).Select(q => q.QuestionnaireId.Id).ToArray();

                var visibleQuestionnaires = queryAssignmentsResult.Union(queryInterviewsResult).ToList();
                
                questionnaireIds = questionnaireIds.Where(x => visibleQuestionnaires.Contains(x)).ToList();
            }

            return this.questionnairesAccessor.Query(_ => _
                .Where(x => !x.IsDeleted 
                            && questionnaireIds.Contains(x.Id)
                            && (query == null || x.Title.ToLower().Contains(lowerCaseQuery)))
                .ToList());
        }
        
        [HttpGet]
        [ApiNoCache]
        public ComboboxModel<ComboboxViewItem> Shapefiles(string query = null)
        {
            var maps = mapStorageService.GetUserShapefiles(query);
            return new ComboboxModel<ComboboxViewItem>(maps);
        }
        
        public class GeoJsonInfo
        {
            public string GeoJson { get; set; }
            public double XMax { get; set; }
            public double XMin { get; set; }
            public double YMax { get; set; }
            public double YMin { get; set; }
        }
        
        [HttpGet]
        public ActionResult<GeoJsonInfo> ShapefileInfo(string mapName)
        {
            var map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            return new GeoJsonInfo
            {
                GeoJson = map.GeoJson,
                XMax = map.XMaxVal,
                XMin = map.XMinVal,
                YMax = map.YMaxVal,
                YMin = map.YMinVal,
            };
        }
    }
}
