using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Castle.Components.DictionaryAdapter;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    [ApiNoCache]
    [RoutePrefix("questionnaire/{id:Guid}")]
    public class CommentsController : ApiController
    {
        private readonly ICommentsService commentsService;
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public CommentsController(
            ICommentsService commentsService, 
            IMembershipUserService userHelper,
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.commentsService = commentsService;
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [CamelCase]
        [Route("commentThreads")]
        public List<CommentThread> commentThreads(Guid id)
        {
            bool hasAccess = this.userHelper.WebUser.IsAdmin || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.userHelper.WebUser.UserId);

            return hasAccess ? this.commentsService.LoadCommentThreads(id) : new List<CommentThread>();
        }

        [HttpGet]
        [CamelCase]
        [Route("entity/{itemId:Guid}/comments")]
        public List<CommentView> Get(Guid id, Guid itemId)
        {
            bool hasAccess = this.userHelper.WebUser.IsAdmin || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.userHelper.WebUser.UserId);
            return hasAccess ? this.commentsService.LoadCommentsForEntity(id, itemId) : new EditableList<CommentView>();
        }

        [HttpPost]
        [CamelCase]
        [Route("entity/addComment")]
        public HttpResponseMessage PostComment(Guid id, AddCommentModel comment)
        {
            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(new JsonResponseResult
                {
                    Error = string.Join(", ", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage))
                });
            }
            bool hasAccess = this.userHelper.WebUser.IsAdmin || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.userHelper.WebUser.UserId);
            if (!hasAccess)
            {
                return this.Request.CreateResponse(new JsonResponseResult
                {
                    Error = "Access denied"
                });
            }

            IMembershipWebUser user = this.userHelper.WebUser;
            commentsService.PostComment(comment.Id, comment.QuestionnaireId, comment.EntityId, comment.Comment, user.UserName, user.MembershipUser.Email);
            return this.Request.CreateResponse(new JsonResponseResult());
        }

        [HttpPatch]
        [CamelCase]
        [Route("comment/resolve/{commentdId:Guid}")]
        public HttpResponseMessage ResolveComment(Guid id, Guid commentdId)
        {
            commentsService.ResolveComment(commentdId);
            return this.Request.CreateResponse(new JsonResponseResult());
        }

        [HttpDelete]
        [Route("comment/{commentId:Guid}")]
        public HttpResponseMessage DeleteComment(Guid id, Guid commentId)
        {
            commentsService.DeleteComment(commentId);
            return this.Request.CreateResponse(new JsonResponseResult());
        }
    }
}
