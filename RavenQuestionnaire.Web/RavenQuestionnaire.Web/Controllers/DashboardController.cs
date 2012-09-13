using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBagManager _bagManager;
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;

        public DashboardController(IViewRepository viewRepository,
                                               IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            
            this.viewRepository = viewRepository;
            _bagManager = bagManager;
            _globalProvider = globalProvider;
        }
        public ActionResult Index()
        {
           /* var model = new CQGroupedBrowseView(0, 1, 2, new List<CQGroupItem>(){ 
                new CQGroupItem(0,1,3, new List<CompleteQuestionnaireBrowseItem>()
                                           {
                                               new CompleteQuestionnaireBrowseItem("1","t1", DateTime.Now.AddDays(-1), DateTime.Now, new SurveyStatus("1","Initial"), 15,0,_globalProvider.GetCurrentUser() ),
                                               new CompleteQuestionnaireBrowseItem("2","t2", DateTime.Now.AddDays(-2), DateTime.Now.AddHours(-1), new SurveyStatus("2","Complete"),15,15, _globalProvider.GetCurrentUser() ),
                                               new CompleteQuestionnaireBrowseItem("3","t4", DateTime.Now.AddDays(-3), DateTime.Now.AddMinutes(-3), new SurveyStatus("3","Error"),15,13, _globalProvider.GetCurrentUser() )
                                           },"Uganda","1" ),
                new CQGroupItem(0,1,1, new List<CompleteQuestionnaireBrowseItem>()
                                           {
                                                
                                               new CompleteQuestionnaireBrowseItem("4","t4", DateTime.Now.AddDays(-4), DateTime.Now.AddYears(1), new SurveyStatus("1","Initial"), 0,0,_globalProvider.GetCurrentUser() )
                                           },"Buissines","2" )
            });*/
            var model =
                viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(new CQGroupedBrowseInputModel());
            return View(model);
        }

    }
}
