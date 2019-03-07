using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    [ResponseCache(NoStore = true)]
    [Route("questionnaire/{id:Guid}")]
    public class CommentsController : Controller
    {
        private readonly ICommentsService commentsService;
        private readonly ILoggedInUser loggedInUser;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public CommentsController(
            ICommentsService commentsService, 
            ILoggedInUser loggedInUser,
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.commentsService = commentsService;
            this.loggedInUser = loggedInUser;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        [Route("commentThreads")]
        public List<CommentThread> commentThreads(Guid id)
        {
            bool hasAccess = this.loggedInUser.IsAdmin || 
                             this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.loggedInUser.Id);

            return hasAccess ? this.commentsService.LoadCommentThreads(id) : new List<CommentThread>();
        }

        [HttpGet]
        [Route("entity/{itemId:Guid}/comments")]
        public List<CommentView> Get(Guid id, Guid itemId)
        {
            bool hasAccess = this.loggedInUser.IsAdmin|| this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.loggedInUser.Id);
            return hasAccess ? this.commentsService.LoadCommentsForEntity(id, itemId) : new List<CommentView>();
        }

        [HttpPost]
        [Route("entity/addComment")]
        public IActionResult PostComment(Guid id, AddCommentModel comment)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    Error = string.Join(", ", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage))
                });
            }
            bool hasAccess = this.loggedInUser.IsAdmin|| this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id, this.loggedInUser.Id);
            if (!hasAccess)
            {
                return Json(new
                {
                    Error = "Access denied"
                });
            }

            commentsService.PostComment(comment.Id, comment.QuestionnaireId, comment.EntityId, comment.Comment, loggedInUser.Login, loggedInUser.Email);
            return Ok();
        }

        [HttpPatch]
        [Route("comment/resolve/{commentdId:Guid}")]
        public IActionResult ResolveComment(Guid id, Guid commentdId)
        {
            commentsService.ResolveComment(commentdId);
            return Ok();
        }

        [HttpDelete]
        [Route("comment/{commentId:Guid}")]
        public IActionResult DeleteComment(Guid id, Guid commentId)
        {
            commentsService.DeleteComment(commentId);
            return Ok();
        }
    }
}
