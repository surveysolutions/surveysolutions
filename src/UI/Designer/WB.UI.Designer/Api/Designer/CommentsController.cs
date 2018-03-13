using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Comments;
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
        private readonly ICommentsFactory commentsFactory;

        public CommentsController(ICommentsFactory commentsFactory)
        {
            this.commentsFactory = commentsFactory;
        }

        [HttpGet]
        [CamelCase]
        [Route("entity/{itemId:Guid}/comments")]
        public List<CommentView> Get(Guid id, Guid itemId)
        {
            return this.commentsFactory.LoadCommentsForEntity(id, itemId);
        }

        [HttpPost]
        [CamelCase]
        [Route("entity/{itemId:Guid}/addComment")]
        public HttpResponseMessage AddComment(Guid id, AddCommentModel comment)
        {
            return this.Request.CreateResponse(new JsonResponseResult());
        }

        [HttpPatch]
        [CamelCase]
        [Route("comment/resolve/{commentdId:Guid}")]
        public HttpResponseMessage ResolveComment(Guid id, Guid commentdId)
        {
            return this.Request.CreateResponse(new JsonResponseResult());
        }
    }
}
