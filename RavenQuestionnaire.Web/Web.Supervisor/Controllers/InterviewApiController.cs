using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Http;
using Core.Supervisor.Views.ChangeStatus;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Interviews;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using Web.Supervisor.Models;
using System.Linq;

namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
            IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.teamInterviewViewFactory = teamInterviewViewFactory;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewDetailsFactory = interviewDetailsFactory;
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
            return new InverviewChangeStateHistoryView()
            {
                HistoryItems =
                    this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = data.InterviewId})
                        .StatusHistory.Select(x => new HistoryItemView()
                        {
                            Comment = x.Comment,
                            Date = x.Date.ToShortDateString(),
                            State = x.Status.ToLocalizeString()
                        })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor, Headquarter")]
        public NewInterviewDetailsView InterviewDetails(InterviewDetailsViewModel data)
        {
            var a = this.interviewDetailsFactory.Load(
                new InterviewDetailsInputModel()
                {
                    CompleteQuestionnaireId = data.InterviewId,
                    CurrentGroupPublicKey = data.CurrentGroupId,
                    PropagationKey = data.CurrentPropagationKey,
                    User = this.GlobalInfo.GetCurrentUser()
                });

            var ret = new NewInterviewDetailsView()
            {
                Details = a,
                Questions = a.Groups.Select(g =>
                    new {group = g, questions = g.Questions})
                    .SelectMany(x => new List<QuestionModel>(x.questions.Select(q => SelectModelByQuestion(x.group, q))))
            };

            return ret;
        }

        private QuestionModel SelectModelByQuestion(InterviewGroupView parentGroup, InterviewQuestionView dto)
        {
            QuestionModel model = null;
            string uid = string.Concat(dto.Id, "_", string.Join("_", parentGroup.RosterVector));
            string answerAsString = dto.Answer == null ? string.Empty : dto.Answer.ToString();
            var options =
                        dto.Options.Select(
                            option =>
                                new
                                {
                                    Value = decimal.Parse(option.Value.ToString(), CultureInfo.InvariantCulture),
                                    Label = option.Label
                                });
            switch (dto.QuestionType)
            {
                case QuestionType.DateTime:
                    model = new DateQuestionModel() {answer = answerAsString};
                    break;
                case QuestionType.GpsCoordinates:
                    string accuracy = string.Empty,
                        latitude = string.Empty,
                        longitude = string.Empty,
                        timestamp = string.Empty;

                    var answerAsGeoPosition = dto.Answer as GeoPosition;
                    if (answerAsGeoPosition != null)
                    {
                        accuracy = answerAsGeoPosition.Accuracy.ToString();
                        latitude = answerAsGeoPosition.Latitude.ToString();
                        longitude = answerAsGeoPosition.Longitude.ToString();
                        timestamp = answerAsGeoPosition.Timestamp.ToString();
                    }
                    model = new GpsQuestionModel()
                    {
                        accuracy = accuracy,
                        latitude = latitude,
                        longitude = longitude,
                        timestamp = timestamp
                    };
                    break;
                case QuestionType.Numeric:
                    model = new NumericQuestionModel()
                    {
                        //isInteger = dto.Settings == null ? true : dto.Settings.IsInteger,
                        //countOfDecimalPlaces = dto.Settings == null ? null : dto.Settings.CountOfDecimalPlaces,
                        answer = answerAsString
                    };
                    break;
                case QuestionType.QRBarcode:
                    model = new QRBarcodeQuestionModel() {answer = answerAsString};
                    break;
                case QuestionType.Text:
                    model = new TextQuestionModel() {answer = answerAsString};
                    break;
                case QuestionType.MultyOption:
                    IEnumerable<decimal> answersAsDecimalArray = dto.Answer == null
                        ? new decimal[0]
                        : ((IEnumerable<object>) dto.Answer).Select(option => option.ToString().Parse<decimal>());

                    model = new MultiQuestionModel()
                    {
                        options =
                            options.Select(
                                (option, index) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = answersAsDecimalArray.Contains(option.Value),
                                        label = option.Label,
                                        value = option.Value,
                                        orderNo = index
                                    }),
                        //areAnswersOrdered = dto.Settings == null ? true : dto.Settings.AreAnswersOrdered,
                        //maxAllowedAnswers = dto.Settings == null ? null : dto.Settings.MaxAllowedAnswers,
                        answer =
                            string.Join(", ",
                                options.Where(option => answersAsDecimalArray.Contains(option.Value))
                                    .Select(option => option.Label))
                    };
                    break;
                case QuestionType.SingleOption:
                    decimal? answerAsDecimal = null;
                    string answerLabel = string.Empty;
                    if (!string.IsNullOrEmpty(answerAsString))
                    {
                        answerAsDecimal = answerAsString.Parse<decimal>();
                        var selectedAnswer = options.FirstOrDefault(option => option.Value == answerAsDecimal);
                        answerLabel = selectedAnswer == null ? string.Empty : selectedAnswer.Label;
                    }
                    
                    model = new SingleQuestionModel()
                    {
                        options =
                            options.Select(
                                (option, index) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = answerAsDecimal.HasValue && (answerAsDecimal.Value == option.Value),
                                        label = option.Label,
                                        value = option.Value,
                                        orderNo = index
                                    }),
                        selectedOption = answerAsDecimal,
                        answer = answerLabel
                    };
                    break;
                case QuestionType.TextList:
                    model = new CategoricalQuestionModel()
                    {
                        options =
                            dto.Options.Select(
                                option => new OptionModel(uid) {value = option.Value, label = option.Label})
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
            model.scope = Enum.GetName(typeof (QuestionScope), dto.Scope);
            model.isAnswered = dto.IsAnswered;
            model.id = dto.Id;
            model.title = HttpUtility.UrlDecode(dto.Title);
            model.isFlagged = dto.IsFlagged;
            model.questionType = Enum.GetName(typeof (QuestionType), dto.QuestionType);
            model.isCapital = dto.IsCapital;
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

    public class NewInterviewDetailsView
    {
        public InterviewDetailsView Details { get; set; }
        public IEnumerable<QuestionModel> Questions { get; set; }
    }

    public class QuestionModel
    {
        public string uiId
        {
            get { return string.Concat(this.id, "_", string.Join("_", this.rosterVector)); }
        }

        public Guid id { set; get; }
        public string variable { set; get; }
        public bool isCapital { set; get; }
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
        public bool isVisible { set; get; }
        public bool isSelected { set; get; }
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

    public class DateQuestionModel : TextQuestionModel{}
    public class QRBarcodeQuestionModel : TextQuestionModel { }

    public class CategoricalQuestionModel : TextQuestionModel
    {
        public IEnumerable<OptionModel> options { get; set; }
    }

    public class SingleQuestionModel : CategoricalQuestionModel
    {
        public decimal? selectedOption { get; set; }
    }

    public class MultiQuestionModel : CategoricalQuestionModel
    {
        public bool areAnswersOrdered { get; set; }
        public int? maxAllowedAnswers { get; set; }
        public IEnumerable<OptionModel> selectedOptions { get; set; }
    }

    public class OptionModel
    {
        public OptionModel(string questionId)
        {
            this.questionId = questionId;
        }

        private string questionId { get; set; }
        public string label { get; set; }
        public object value { get; set; }
        public bool isSelected { get; set; }

        public string optionFor
        {
            get { return string.Concat("option-", this.questionId, "-", this.value); }
        }

        public int orderNo { get; set; }
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