using System.Web.Routing;

namespace WB.UI.Designer
{
    public class AttachmentRouteHandler : IRouteHandler
    {
        public System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AttachmentHandler(requestContext);
        }
    }
}