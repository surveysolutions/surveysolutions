using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
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
    
    [QuestionnairePermissions]
    [Route("details/{id}/entity/{entityId}")]
    public IActionResult Details(QuestionnaireRevision? id, Guid? entityid)
    {
        if(id == null)
            return this.RedirectToAction("Index", "QuestionnaireList");

        if (entityid == null)
            return NotFound();

        var questionnaire = questionnaireViewFactory.Load(id);
        if (questionnaire == null || questionnaire.Source.IsDeleted)
            return NotFound();

        var entity = questionnaire.Source.Find<IComposite>(entityid.Value);
        if (entity == null)
            return NotFound();
            
        Guid? chapterId = GetChapterId(entity);
        string entityType = entity switch
        {
            IQuestion => "question",
            StaticText => "statictext",
            Variable => "variable",
            Group => "group",
            Roster => "roster",
            _ => "unknown"
        };
        
        return RedirectToAction("Details", new RouteValueDictionary
        {
            { "id", id }, { "chapterId", chapterId?.FormatGuid() }, { "entityType", entityType }, { "entityid", entityid?.FormatGuid() }
        });
    }
    
    private Guid? GetChapterId(IComposite entity)
    {
        var currentParent = entity.GetParent() as IGroup;
        List<Guid> parentIds = new List<Guid>();
        
        while (currentParent != null)
        {
            parentIds.Add(currentParent.PublicKey);
            currentParent = currentParent.GetParent() as IGroup;
        }

        if (parentIds.Count <= 1)
            return null;
        
        return parentIds[^2];
    }
    
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
