﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
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
            var interviewSummary = this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = data.InterviewId });

            if (interviewSummary == null)
                return null;

            return new InverviewChangeStateHistoryView()
            {
                HistoryItems = interviewSummary.StatusHistory.Select(x => new HistoryItemView()
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
            var view = this.interviewDetailsFactory.Load(
                new InterviewDetailsInputModel()
                {
                    CompleteQuestionnaireId = data.InterviewId,
                    CurrentGroupPublicKey = data.CurrentGroupId,
                    PropagationKey = data.CurrentPropagationKey,
                    User = this.GlobalInfo.GetCurrentUser()
                });

            var ret = new NewInterviewDetailsView()
            {
                InterviewInfo = new InterviewInfoModel()
                {
                    id = view.PublicKey.ToString(),
                    questionnaireId = view.QuestionnairePublicKey.ToString(),
                    title = view.Title,
                    status = Enum.GetName(typeof(InterviewStatus), view.Status)
                },
                Groups = view.Groups.Select(group => new GroupModel(group.ParentId)
                {
                    id = group.Id.ToString(),
                    depth = group.Depth,
                    title = group.Title,
                    rosterVector = group.RosterVector,
                    questions = new List<QuestionModel>(group.Questions.Select(q => this.SelectModelByQuestion(group, q)))
                })
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
                            Value = option.Value.ToString().Parse<decimal>(),
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
                    IEnumerable<decimal> answersAsDecimalArray = dto.Answer is IEnumerable
                        ? ((IEnumerable)dto.Answer).OfType<object>()
                            .Select(option => option.ToString().Parse<decimal>())
                        : new decimal[0];

                    bool areAnswersOrdered =
                        dto.Settings == null
                            ? true
                            : dto.Settings.GetType().GetProperty("AreAnswersOrdered").GetValue(dto.Settings, null);
                    int? maxAllowedAnswers =
                        dto.Settings == null
                            ? null
                            : dto.Settings.GetType().GetProperty("MaxAllowedAnswers").GetValue(dto.Settings, null);


                    model = new MultiQuestionModel()
                    {
                        options =
                            options.Select(
                                (option, index) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = answersAsDecimalArray.Contains(option.Value),
                                        label = option.Label,
                                        value = option.Value.ToString(),
                                        orderNo =
                                            dto.Scope == QuestionScope.Supervisor
                                                ? null
                                                : (!areAnswersOrdered
                                                    ? (int?)null
                                                    : (!answersAsDecimalArray.Contains(option.Value)
                                                        ? (int?)null
                                                        : answersAsDecimalArray.ToList().IndexOf(option.Value) + 1))
                                    }),
                        selectedOptions = answersAsDecimalArray.Select(answer => answer.ToString()),
                        areAnswersOrdered = areAnswersOrdered,
                        maxAllowedAnswers = maxAllowedAnswers,
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
                                (option) =>
                                    new OptionModel(uid)
                                    {
                                        isSelected = answerAsDecimal.HasValue && (answerAsDecimal.Value == option.Value),
                                        label = option.Label,
                                        value = option.Value.ToString()
                                    }),
                        selectedOption = answerAsString,
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