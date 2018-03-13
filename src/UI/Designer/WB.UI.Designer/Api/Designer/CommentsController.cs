using System.Collections.Generic;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    [ApiNoCache]
    public class CommentsController : ApiController
    {
        private readonly ICommentsFactory commentsFactory;

        public CommentsController(ICommentsFactory commentsFactory)
        {
            this.commentsFactory = commentsFactory;
        }

        [HttpGet]
        [CamelCase]
        public List<CommentView> Get(string id, string itemId)
        {
            return this.commentsFactory.LoadCommentsForEntity(id);
        }
    }
}
