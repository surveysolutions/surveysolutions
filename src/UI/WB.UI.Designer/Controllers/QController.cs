using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Internal;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Exceptions;

namespace WB.UI.Designer.Controllers;

[Route("/q")]
public class QController : Controller
{
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
    
    
    [Route("errors")]
    [HttpPost]
    public IActionResult LogError([FromBody] ClientErrorModel clientError)
    {
        try
        {
            var exception = new ClientException(clientError);

            exception.Log(HttpContext, category: "vue3");
            
            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}
