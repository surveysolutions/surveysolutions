namespace WB.UI.Designer.Filters
{
    using System.Web.Mvc;

    using Main.Core;

    /// <summary>
    /// Filter which ensures that read layer is built before action is executed.
    /// </summary>
    public class RequiresReadLayerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            NcqrsInit.EnsureReadLayerIsBuilt();
        }
    }
}