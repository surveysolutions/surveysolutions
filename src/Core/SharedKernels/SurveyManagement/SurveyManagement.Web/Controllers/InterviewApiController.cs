using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
            IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.teamInterviewViewFactory = teamInterviewViewFactory;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewDetailsFactory = interviewDetailsFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
        }

        [HttpPost]
        public AllInterviewsView AllInterviews(DocumentListViewModel data)
        {
            var input = new AllInterviewsInputModel
            {
                Orders = data.SortOrder
            };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.QuestionnaireId = data.Request.TemplateId;
                input.QuestionnaireVersion = data.Request.TemplateVersion;
                input.TeamLeadId = data.Request.ResponsibleId;
                input.Status = data.Request.Status;
            }

            return this.allInterviewsViewFactory.Load(input);
        }

        [HttpPost]
        public TeamInterviewsView TeamInterviews(DocumentListViewModel data)
        {
            var input = new TeamInterviewsInputModel(viewerId: this.GlobalInfo.GetCurrentUser().Id)
            {
                Orders = data.SortOrder
            };

            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.QuestionnaireId = data.Request.TemplateId;
                input.QuestionnaireVersion = data.Request.TemplateVersion;
                input.ResponsibleId = data.Request.ResponsibleId;
                input.Status = data.Request.Status;
            }

            return this.teamInterviewViewFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor, Headquarter")]
        public InverviewChangeStateHistoryView ChangeStateHistory(ChangeStateHistoryViewModel data)
        {
            var interviewSummary = this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = data.InterviewId });

            if (interviewSummary == null)
                return null;

            return new InverviewChangeStateHistoryView()
            {
                HistoryItems = interviewSummary.StatusHistory.Select(x => new HistoryItemView()
                {
                    Comment = x.Comment,
                    Date = x.Date.ToShortDateString(),
                    State = x.Status.ToLocalizeString(),
                    Responsible = x.Responsible
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor, Headquarter")]
        public NewInterviewDetailsView InterviewDetails(InterviewDetailsViewModel data)
        {
            var view = this.interviewDetailsFactory.Load(
                new InterviewDetailsInputModel()
                {
                    CompleteQuestionnaireId = data.InterviewId,
                    CurrentGroupPublicKey = data.CurrentGroupId,
                    PropagationKey = data.CurrentPropagationKey,
                    User = this.GlobalInfo.GetCurrentUser()
                });

            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(data.InterviewId);

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter ||
                (this.GlobalInfo.IsSurepvisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                throw new HttpResponseException(HttpStatusCode.Forbidden);

            return new NewInterviewDetailsView()
            {
                InterviewInfo = new InterviewInfoModel()
                {
                    id = view.PublicKey.ToString(),
                    questionnaireId = view.QuestionnairePublicKey.ToString(),
                    title = view.Title,
                    status = view.Status.ToLocalizeString(),
                    responsible = view.Responsible != null ? view.Responsible.Name : null
                },
                Groups = view.Groups.Select(@group => new GroupModel(@group.ParentId)
                {
                    id = @group.Id.ToString(),
                    depth = @group.Depth,
                    title = @group.Title,
                    rosterVector = @group.RosterVector,
                    questions = new List<QuestionModel>(@group.Questions.Select(q => this.SelectModelByQuestion(@group, q)))
                })
            };
        }

        [Authorize(Roles = "Headquarter")]
        public InterviewSummaryForMapPointView InterviewSummaryForMapPoint(InterviewSummaryForMapPointViewModel data)
        {
            if (data == null)
                return null;

            var interviewSummaryView = this.interviewSummaryViewFactory.Load(data.InterviewId);
            if (interviewSummaryView == null)
                return null;

            var interviewSummaryForMapPointView = new InterviewSummaryForMapPointView()
            {
                InterviewerName = interviewSummaryView.ResponsibleName,
                SupervisorName = interviewSummaryView.TeamLeadName
            };

            if (interviewSummaryView.CommentedStatusesHistory != null &&
                interviewSummaryView.CommentedStatusesHistory.Any())
            {
                var lastStatus = interviewSummaryView.CommentedStatusesHistory.Last();

                interviewSummaryForMapPointView.LastStatus = lastStatus.Status.ToLocalizeString();

                var lastCompletedStatus =
                    interviewSummaryView.CommentedStatusesHistory.Last(statusInfo => statusInfo.Status == InterviewStatus.Completed);

                if (lastCompletedStatus != null)
                    interviewSummaryForMapPointView.LastCompletedDate =
                        AnswerUtils.AnswerToString(lastCompletedStatus.Date);
            }
            return interviewSummaryForMapPointView;
        }

        private QuestionModel SelectModelByQuestion(InterviewGroupView parentGroup, InterviewQuestionView dto)
        {
            QuestionModel model = null;
            string uid = string.Concat(dto.Id, "_", string.Join("_", parentGroup.RosterVector));
            var isLinked = dto is InterviewLinkedQuestionView;
            var hasAnswer = dto.Answer != null;
            string answerAsString = !hasAnswer ? string.Empty : dto.Answer.ToString();
            var avalibleOptions =
                dto.Options.Select(
                    option =>
                        new
                        {
                            Value =
                                isLinked
                                    ? (object)
                                        (((IEnumerable) option.Value).OfType<object>()
                                            .Select(x => x.ToString().Parse<decimal>())).ToArray()
                                    : option.Value.ToString().Parse<decimal>(),
                            Label = option.Label
                        });
            switch (dto.QuestionType)
            {
                case QuestionType.DateTime:
                    model = new DateQuestionModel() { answer = answerAsString };
                    break;
                case QuestionType.GpsCoordinates:
                    string accuracy = string.Empty,
                        latitude = string.Empty,
                        longitude = string.Empty,
                        timestamp = string.Empty,
                        altitude = string.Empty;

                    var answerAsGeoPosition = dto.Answer as GeoPosition;
                    if (answerAsGeoPosition != null)
                    {
                        accuracy = answerAsGeoPosition.Accuracy.ToString();
                        latitude = answerAsGeoPosition.Latitude.ToString();
                        longitude = answerAsGeoPosition.Longitude.ToString();
                        altitude = answerAsGeoPosition.Altitude.ToString();
                        timestamp = answerAsGeoPosition.Timestamp.ToString();
                    }
                    model = new GpsQuestionModel()
                    {
                        accuracy = accuracy,
                        latitude = latitude,
                        longitude = longitude,
                        timestamp = timestamp,
                        altitude = altitude
                    };
                    break;
                case QuestionType.AutoPropagate:
                case QuestionType.Numeric:
                    model = new NumericQuestionModel()
                    {
                        isInteger = dto.Settings == null ? true : dto.Settings.GetType().GetProperty("IsInteger").GetValue(dto.Settings, null),
                        countOfDecimalPlaces = dto.Settings == null ? null : dto.Settings.GetType().GetProperty("CountOfDecimalPlaces").GetValue(dto.Settings, null),
                        answer = answerAsString
                    };
                    break;
                case QuestionType.QRBarcode:
                    model = new QRBarcodeQuestionModel() { answer = answerAsString };
                    break;
                case QuestionType.Text:
                    model = new TextQuestionModel() { answer = answerAsString };
                    break;
                case QuestionType.MultyOption:
                    var answersAsDecimalArray = hasAnswer && !isLinked
                        ? ((IEnumerable)dto.Answer).OfType<object>().Select(option => option.ToString().Parse<decimal>()).ToArray()
                        : new decimal[0];
                    var answersAsDecimalArrayOnLinkedQuestion = hasAnswer && isLinked
                        ? ((IEnumerable) dto.Answer).OfType<IEnumerable>()
                            .Select(
                                option =>
                                    ((IEnumerable) option).OfType<object>()
                                        .Select(x => x.ToString().Parse<decimal>()).ToArray()).ToArray()
                        : new decimal[0][];
                    bool areAnswersOrdered =
                        dto.Settings == null
                            ? true
                            : dto.Settings.GetType().GetProperty("AreAnswersOrdered").GetValue(dto.Settings, null);
                    int? maxAllowedAnswers =
                        dto.Settings == null
                            ? null
                            : dto.Settings.GetType().GetProperty("MaxAllowedAnswers").GetValue(dto.Settings, null);
                    Func<object, bool> isSelected =
                        (option) => hasAnswer && (isLinked
                            ? answersAsDecimalArrayOnLinkedQuestion.Any(x=>x.SequenceEqual((decimal[]) option))
                            : answersAsDecimalArray.Contains((decimal) option));

                    Func<object, int?> getOrderNo = (optionValue) =>dto.Scope == QuestionScope.Supervisor
                                                ? null
                                                : (!areAnswersOrdered
                                                    ? (int?) null
                                                    : (!isSelected(optionValue))
                                                        ? (int?) null
                                                        : isLinked
                                                            ? Array.FindIndex(answersAsDecimalArrayOnLinkedQuestion,
                                                                o => o.SequenceEqual((decimal[]) optionValue)) + 1
                                                            : answersAsDecimalArray.ToList().IndexOf((decimal) optionValue) + 1);

                    model = new MultiQuestionModel()
                    {
                        options =
                            avalibleOptions.Select(
                                (option, index) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = isSelected(option.Value),
                                        label = option.Label,
                                        value = isLinked ? string.Join(", ", (decimal[]) option.Value) : option.Value.ToString(),
                                        orderNo = getOrderNo(option.Value)
                                    }),
                        selectedOptions =
                            isLinked
                                ? answersAsDecimalArrayOnLinkedQuestion.Select(answer => string.Join(", ", answer))
                                : answersAsDecimalArray.Select(answer => answer.ToString()),
                        areAnswersOrdered = areAnswersOrdered,
                        maxAllowedAnswers = maxAllowedAnswers,
                        answer =
                            string.Join(", ",
                                avalibleOptions.Where(option => isSelected(option.Value)).OrderBy(option => getOrderNo(option.Value))
                                    .Select(option => option.Label))
                    };

                    break;
                case QuestionType.SingleOption:
                    string answerLabel = string.Empty;
                    decimal? answerAsDecimal = hasAnswer && !isLinked ? dto.Answer.ToString().Parse<decimal>() : (decimal?)null;
                    var answersAsDecimalOnLinkedQuestion = hasAnswer && isLinked
                        ? ((IEnumerable)dto.Answer).OfType<object>().Select(option => option.ToString().Parse<decimal>()).ToArray()
                        : new decimal[0];
                    Func<object, bool> isSelectedOption =
                        (option) => hasAnswer && (isLinked
                            ? ((decimal[]) option).SequenceEqual(answersAsDecimalOnLinkedQuestion)
                            : (answerAsDecimal.HasValue && (decimal) option == answerAsDecimal));
                    var selectedAnswer = avalibleOptions.FirstOrDefault(option => isSelectedOption(option.Value));
                        answerLabel = selectedAnswer == null ? string.Empty : selectedAnswer.Label;
                    model = new SingleQuestionModel()
                    {
                        options =
                            avalibleOptions.Select(
                                (option) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = isSelectedOption(option.Value),
                                        label = option.Label,
                                        value = isLinked ? string.Join(", ", (decimal[])option.Value) : option.Value.ToString()/* option.Value.ToString()*/
                                    }),
                        selectedOption = isLinked ? string.Join(", ", answersAsDecimalOnLinkedQuestion) : answerAsDecimal.ToString(),
                        answer = answerLabel
                    };
                    break;
                case QuestionType.TextList:
                    model = new CategoricalQuestionModel()
                    {
                        options =
                            dto.Options.Select(
                                option => new OptionModel(uid) { value = option.Value.ToString(), label = option.Label })
                    };
                    break;
            }

            model.isReadonly = dto.IsReadOnly;
            model.variable = dto.Variable;
            model.comments =
                dto.Comments == null
                    ? new CommentModel[0]
                    : dto.Comments.Select(
                        comment =>
                            new CommentModel()
                            {
                                id = comment.Id.ToString(),
                                text = comment.Text,
                                userId = comment.CommenterId.ToString(),
                                userName = comment.CommenterName,
                                date = comment.Date
                            });
            model.scope = Enum.GetName(typeof(QuestionScope), dto.Scope);
            model.isAnswered = dto.IsAnswered;
            model.id = dto.Id;
            model.title = HttpUtility.UrlDecode(dto.Title);
            model.isFlagged = dto.IsFlagged;
            model.questionType = Enum.GetName(typeof(QuestionType), dto.QuestionType);
            model.isEnabled = dto.IsEnabled;
            model.isFeatured = dto.IsFeatured;
            model.isMandatory = dto.IsMandatory;
            model.rosterVector = parentGroup.RosterVector;
            model.isInvalid = !dto.IsValid;
            model.validationMessage = dto.ValidationMessage;
            model.validationExpression = dto.ValidationExpression;

            return model;
        }
    }

    public class GroupModel
    {
        private readonly Guid? parentIdPrivate;
        public GroupModel(Guid? parentId)
        {
            this.parentIdPrivate = parentId;
        }

        public IEnumerable<QuestionModel> questions { get; set; }
        public string id { get; set; }
        public int depth { get; set; }
        public string title { get; set; }
        public decimal[] rosterVector { get; set; }

        public string uiId
        {
            get { return string.Concat(this.id, "_", string.Join("_", this.rosterVector)); }
        }

        public string parentId
        {
            get
            {
                return this.parentIdPrivate.HasValue
                    ? string.Concat(this.parentIdPrivate, "_", string.Join("_", this.rosterVector.Take(this.rosterVector.Length - 1)))
                    : string.Empty;
            }
        }
    }

    public class InterviewInfoModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string questionnaireId { get; set; }
        public string responsible { get; set; }
    }
    public class NewInterviewDetailsView
    {
        public InterviewInfoModel InterviewInfo { get; set; }
        public IEnumerable<GroupModel> Groups { get; set; }
    }
    public class QuestionModel
    {
        public string uiId
        {
            get { return string.Concat(this.id, "_", string.Join("_", this.rosterVector)); }
        }

        public Guid id { set; get; }
        public string variable { set; get; }
        public IEnumerable<CommentModel> comments { set; get; }
        public bool isReadonly { set; get; }
        public bool isEnabled { set; get; }
        public bool isFeatured { set; get; }
        public bool isFlagged { set; get; }
        public bool isMandatory { set; get; }
        public decimal[] rosterVector { set; get; }
        public string questionType { set; get; }
        public string title { set; get; }
        public bool? isInvalid { set; get; }
        public bool isAnswered { set; get; }
        public string validationMessage { set; get; }
        public string validationExpression { set; get; }
        public string scope { set; get; }
        public bool? isValid { set; get; }

    }
    public class GpsQuestionModel : QuestionModel
    {
        public string latitude { set; get; }
        public string longitude { set; get; }
        public string accuracy { set; get; }
        public string timestamp { set; get; }
        public string altitude { set; get; }
    }
    public class TextQuestionModel : QuestionModel
    {
        public string answer { set; get; }
    }
    public class NumericQuestionModel : TextQuestionModel
    {
        public bool isInteger = true;
        public int? countOfDecimalPlaces { set; get; }
    }
    public class DateQuestionModel : TextQuestionModel { }
    public class QRBarcodeQuestionModel : TextQuestionModel { }
    public class CategoricalQuestionModel : TextQuestionModel
    {
        public IEnumerable<OptionModel> options { get; set; }
    }
    public class SingleQuestionModel : CategoricalQuestionModel
    {
        public string selectedOption { get; set; }
    }
    public class MultiQuestionModel : CategoricalQuestionModel
    {
        public bool areAnswersOrdered { get; set; }
        public int? maxAllowedAnswers { get; set; }
        public IEnumerable<string> selectedOptions { get; set; }
    }
    public class OptionModel
    {
        public OptionModel(string questionId)
        {
            this.questionId = questionId;
        }

        private string questionId { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public bool isSelected { get; set; }

        public string optionFor
        {
            get { return string.Concat("option-", this.questionId, "-", this.value); }
        }

        public int? orderNo { get; set; }
    }
    public class CommentModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public DateTime date { get; set; }
        public string userName { get; set; }
        public string userId { get; set; }
    }
}