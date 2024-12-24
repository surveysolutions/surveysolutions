using System;
using System.Globalization;
using System.Linq;
using GreenDonut.Predicates;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.GeoTracking;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.User;


namespace WB.UI.Headquarters.Controllers.Api;

[Route("api/{controller}/{action=Get}")]
public class AssignmentGeoApiController: ControllerBase
{
    private readonly IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage;
    private readonly IUserRepository userRepository;

    public AssignmentGeoApiController(
        IPlainStorageAccessor<GeoTrackingRecord> geoTrackingStorage,
        IUserRepository userRepository
        )
    {
        this.geoTrackingStorage = geoTrackingStorage;
        this.userRepository = userRepository;
    }
    
    public class GeoTrackingViewModel
    {
        public long Id { get; set; }
        public Guid InterviewerId { get; set; }
        public int AssignmentId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public GeoTrackingPointViewModel[] Points { get; set; }
    }
    
    public class GeoTrackingPointViewModel
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    [HttpGet]
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    public GeoTrackingViewModel[] GeoTrackingHistory(int assignmentId, long? trackId, Guid? responsibleId, 
        DateTimeOffset? start, DateTimeOffset? end)
    {
        var records = geoTrackingStorage
            .Query(r =>
            {
                r = r.Where(x => x.AssignmentId == assignmentId);
                
                if (trackId.HasValue)
                {
                    r = r.Where(t => t.Id == trackId.Value);
                }
                if (responsibleId.HasValue)
                {
                    r = r.Where(t => t.InterviewerId == responsibleId.Value);
                }
                if (start.HasValue)
                {
                    r = r.Where(t => t.End.HasValue && t.End.Value >= start.Value.UtcDateTime);
                }
                if (end.HasValue)
                {
                    r = r.Where(t => t.Start <= end.Value.UtcDateTime);
                }
                return r;
            })
            .ToArray();
        return records.Select(r => new GeoTrackingViewModel()
        {
            Id = r.Id,
            AssignmentId = r.AssignmentId,
            InterviewerId = r.InterviewerId,
            Start = r.Start,
            End = r.End,
            Points = r.Points.Select(p => new GeoTrackingPointViewModel()
            {
                Lat = p.Latitude,
                Lng = p.Longitude,
                Time = p.Time,
            }).ToArray()
        }).ToArray();
    }
    
    [HttpGet]
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    public ResponsibleComboboxModel GetInterviewers(int assignmentId, string query = "", int pageSize = 12)
    {
        var interviewersWithGeoTrackingrecords = geoTrackingStorage.Query(r => r
                    .Where(x => x.AssignmentId == assignmentId)
                    .Select(x => x.InterviewerId)
                );

        var users = userRepository.Users;
        users = users.Where(u => interviewersWithGeoTrackingrecords.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchByToLower = query.ToLower();
            
            users = users.Where(x =>
                (x.UserName != null && x.UserName.ToLower().Contains(searchByToLower))
                || (x.Email != null && x.Email.ToLower().Contains(searchByToLower)));
        }

        var total = users.Count();

        var filteredUsers = users
            .OrderBy(x => x.UserName)
            .Take(pageSize)
            .ToList()
            .Select(x => new ResponsibleComboboxOptionModel(x.Id.ToString(), x.UserName,  x.Role.ToString().ToLower()))
            .ToArray();

        var result = new ResponsibleComboboxModel(filteredUsers, total);
        return result;
    }
    
    [HttpGet]
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    public ResponsibleComboboxModel GetTracks(int assignmentId, string query = "", int pageSize = 12)
    {
        var filtered = geoTrackingStorage.Query(r =>
        {
            var tracks = r.Where(x => x.AssignmentId == assignmentId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchByToLower = query.ToLower();
            
                tracks = tracks.Where(x =>
                    x.Id.ToString().Contains(searchByToLower));
            }
            
            return tracks;
        });
        
        var total = filtered.Count();

        var items = filtered
            .OrderBy(x => x.Id)
            .Take(pageSize)
            .ToList()
            .Select(x => new ResponsibleComboboxOptionModel(
                x.Id.ToString(),  
                x.Id.ToString() + ". " + x.Start, 
                null))
            .ToArray();

        var result = new ResponsibleComboboxModel(items, total);
        return result;
    }
}
