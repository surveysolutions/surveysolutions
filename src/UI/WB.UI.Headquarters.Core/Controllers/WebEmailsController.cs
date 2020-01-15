using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Controllers
{
    public class WebEmailsController : Controller
    {
        private readonly IPlainKeyValueStorage<EmailParameters> emailParamsStorage;

        public WebEmailsController(IPlainKeyValueStorage<EmailParameters> emailParamsStorage)
        {
            this.emailParamsStorage = emailParamsStorage;
        }

        // GET: WebEmail
        public IActionResult Html(string id)
        {
            var emailParams = emailParamsStorage.GetById(id);
            if (emailParams.Id == null)
                emailParams.Id = id;
            return View("EmailHtml", emailParams);
        }

        //public ActionResult Text(string id)
        //{
        //    var emailParams = emailParamsStorage.GetById(id);
        //    ViewEngineResult viewEngineResult = null;
            
        //    viewEngineResult = ViewEngines.Engines.FindView(this.ControllerContext, "EmailText", null);
      
        //    if (viewEngineResult == null)
        //        throw new FileNotFoundException("View cannot be found.");
      
        //    // get the view and attach the model to view data
        //    var view = viewEngineResult.View;
        //    ControllerContext.Controller.ViewData.Model = emailParams;
      
        //    string result = null;
      
        //    using (var sw = new StringWriter())
        //    {
        //        var ctx = new ViewContext(this.ControllerContext, view,
        //            this.ControllerContext.Controller.ViewData,
        //            this.ControllerContext.Controller.TempData,
        //            sw);
        //        view.Render(ctx, sw);
        //        result = sw.ToString();
        //    }
      
        //    return Content(result);
        //}
    }
}
