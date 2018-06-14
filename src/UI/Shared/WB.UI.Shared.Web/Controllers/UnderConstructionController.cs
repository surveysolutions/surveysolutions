﻿using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Resources;

namespace WB.UI.Shared.Web.Controllers
{

    public class UnderConstructionController : Controller
    {
        public class UnderConstructionModel
        {
            public string Title { get; set; }
            public string Message { get; set; }
        }

        public ActionResult Index()
        {
            var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

            var model = new UnderConstructionModel()
            {
                Title = UnderConstruction.UnderConstructionTitle,
                Message = status.Message ?? UnderConstruction.ServerInitializing
            };

            return View(model);
        }
    }
}
