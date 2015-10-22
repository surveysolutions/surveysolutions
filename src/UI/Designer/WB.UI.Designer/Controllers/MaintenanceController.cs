﻿using System.Web.Mvc;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Controllers
{
    public class MaintenanceController : Controller
    {
        [NoTransaction]
        public ActionResult WaitForReadSideRebuild(string returnUrl)
        {
            return this.View(model: returnUrl);
        }
    }
}