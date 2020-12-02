using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.CalendarEvents
{
    [TestOf(typeof(CalendarEventService))]
    public class CalendarEventServiceTests
    {
        [Test]
        public void when_getting_existing_calendar_event()
        {
            var calendarEventId = Id.g1;
            var calendarEventId2 = Id.g2;

            var calendarEventService = 
                Create.Service.CalendarEventService(
                    new CalendarEvent(){PublicKey = calendarEventId},
                    new CalendarEvent(){PublicKey = calendarEventId2});

            // Act
            var calendarEvent = calendarEventService.GetCalendarEventById(calendarEventId);

            Assert.That(calendarEvent, Is.Not.Null);
            Assert.That(calendarEvent.PublicKey, Is.EqualTo(calendarEventId));
        }

        [Test]
        public void when_getting_active_calendar_event_for_interview()
        {
            var calendarEventId = Id.g1;
            var interviewId = Id.g2;
            
            var calendarEventService = 
            Create.Service.CalendarEventService(new CalendarEvent()
            {
                PublicKey = calendarEventId, 
                InterviewId = interviewId,
                DeletedAtUtc = null,
                CompletedAtUtc = null
            });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForInterviewId(interviewId);

            Assert.That(calendarEvent, Is.Not.Null);
            Assert.That(calendarEvent.PublicKey, Is.EqualTo(calendarEventId));
        }
        
        [Test]
        public void when_getting_active_calendar_event_for_interview_and_event_is_deleted()
        {
            var calendarEventId = Id.g1;
            var interviewId = Id.g2;
            
            var calendarEventService = 
                Create.Service.CalendarEventService(new CalendarEvent()
                {
                    PublicKey = calendarEventId, 
                    InterviewId = interviewId,
                    DeletedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = null
                });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForInterviewId(interviewId);

            Assert.That(calendarEvent, Is.Null);
        }
        
        [Test]
        public void when_getting_active_calendar_event_for_interview_and_event_is_completed()
        {
            var calendarEventId = Id.g1;
            var interviewId = Id.g2;
            
            var calendarEventService = 
                Create.Service.CalendarEventService(new CalendarEvent()
                {
                    PublicKey = calendarEventId, 
                    InterviewId = interviewId,
                    DeletedAtUtc = null,
                    CompletedAtUtc = DateTime.UtcNow
                });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForInterviewId(interviewId);

            Assert.That(calendarEvent, Is.Null);
        }
        
        [Test]
        public void when_getting_active_calendar_event_for_assignment()
        {
            var calendarEventId = Id.g1;
            var assignmentId = 1;
            
            var calendarEventService = 
            Create.Service.CalendarEventService(new CalendarEvent()
            {
                PublicKey = calendarEventId, 
                AssignmentId = assignmentId,
                DeletedAtUtc = null,
                CompletedAtUtc = null
            });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignmentId);

            Assert.That(calendarEvent, Is.Not.Null);
            Assert.That(calendarEvent.PublicKey, Is.EqualTo(calendarEventId));
        }
        
        [Test]
        public void when_getting_active_calendar_event_for_assignment_and_event_is_deleted()
        {
            var calendarEventId = Id.g1;
            var assignmentId = 1;
            
            var calendarEventService = 
                Create.Service.CalendarEventService(new CalendarEvent()
                {
                    PublicKey = calendarEventId, 
                    AssignmentId = assignmentId,
                    DeletedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = null
                });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignmentId);

            Assert.That(calendarEvent, Is.Null);
        }
        
        [Test]
        public void when_getting_active_calendar_event_for_assignment_and_event_is_completed()
        {
            var calendarEventId = Id.g1;
            var assignmentId = 1;
            
            var calendarEventService = 
                Create.Service.CalendarEventService(new CalendarEvent()
                {
                    PublicKey = calendarEventId, 
                    AssignmentId = assignmentId,
                    DeletedAtUtc = null,
                    CompletedAtUtc = DateTime.UtcNow
                });
                
            // Act
            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignmentId);

            Assert.That(calendarEvent, Is.Null);
        }

        [Test]
        public void when_getting_calendar_events_for_user()
        {
            var calendarEventId = Id.g1;
            var calendarEventId2 = Id.g2;
            var calendarEventId3 = Id.g3;
            var calendarEventId4 = Id.g4;
            var calendarEventId5 = Id.g5;
            
            var assignmentId = 1;
            var assignmentId2 = 2;
            var assignmentId3 = 3;
            
            var interviewId = Id.g10;

            var userId = Id.gA;
            var userId2 = Id.gB;

            var assignments = new Assignment[] {
                new Assignment()
                {
                    Id = assignmentId,
                    PublicKey = Id.g9,
                    ResponsibleId = userId
                },
                new Assignment()
                {
                    Id = assignmentId2,
                    PublicKey = Id.g8,
                    ResponsibleId = userId
                },
                new Assignment()
                {
                    Id = assignmentId3,
                    PublicKey = Id.g7,
                    ResponsibleId = userId2
                },
            };

            var interviews = new Mock<IInterviewInformationFactory>();
            interviews
                .Setup(x => x.GetInProgressInterviewsForInterviewer(userId))
                .Returns(new List<InterviewInformation>()
                {
                    new InterviewInformation()
                    {
                        Id = interviewId
                    }
                } );
            var calendarEventService = 
                Create.Service.CalendarEventService(new[]{
                new CalendarEvent()
                {
                    PublicKey = calendarEventId,
                    InterviewId = interviewId,
                },
                new CalendarEvent()
                {
                    PublicKey = calendarEventId2,
                    InterviewId = interviewId,
                    CompletedAtUtc = DateTime.Now
                },
                new CalendarEvent()
                {
                    PublicKey = calendarEventId3,
                    AssignmentId = assignmentId
                },
                new CalendarEvent()
                {
                    PublicKey = calendarEventId4,
                    AssignmentId = assignmentId
                },
                new CalendarEvent()
                {
                    PublicKey = calendarEventId5,
                    AssignmentId = assignmentId2,
                    DeletedAtUtc = DateTime.Now
                }
            },
                    assignments,
                    interviews.Object);
            // Act
            var calendarEvents = calendarEventService.GetAllCalendarEventsForUser(userId);

            Assert.That(calendarEvents.Count, Is.EqualTo(1));
            Assert.That(calendarEvents.First().PublicKey, Is.EqualTo(calendarEventId));
        }
        
        [Test]
        public void when_getting_calendar_events_for_supervisor()
        {
            var calendarEventId = Id.g1;
            var calendarEventId2 = Id.g2;
            var calendarEventId3 = Id.g3;
            var calendarEventId4 = Id.g4;
            var calendarEventId5 = Id.g5;
            
            var assignmentId = 1;
            var assignmentId2 = 2;
            
            var interviewId = Id.g10;
            var interviewId1 = Id.g9;

            var userId = Id.gA;
            var supervisorId = Id.gC;
            
            var responsible = new ReadonlyUser();
            var profile = new ReadonlyProfile();
            profile.AsDynamic().SupervisorId = supervisorId;
            responsible.AsDynamic().ReadonlyProfile = profile;
            
            var assidnmentInTeam = new Assignment()
            {
                Id = assignmentId2,
                PublicKey = Id.g9,
                ResponsibleId = userId
            };
            assidnmentInTeam.AsDynamic().Responsible = responsible;
            
            var assignments = new [] {assidnmentInTeam};

            var interviews = new Mock<IInterviewInformationFactory>();
            interviews
                .Setup(x => x.GetInProgressInterviewsForSupervisor(supervisorId))
                .Returns(new List<InterviewInformation>()
                {
                    new InterviewInformation()
                    {
                        Id = interviewId
                    }
                } );

            var @event1 = new CalendarEvent()
                {
                    PublicKey = calendarEventId,
                    InterviewId = interviewId
                };
            @event1.AsDynamic().Creator = responsible;

            var @event2 = new CalendarEvent()
            {
                PublicKey = calendarEventId2,
                InterviewId = interviewId,
                CompletedAtUtc = DateTime.Now
            };
            event2.AsDynamic().Creator = responsible;

            var @event3 = new CalendarEvent()
            {
                PublicKey = calendarEventId3,
                AssignmentId = assignmentId
            };
            event3.AsDynamic().Creator = responsible;
            
            var @event4 = new CalendarEvent()
            {
                PublicKey = calendarEventId4,
                AssignmentId = assignmentId,
                InterviewId = interviewId1
            };
            event4.AsDynamic().Creator = responsible;
            
            var @event5 = new CalendarEvent()
            {
                PublicKey = calendarEventId5,
                AssignmentId = assignmentId2,
                DeletedAtUtc = DateTime.Now
            };
            event5.AsDynamic().Creator = responsible;

            var calendarEventService = 
                Create.Service.CalendarEventService(new[]{@event1, @event2, @event3, @event4, @event5},
                    assignments,
                    interviews.Object);
            // Act
            var calendarEvents = calendarEventService.GetAllCalendarEventsUnderSupervisor(supervisorId);

            Assert.That(calendarEvents.Count, Is.EqualTo(2));
            Assert.That(calendarEvents.First().PublicKey, Is.EqualTo(calendarEventId));
        }
    }
}
