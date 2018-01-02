using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.WebTester.Controllers
{
    public class WebTesterController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebTesterController(IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
        }

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
