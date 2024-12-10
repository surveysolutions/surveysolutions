using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Vite.Extensions.AspNetCore;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Filters;

namespace WB.UI.Designer.Controllers;

[Route("/q")]
public class QController : QControllerBase
{
    public QController(ITagHelperComponentManager tagHelperComponentManager,
        IWebHostEnvironment webHost,
        IOptions<ViteTagOptions> options,
        IMemoryCache memoryCache,
        DesignerDbContext dbContext,
        IQuestionnaireViewFactory questionnaireViewFactory)
        : base(dbContext, questionnaireViewFactory)
    {
        ViteTagHelperComponent.RegisterIfRequired(tagHelperComponentManager, webHost, options, memoryCache);
    }
    
    [QuestionnairePermissions]
    [Route("details/{id}")]
    [Route("details/{id}/chapter/{chapterId}/{entityType}/{entityId}")]
    public IActionResult Details(QuestionnaireRevision? id, Guid? chapterId, string entityType, Guid? entityid)
    {
        if(id == null)
            return this.RedirectToAction("Index", "QuestionnaireList");

        var questionnaire = questionnaireViewFactory.Load(id);
        if (questionnaire == null || questionnaire.Source.IsDeleted)
            return NotFound();

        if (ShouldRedirectToOriginalId(id))
        {
            return RedirectToAction("Details", new RouteValueDictionary
            {
                { "id", id.OriginalQuestionnaireId.FormatGuid() }, { "chapterId", chapterId?.FormatGuid() }, { "entityType", entityType }, { "entityid", entityid?.FormatGuid() }
            });
        }

        return (User.IsAdmin() || this.UserHasAccessToEditOrViewQuestionnaire(id))
            ? this.View("Vue")
            : this.LackOfPermits();
    }
    
    
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
