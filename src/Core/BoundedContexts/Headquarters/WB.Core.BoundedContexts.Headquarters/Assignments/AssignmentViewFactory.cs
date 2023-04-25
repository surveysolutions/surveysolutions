using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Sanitizer;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentViewFactory : IAssignmentViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor;
        private readonly IWebInterviewConfigProvider WebInterviewConfigProvider;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IHeadquartersEventStore hqEventStore;
        private readonly IUserViewFactory userViewFactory;

        public AssignmentViewFactory(IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IQuestionnaireStorage questionnaireStorage,
            IHeadquartersEventStore hqEventStore,
            IUserViewFactory userViewFactory, 
            IWebInterviewConfigProvider webInterviewConfigProvider, 
            IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor)
        {
            this.assignmentsStorage = assignmentsStorage;
            this.summaries = summaries;
            this.questionnaireStorage = questionnaireStorage;
            this.hqEventStore = hqEventStore ?? throw new ArgumentNullException(nameof(hqEventStore));
            this.userViewFactory = userViewFactory;
            WebInterviewConfigProvider = webInterviewConfigProvider;
            this.calendarEventsAccessor = calendarEventsAccessor;
        }

        public AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input)
        {
            List<int> ids = new List<int>();
            var assignments = this.assignmentsStorage.Query(_ =>
            {
                var items = this.ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                ids = items.Skip(input.Offset)
                    .Take(input.Limit)
                    .Select(x => x.Id)
                    .ToList();

                var neededItems = _.Where(x => ids.Contains(x.Id));

                var fetchReqests = this.DefineOrderBy(neededItems, input)
                    .Fetch(x => x.IdentifyingData)
                    .Fetch(x => x.Responsible);
                    //.Fetch(x => x.RoleIds); //throws Null reference exception, but should be here :( https://stackoverflow.com/q/21243592/72174

                List<Assignment> list;

                if (input.ShowQuestionnaireTitle)
                {
                    list = fetchReqests.Fetch(x => x.Questionnaire).ToList();
                }
                else
                {
                    list = fetchReqests.ToList();
                }

                return list;
            });

            var counts = summaries.Query(_ => 
                (from i in _
                where ids.Contains((int)i.AssignmentId)
                group i by i.AssignmentId into gr
                select new
                {
                    gr.Key,
                    InterviewsCount = gr.Count()
                }).ToList());

            var questionnaires = assignments.Select(x => x.QuestionnaireId).Distinct();
            var webConfigs = questionnaires.Select(x => this.WebInterviewConfigProvider.Get(x))
                .ToList(); 
            
            var result = new AssignmentsWithoutIdentifingData
            {
                Page = input.Offset,
                PageSize = input.Limit,
                Items = assignments.Select(x =>
                {
                    var row = new AssignmentRow
                    {
                        QuestionnaireId = x.QuestionnaireId,
                        CreatedAtUtc = x.CreatedAtUtc,
                        ResponsibleId = x.ResponsibleId,
                        UpdatedAtUtc = x.UpdatedAtUtc,
                        WebModeEnabledOnQuestionnaire = webConfigs.First(w => w.QuestionnaireId.Equals(x.QuestionnaireId)).Started,
                        Quantity = x.Quantity ?? -1,
                        InterviewsCount = counts.FirstOrDefault(c => c.Key == x.Id)?.InterviewsCount ?? 0,
                        Id = x.Id,
                        Archived = x.Archived,
                        Responsible = x.Responsible.Name,
                        ResponsibleRole = x.Responsible.RoleIds.First().ToUserRole().ToString(),
                        IdentifyingQuestions = this.GetIdentifyingColumnText(x),
                        IsAudioRecordingEnabled = x.AudioRecording,
                        Email = x.Email,
                        Password = x.Password,
                        WebMode = x.WebMode,
                        ReceivedByTabletAtUtc = x.ReceivedByTabletAtUtc,
                        Comments = x.Comments,
                        CalendarEvent = GetCalendarEventForAssignmentOrNull(x.Id)
                    };

                    if (input.ShowQuestionnaireTitle)
                    {
                        row.QuestionnaireTitle = x.Questionnaire?.Title;
                    }

                    return row;
                }).ToList(),
            };

            result.TotalCount = this.assignmentsStorage.Query(_ => this.ApplyFilter(input, _).Count());

            return result;
        }

        private CalendarEventView GetCalendarEventForAssignmentOrNull(int id)
        {
            var calendarEvent = calendarEventsAccessor.Query(
                x => x.FirstOrDefault(
                    y => y.InterviewId == null
                         && y.AssignmentId == id
                         && y.DeletedAtUtc == null
                         && y.CompletedAtUtc == null));

            return calendarEvent == null ? null : new CalendarEventView(
                calendarEvent.Start,
                calendarEvent.Comment,
                calendarEvent.PublicKey);
        }

        public List<AssignmentIdentifyingQuestionRow> GetIdentifyingColumnText(Assignment assignment)
        {
            QuestionnaireIdentity assignmentQuestionnaireId = assignment.QuestionnaireId;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignmentQuestionnaireId, null);

            if (questionnaire == null) return new List<AssignmentIdentifyingQuestionRow>();

            List<AssignmentIdentifyingQuestionRow> identifyingColumnText =
                assignment.IdentifyingData
                          .Where(x => questionnaire.GetQuestionType(x.Identity.Id) != QuestionType.GpsCoordinates)
                          .Select(x => new AssignmentIdentifyingQuestionRow(questionnaire.GetQuestionTitle(x.Identity.Id).RemoveHtmlTags(),
                                    x.AnswerAsString,
                                    x.Identity,
                                    questionnaire.GetQuestionVariableName(x.Identity.Id)))
                          .ToList();
            return identifyingColumnText;
        }

        public async Task<AssignmentHistory> LoadHistoryAsync(Guid assignmentPublicKey, int offset, int limit)
        {
            var events = await this.hqEventStore.GetEventsInReverseOrderAsync(assignmentPublicKey, offset, limit)
                .ConfigureAwait(false);
            var result = new AssignmentHistory();

            var totalLength = await this.hqEventStore.TotalEventsCountAsync(assignmentPublicKey).ConfigureAwait(false);
            result.RecordsFiltered = totalLength;

            foreach (IEvent committedEvent in events.Select(x => x.Payload))
            {
                var assignmentEvent = (AssignmentEvent) committedEvent;
                var userName = userViewFactory.GetUser(new UserViewInputModel(assignmentEvent.UserId))?.UserName ?? "Unknown";

                var historyItem = new AssignmentHistoryItem(AssignmentHistoryAction.Unknown,
                    userName, 
                    assignmentEvent.OriginDate.UtcDateTime);

                switch (committedEvent)
                {
                    case AssignmentCreated c:
                        historyItem.Action = AssignmentHistoryAction.Created;

                        var responsible = userViewFactory.GetUser(new UserViewInputModel(c.ResponsibleId))?.UserName ?? "Unknown";
                        historyItem.AdditionalData = new
                        {
                            c.Comment,
                            Responsible = responsible,
                            UpgradedFromId = c.UpgradedFromId
                        };
                        break;
                    case AssignmentArchived _:
                        historyItem.Action = AssignmentHistoryAction.Archived;
                        break;
                    case AssignmentDeleted _:
                        historyItem.Action = AssignmentHistoryAction.Deleted;
                        break;
                    case AssignmentReceivedByTablet _:
                        historyItem.Action = AssignmentHistoryAction.ReceivedByTablet;
                        break;
                    case AssignmentUnarchived _:
                        historyItem.Action = AssignmentHistoryAction.UnArchived;
                        break;
                    case AssignmentAudioRecordingChanged a:
                        historyItem.Action = AssignmentHistoryAction.AudioRecordingChanged;
                        historyItem.AdditionalData = new
                        {
                            a.AudioRecording
                        };
                        break;
                    case AssignmentQuantityChanged q:
                        historyItem.Action = AssignmentHistoryAction.QuantityChanged;
                        historyItem.AdditionalData = new
                        {
                            q.Quantity
                        };
                        break;
                    case AssignmentReassigned r:
                        historyItem.Action = AssignmentHistoryAction.Reassigned;
                        var targetLogin =
                            this.userViewFactory.GetUser(r.ResponsibleId)?.UserName ?? "Unknown";
                        historyItem.AdditionalData = new
                        {
                            NewResponsible = targetLogin,
                            r.Comment
                        };
                        break;
                    case AssignmentWebModeChanged w:
                        historyItem.Action = AssignmentHistoryAction.WebModeChanged;
                        historyItem.AdditionalData = new
                        {
                            w.WebMode
                        };
                        break;
                }

                result.History.Add(historyItem);
            }

            return result;
        }

        private IQueryable<Assignment> DefineOrderBy(IQueryable<Assignment> query, AssignmentsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x => x.UpdatedAtUtc);
            }

            if (orderBy.Field.Contains("InterviewsCount"))
            {
                return OrderByInterviewsCount(query, orderBy);
            }

            if (orderBy.Field.Contains("QuestionnaireTitle"))
            {
                return OrderByQuestionnaire(query, orderBy);
            }

            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

        private static readonly Expression<Func<Assignment, int>> OrderByQuery = x => x.InterviewSummaries.Count;
        private static IQueryable<Assignment> OrderByInterviewsCount(IQueryable<Assignment> query, OrderRequestItem orderBy)
        {
            return orderBy.Direction == OrderDirection.Asc 
                ? query.OrderBy(OrderByQuery) 
                : query.OrderByDescending(OrderByQuery);
        }

        static readonly Expression<Func<Assignment, string>> OrderByQuestionnaireTitle = x => x.Questionnaire.Title;
        private static IQueryable<Assignment> OrderByQuestionnaire(IQueryable<Assignment> query, OrderRequestItem orderBy)
        {
            return orderBy.Direction == OrderDirection.Asc 
                ? query.OrderBy(OrderByQuestionnaireTitle) 
                : query.OrderByDescending(OrderByQuestionnaireTitle);
        }

        private IQueryable<Assignment> ApplyFilter(AssignmentsInputModel input, IQueryable<Assignment> assignments)
        {
            var items = assignments.Where(x => x.Archived == input.ShowArchive);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                int id = 0;

                var lowerSearchBy = input.SearchBy.ToLower();

                Expression<Func<Assignment, bool>> textSearchExpression = x => false;

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.IdentifyingQuestions))
                {
                    textSearchExpression = textSearchExpression
                        .OrCondition(x => x.IdentifyingData.Any(a => a.AnswerAsString.ToLower().Contains(lowerSearchBy)));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.ResponsibleId))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Responsible.Name.ToLower().Contains(lowerSearchBy));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.ResponsibleId))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Questionnaire.Title.ToLower().Contains(lowerSearchBy));
                }

                if (input.SearchByFields.HasFlag(AssignmentsInputModel.SearchTypes.Id) && int.TryParse(input.SearchBy, out id))
                {
                    textSearchExpression = textSearchExpression.OrCondition(x => x.Id == id);
                }

                items = items.Where(textSearchExpression);
            }

            if (input.ReceivedByTablet != AssignmentReceivedState.All)
            {
                if (input.ReceivedByTablet == AssignmentReceivedState.NotReceived)
                {
                    items = items.Where(x => x.ReceivedByTabletAtUtc == null);
                }
                else if (input.ReceivedByTablet == AssignmentReceivedState.Received)
                {
                    items = items.Where(x => x.ReceivedByTabletAtUtc != null);
                }
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId.QuestionnaireId == input.QuestionnaireId);
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId.Version == input.QuestionnaireVersion);
            }

            if (input.ResponsibleId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.ResponsibleId);
            }

            if (input.SupervisorId.HasValue)
            {
                items = items.Where(x => x.Responsible.ReadonlyProfile.SupervisorId == input.SupervisorId || x.ResponsibleId == input.SupervisorId);
            }

            if (input.OnlyWithInterviewsNeeded)
            {
                items = items.Where(x => !x.Quantity.HasValue || x.Quantity - x.InterviewSummaries.Count > 0);
            }

            if (input.NonCawiOnly)
            {
                items = items.Where(x => x.WebMode != true);
            }

            if (input.DateStart.HasValue || input.DateEnd.HasValue)
            {
                items = items.Where(x => 
                    x.Quantity.HasValue 
                    && (x.CreatedAtUtc >= input.DateStart || input.DateStart == null)
                    && (x.CreatedAtUtc <  input.DateEnd   || input.DateEnd == null)
                );
            }

            if (input.UserRole.HasValue)
            {
                items = items.Where(x => 
                    x.Responsible.RoleIds.Any(r => r == input.UserRole.Value.ToUserId())
                );
            }

            if (input.Id.HasValue)
            {
                items = items.Where(x => x.Id == input.Id);
            }

            return items;
        }
    }
}
