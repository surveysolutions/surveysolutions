using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WB.UI.WebTester.Controllers
{
    public class WebTesterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }

    public class ApiTestModel
    {
        public Guid Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public int NumOfTranslations { get; set; }
        public List<string> Attaches { get; set; }
        public string Title { get; set; }
    }
}
