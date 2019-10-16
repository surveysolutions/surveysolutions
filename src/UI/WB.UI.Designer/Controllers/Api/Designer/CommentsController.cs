﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    [ResponseCache(NoStore = true)]
    [Route("questionnaire/{id}")]
    public class CommentsController : Controller
    {
        private readonly ICommentsService commentsService;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly DesignerDbContext dbContext;
        private readonly UserManager<DesignerIdentityUser> users;

        public CommentsController(
            ICommentsService commentsService, 
            IQuestionnaireViewFactory questionnaireViewFactory,
            DesignerDbContext dbContext,
            UserManager<DesignerIdentityUser> users)
        {
            this.commentsService = commentsService;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.dbContext = dbContext;
            this.users = users;
        }

        [HttpGet]
        [Route("commentThreads")]
        public List<CommentThread> commentThreads(QuestionnaireRevision id)
        {
            bool hasAccess = User.IsAdmin() || 
                             this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id.QuestionnaireId, this.User.GetId());

            return hasAccess ? this.commentsService.LoadCommentThreads(id.QuestionnaireId) : new List<CommentThread>();
        }

        [HttpGet]
        [Route("entity/{itemId:Guid}/comments")]
        public async Task<List<CommentView>> Get(QuestionnaireRevision id, Guid itemId)
        {
            bool hasAccess = User.IsAdmin() 
                || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id.QuestionnaireId, User.GetId());

            return hasAccess 
                ? await this.commentsService.LoadCommentsForEntity(id.QuestionnaireId, itemId) 
                : new List<CommentView>();
        }

        [HttpPost]
        [Route("entity/addComment")]
        public async Task<IActionResult> PostComment(QuestionnaireRevision id, [FromBody]AddCommentModel commentModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    Error = string.Join(", ", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage))
                });
            }
            bool hasAccess = User.IsAdmin() 
                || this.questionnaireViewFactory.HasUserAccessToRevertQuestionnaire(id.QuestionnaireId, User.GetId());

            hasAccess = hasAccess && id.Revision == null;

            if (!hasAccess)
            {
                return Json(new
                {
                    Error = "Access denied"
                });
            }

            var user = await this.users.GetUserAsync(User);

            commentsService.PostComment(commentModel.Id, 
                commentModel.QuestionnaireId,
                commentModel.EntityId, 
                commentModel.Comment, 
                user.UserName,
                user.Email);

            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch]
        [Route("comment/resolve/{commentId:Guid}")]
        public async Task<IActionResult> ResolveComment(Guid id, Guid commentId)
        {

            await commentsService.ResolveCommentAsync(commentId);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("comment/{commentId:Guid}")]
        public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
        {
            await commentsService.DeleteCommentAsync(commentId);
            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
